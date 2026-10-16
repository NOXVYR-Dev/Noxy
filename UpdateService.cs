using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Threading;

namespace Noxy;

public sealed class UpdateService
{
    private const string Repository = "NOXVYR-Dev/Noxy";
    private const string PackageName = "Noxy-win-x64.zip";
    private static readonly HttpClient Client = CreateClient();
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromMinutes(5) };
    private readonly SemaphoreSlim checkLock = new(1, 1);
    private string? packageUrl;
    private string? checksumUrl;

    public event Action? Changed;
    public string Status { get; private set; } = "Suche nach Updates …";
    public string ChangeLog { get; private set; } = "Es wurden keine Versionshinweise veröffentlicht.";
    public bool UpdateAvailable { get; private set; }
    public bool IsBusy { get; private set; }
    public string CurrentVersion => GetCurrentVersion();

    public UpdateService()
    {
        timer.Tick += async (_, _) => await CheckAsync();
        timer.Start();
        _ = CheckAsync();
    }

    public async Task CheckAsync()
    {
        if (!await checkLock.WaitAsync(0)) return;
        IsBusy = true;
        Status = "Suche nach Updates …";
        Changed?.Invoke();
        try
        {
            using var response = await Client.GetAsync($"https://api.github.com/repos/{Repository}/releases/latest");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                UpdateAvailable = false;
                Status = "Noch kein GitHub-Release veröffentlicht.";
                return;
            }
            response.EnsureSuccessStatusCode();
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = json.RootElement;
            var tag = root.GetProperty("tag_name").GetString()?.TrimStart('v', 'V');
            if (!Version.TryParse(tag, out var latest))
                throw new InvalidDataException("Das Release hat keine gültige Versionsnummer.");

            ChangeLog = root.TryGetProperty("body", out var body) && !string.IsNullOrWhiteSpace(body.GetString())
                ? body.GetString()!.Trim()
                : "Es wurden keine Versionshinweise veröffentlicht.";

            packageUrl = null;
            checksumUrl = null;
            foreach (var asset in root.GetProperty("assets").EnumerateArray())
            {
                var name = asset.GetProperty("name").GetString();
                var url = asset.GetProperty("browser_download_url").GetString();
                if (name == PackageName) packageUrl = url;
                if (name == PackageName + ".sha256") checksumUrl = url;
            }

            UpdateAvailable = latest > Version.Parse(CurrentVersion);
            if (UpdateAvailable && packageUrl is not null && checksumUrl is not null)
                Status = $"Version {latest} verfügbar.";
            else if (UpdateAvailable)
            {
                UpdateAvailable = false;
                Status = "Release gefunden, aber das Update-Paket fehlt.";
            }
            else
                Status = $"Version {CurrentVersion} ist aktuell.";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or InvalidDataException)
        {
            Status = "Update-Prüfung fehlgeschlagen. Internetverbindung später erneut prüfen.";
        }
        finally
        {
            IsBusy = false;
            checkLock.Release();
            Changed?.Invoke();
        }
    }

    public async Task InstallUpdateAsync()
    {
        if (!UpdateAvailable || packageUrl is null || checksumUrl is null || IsBusy) return;
        IsBusy = true;
        Status = "Update wird heruntergeladen …";
        Changed?.Invoke();
        string? stage = null;
        Process? updaterProcess = null;
        try
        {
            var appPath = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(appPath) || !File.Exists(appPath))
                throw new IOException("Der Speicherort der App konnte nicht ermittelt werden.");

            var appDirectory = Path.GetDirectoryName(appPath)!;
            var writeCheck = Path.Combine(appDirectory, $".noxy-write-{Guid.NewGuid():N}.tmp");
            await File.WriteAllTextAsync(writeCheck, "ok");
            File.Delete(writeCheck);

            stage = Path.Combine(Path.GetTempPath(), $"Noxy-update-{Guid.NewGuid():N}");
            Directory.CreateDirectory(stage);
            var zipPath = Path.Combine(stage, PackageName);
            await File.WriteAllBytesAsync(zipPath, await Client.GetByteArrayAsync(packageUrl));
            var expectedHash = (await Client.GetStringAsync(checksumUrl)).Trim().Split(' ', '\t', '\r', '\n')[0];
            using var zipStream = File.OpenRead(zipPath);
            var actualHash = Convert.ToHexString(await SHA256.HashDataAsync(zipStream)).ToLowerInvariant();
            if (!string.Equals(expectedHash, actualHash, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Die Prüfsumme des Updates stimmt nicht überein.");

            ZipFile.ExtractToDirectory(zipPath, stage);
            var newApp = Path.Combine(stage, "Noxy.exe");
            if (!File.Exists(newApp)) throw new InvalidDataException("Im Update-Paket fehlt Noxy.exe.");

            var scriptPath = Path.Combine(Path.GetTempPath(), $"Noxy-apply-update-{Guid.NewGuid():N}.ps1");
            var readyPath = Path.Combine(stage, "updater.ready");
            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Noxy", "update.log");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            await File.WriteAllTextAsync(scriptPath, BuildUpdateScript());
            var start = new ProcessStartInfo("powershell.exe")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            start.ArgumentList.Add("-NoLogo");
            start.ArgumentList.Add("-NoProfile");
            start.ArgumentList.Add("-NonInteractive");
            start.ArgumentList.Add("-ExecutionPolicy");
            start.ArgumentList.Add("Bypass");
            start.ArgumentList.Add("-File");
            start.ArgumentList.Add(scriptPath);
            start.ArgumentList.Add("-AppPath");
            start.ArgumentList.Add(appPath);
            start.ArgumentList.Add("-NewAppPath");
            start.ArgumentList.Add(newApp);
            start.ArgumentList.Add("-StagePath");
            start.ArgumentList.Add(stage);
            start.ArgumentList.Add("-ReadyPath");
            start.ArgumentList.Add(readyPath);
            start.ArgumentList.Add("-ProcessIdToWait");
            start.ArgumentList.Add(Environment.ProcessId.ToString());
            start.ArgumentList.Add("-LogPath");
            start.ArgumentList.Add(logPath);
            updaterProcess = Process.Start(start);
            if (updaterProcess is null)
                throw new IOException("Der Update-Vorgang konnte nicht gestartet werden.");
            for (var i = 0; i < 200 && !File.Exists(readyPath) && !updaterProcess.HasExited; i++)
                await Task.Delay(100);
            if (!File.Exists(readyPath))
            {
                if (!updaterProcess.HasExited)
                {
                    try { updaterProcess.Kill(true); } catch { }
                }
                throw new IOException($"Der Update-Helfer hat nicht gestartet. Noxy bleibt geöffnet. Protokoll: {logPath}");
            }
            Status = "Update wird installiert. Noxy startet gleich neu …";
            Changed?.Invoke();
            System.Windows.Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            Status = $"Update fehlgeschlagen: {ex.Message}";
            IsBusy = false;
            Changed?.Invoke();
            if (stage is not null && (updaterProcess is null || updaterProcess.HasExited))
            {
                try { Directory.Delete(stage, true); } catch { }
            }
        }
    }

    private static string BuildUpdateScript() => """
param(
    [Parameter(Mandatory=$true)][string]$AppPath,
    [Parameter(Mandatory=$true)][string]$NewAppPath,
    [Parameter(Mandatory=$true)][string]$StagePath,
    [Parameter(Mandatory=$true)][string]$ReadyPath,
    [Parameter(Mandatory=$true)][int]$ProcessIdToWait,
    [Parameter(Mandatory=$true)][string]$LogPath
)
$ErrorActionPreference = 'Stop'
function Write-Log([string]$Message) {
    Add-Content -LiteralPath $LogPath -Value "$(Get-Date -Format o) $Message"
}
try {
    Write-Log 'Update helper started.'
    Set-Content -LiteralPath $ReadyPath -Value 'ready' -Encoding ascii
    $old = Get-Process -Id $ProcessIdToWait -ErrorAction SilentlyContinue
    if ($old -and -not $old.WaitForExit(120000)) { throw 'Noxy wurde nicht rechtzeitig beendet; die alte Version bleibt erhalten.' }
    if (-not (Test-Path -LiteralPath $NewAppPath)) { throw 'Die neue Noxy.exe fehlt im Update-Paket.' }
    $backupPath = "$AppPath.bak"
    $replacementPath = "$AppPath.new"
    Copy-Item -LiteralPath $NewAppPath -Destination $replacementPath -Force
    if (Test-Path -LiteralPath $AppPath) { Copy-Item -LiteralPath $AppPath -Destination $backupPath -Force }
    Move-Item -LiteralPath $replacementPath -Destination $AppPath -Force
    Write-Log 'Neue Programmdatei kopiert; Noxy startet neu.'
    Start-Process -FilePath $AppPath
    Start-Sleep -Seconds 3
    Remove-Item -LiteralPath $backupPath -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $StagePath -Recurse -Force -ErrorAction SilentlyContinue
    Write-Log 'Update abgeschlossen.'
} catch {
    Write-Log "Update fehlgeschlagen: $($_.Exception.Message)"
    try {
        if (Test-Path -LiteralPath "$AppPath.bak") {
            Copy-Item -LiteralPath "$AppPath.bak" -Destination $AppPath -Force
            Start-Process -FilePath $AppPath
            Write-Log 'Vorherige Version wiederhergestellt und gestartet.'
        } elseif (-not (Get-Process -Id $ProcessIdToWait -ErrorAction SilentlyContinue) -and (Test-Path -LiteralPath $AppPath)) {
            Start-Process -FilePath $AppPath
            Write-Log 'Vorhandene Version nach Updatefehler erneut gestartet.'
        }
    } catch { Write-Log "Wiederherstellung fehlgeschlagen: $($_.Exception.Message)" }
}
""";

    private static string GetCurrentVersion()
    {
        var version = typeof(UpdateService).Assembly.GetName().Version ?? new Version(0, 1, 3);
        return $"{version.Major}.{version.Minor}.{Math.Max(0, version.Build)}";
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("NoxyDesktopPet", "0.2.4"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        return client;
    }
}

