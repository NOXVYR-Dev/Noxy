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

            var scriptPath = Path.Combine(stage, "apply-update.cmd");
            await File.WriteAllTextAsync(scriptPath, BuildUpdateScript(appPath, newApp, stage, Environment.ProcessId));
            var start = new ProcessStartInfo("cmd.exe")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            start.ArgumentList.Add("/c");
            start.ArgumentList.Add($"\"{scriptPath}\"");
            if (Process.Start(start) is null)
                throw new IOException("Der Update-Vorgang konnte nicht gestartet werden.");
            Status = "Update wird installiert und Noxy neu gestartet …";
            Changed?.Invoke();
            System.Windows.Application.Current.Shutdown();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException or UnauthorizedAccessException or InvalidDataException)
        {
            Status = $"Update fehlgeschlagen: {ex.Message}";
            IsBusy = false;
            Changed?.Invoke();
            if (stage is not null)
            {
                try { Directory.Delete(stage, true); } catch { }
            }
        }
    }

    private static string BuildUpdateScript(string appPath, string newAppPath, string stage, int processId)
    {
        static string Escape(string value) => value.Replace("%", "%%");
        return $"""
@echo off
setlocal DisableDelayedExpansion
set "APP={Escape(appPath)}"
set "NEW={Escape(newAppPath)}"
set "STAGE={Escape(stage)}"
set "PID={processId}"
for /L %%i in (1,1,120) do (
  tasklist /FI "PID eq %PID%" /NH | findstr /R /C:"[ ]%PID%[ ]" >nul
  if errorlevel 1 goto replace
  timeout /t 1 /nobreak >nul
)
:replace
if not exist "%NEW%" goto failed
copy /y "%APP%" "%APP%.bak" >nul
copy /y "%NEW%" "%APP%" >nul
if errorlevel 1 goto rollback
start "" "%APP%"
timeout /t 4 /nobreak >nul
del "%APP%.bak" >nul 2>nul
rd /s /q "%STAGE%" >nul 2>nul
del "%~f0" >nul 2>nul
exit /b 0
:rollback
if exist "%APP%.bak" copy /y "%APP%.bak" "%APP%" >nul
start "" "%APP%"
:failed
timeout /t 3 /nobreak >nul
del "%~f0" >nul 2>nul
""";
    }

    private static string GetCurrentVersion()
    {
        var version = typeof(UpdateService).Assembly.GetName().Version ?? new Version(0, 1, 3);
        return $"{version.Major}.{version.Minor}.{Math.Max(0, version.Build)}";
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("NoxyDesktopPet", "0.1.3"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        return client;
    }
}

