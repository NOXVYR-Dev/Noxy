# Noxy – Desktop-Pet

Noxy ist ein eigenständiges Desktop-Pet für Windows 10/11. Auf dem Ziel-PC muss .NET nicht installiert sein.

## Installation und Updates

Lade `Noxy-Setup-x64.exe` aus dem neuesten GitHub-Release herunter und starte die Datei. Noxy wird nur für dein Windows-Benutzerkonto unter `%LOCALAPPDATA%\Programs\Noxy` installiert; Admin-Rechte und eine separate .NET-Installation sind nicht nötig. Im Setup kannst du den Autostart bei der Windows-Anmeldung und eine Desktop-Verknüpfung auswählen.

Über einen Rechtsklick auf Noxy öffnest du **Einstellungen**. Ein Doppelklick auf Noxy öffnet die Einstellungen ebenfalls; ein einfacher Linksklick dient zum Ziehen. Dort kannst du **Beim Windows-Anmelden automatisch starten** jederzeit ein- oder ausschalten. Noxy prüft alle fünf Minuten auf Updates und zeigt ein verfügbares Update in seiner Sprechblase sowie mit einem grünen Update-Knopf an. Unter **Änderungen der neuesten Version** stehen die Versionshinweise. Klicke auf **Jetzt aktualisieren**: Noxy lädt das Paket herunter, prüft die SHA-256-Prüfsumme, installiert das Update und startet sich neu. Bei einem Startfehler bleibt Noxy geöffnet; Details stehen unter `%LOCALAPPDATA%\Noxy\update.log`. Das ZIP für die portable Nutzung bleibt ebenfalls verfügbar.

## Eine neue Version veröffentlichen

1. Änderungen am Quellcode auf `main` einchecken.
2. In `version.json` die Versionsnummer im Format `Hauptversion.Nebenversion.Patch` erhöhen.
3. Den Commit nach `main` übertragen.

GitHub Actions baut daraufhin die eigenständige Windows-App und den Windows-Setup-Assistenten, erstellt Prüfsummen und veröffentlicht ein GitHub-Release. Noxy erkennt dieses Release automatisch.

## Lokal bauen

Mit installiertem .NET 8 SDK `BUILD-WINDOWS.bat` starten. Die App erscheint unter `publish/Noxy.exe`.

Noxy kann beim Herumlaufen über gemeinsame Kanten direkt angrenzender Monitore wechseln. Am äußeren Rand des Monitorverbunds dreht es um. Die Monitore müssen in den Windows-Anzeigeeinstellungen passend angeordnet sein.

Einstellungen liegen lokal unter `%LOCALAPPDATA%\Noxy`. Vorhandene Einstellungen der Noxvyr-Alpha werden beim ersten Start übernommen.

