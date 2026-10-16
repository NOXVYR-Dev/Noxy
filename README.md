# Noxy – Desktop-Pet

Noxy ist ein eigenständiges Desktop-Pet für Windows 10/11. Auf dem Ziel-PC muss .NET nicht installiert sein.

## Installation und Updates

Lade `Noxy-Setup-x64.exe` aus dem neuesten GitHub-Release herunter und starte die Datei. Noxy wird nur für dein Windows-Benutzerkonto unter `%LOCALAPPDATA%\Programs\Noxy` installiert; Admin-Rechte und eine separate .NET-Installation sind nicht nötig. Im Setup kannst du den Autostart bei der Windows-Anmeldung und eine Desktop-Verknüpfung auswählen.

Über einen Rechtsklick auf Noxy öffnest du **Einstellungen**. Dort kannst du **Beim Windows-Anmelden automatisch starten** jederzeit ein- oder ausschalten. Noxy prüft alle fünf Minuten auf Updates. Wenn eine neue Version bereitsteht, klicke auf **Jetzt aktualisieren**. Noxy lädt das Paket herunter, prüft die SHA-256-Prüfsumme, installiert das Update und startet sich neu. Das ZIP für die portable Nutzung bleibt ebenfalls verfügbar.

## Eine neue Version veröffentlichen

1. Änderungen am Quellcode auf `main` einchecken.
2. In `version.json` die Versionsnummer im Format `Hauptversion.Nebenversion.Patch` erhöhen.
3. Den Commit nach `main` übertragen.

GitHub Actions baut daraufhin die eigenständige Windows-App und den Windows-Setup-Assistenten, erstellt Prüfsummen und veröffentlicht ein GitHub-Release. Noxy erkennt dieses Release automatisch.

## Lokal bauen

Mit installiertem .NET 8 SDK `BUILD-WINDOWS.bat` starten. Die App erscheint unter `publish/Noxy.exe`.

Einstellungen liegen lokal unter `%LOCALAPPDATA%\Noxy`. Vorhandene Einstellungen der Noxvyr-Alpha werden beim ersten Start übernommen.

