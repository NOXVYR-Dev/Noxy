# Noxy – Desktop-Pet

Noxy ist ein eigenständiges Desktop-Pet für Windows 10/11. Auf dem Ziel-PC muss .NET nicht installiert sein.

## Download und Updates

Lade `Noxy-win-x64.zip` aus dem neuesten GitHub-Release herunter, entpacke es in einen Ordner, in dem du Dateien ändern darfst, und starte `Noxy.exe`. Über einen Rechtsklick auf Noxy öffnest du **Einstellungen**. Dort prüft Noxy alle fünf Minuten nach Updates. Wenn eine neue Version bereitsteht, klicke auf **Jetzt aktualisieren**. Noxy lädt das Paket herunter, prüft die SHA-256-Prüfsumme, installiert das Update und startet sich neu.

## Eine neue Version veröffentlichen

1. Änderungen am Quellcode auf `main` einchecken.
2. In `version.json` die Versionsnummer im Format `Hauptversion.Nebenversion.Patch` erhöhen.
3. Den Commit nach `main` übertragen.

GitHub Actions baut daraufhin die eigenständige Windows-App, erstellt das ZIP samt SHA-256-Datei und veröffentlicht ein GitHub-Release. Noxy erkennt dieses Release automatisch.

## Lokal bauen

Mit installiertem .NET 8 SDK `BUILD-WINDOWS.bat` starten. Die App erscheint unter `publish/Noxy.exe`.

Einstellungen liegen lokal unter `%LOCALAPPDATA%\Noxy`. Vorhandene Einstellungen der Noxvyr-Alpha werden beim ersten Start übernommen.

