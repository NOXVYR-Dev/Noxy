# Noxy – Desktop-Pet

## Windows-Version 0.3.5: Wütend bei Schütteln

Version 0.3.5 erscheint als reguläres GitHub-Release. Bereits installierte Noxys erkennen das Update und zeigen einen Hinweis an. Klicke in den Einstellungen auf **Jetzt aktualisieren**. Alternativ stehen dort die direkte Setup-Datei und das portable ZIP unter **Releases** bereit.

- **Schütteln:** Noxy wird beim schnellen Hin-und-her-Bewegen während des Festhaltens für kurze Zeit wütend, zeigt eine veränderte Mimik und gibt einen frechen Kommentar ab. Der Schütteltest filtert kleine Mausbewegungen heraus und verhindert einen versehentlichen Wurf beim Loslassen.

- **Herumlaufen / Zappelig:** selbstständige Spaziergänge mit Pausen, sichtbaren Schritten, Wippen, Blickrichtung und Blinzeln.
- **Flummi-Wurf:** Noxy mit der linken Maustaste schnell ziehen und während der Bewegung loslassen. Er übernimmt Richtung und Tempo, fliegt mit Schwerkraft weiter und prallt an allen vier Rändern des aktuellen Monitor-Arbeitsbereichs zurück. Rückpraller verlieren Energie; spätestens nach fünf Sekunden endet der Flug. Danach gilt wieder der gewählte Bewegungsmodus.
- Langsam ziehen oder vor dem Loslassen kurz stillhalten platziert Noxy ohne Wurf. Erneutes Festhalten stoppt einen laufenden Flug.
- **Statisch:** bleibt stehen und atmet/blinzelt; absichtliches Werfen ist möglich. **Schlafmodus:** geschlossene Augen, Zs, keine Spaziergänge oder Würfe. OBS-/Vollbild-Ruhemodus stoppt Bewegung und Sprechblasen wie bisher.
- Ein geöffnetes Kontextmenü oder Einstellungsfenster pausiert die Bewegung. Doppelklick öffnet weiterhin die Einstellungen.
- Der Flug wird nun mit etwa 60 Bildern pro Sekunde und einer geringeren Höchstgeschwindigkeit berechnet, damit Würfe und Rückpraller weicher wirken.
- Die Arbeitsfläche des Monitors wird DPI-bewusst aus den echten Bildschirmecken umgerechnet. Beim Ziehen und Loslassen in einer Ecke bleibt Noxy dadurch an der präzisen Position.
- Wenn **PC-Statusmeldungen aktiv** ist, nennt Noxy etwa alle vier bis sechs Minuten CPU-, GPU- und RAM-Auslastung. Die Messwerte werden lokal gelesen. GPU-Werte verwenden den stärksten GPU-Engine, den Windows-Treiber meldet; auf manchen Geräten ist diese Messung nicht verfügbar.
- Korrigierte Bildschirmgrenzen: Die Fensterbreite wird nicht mehr doppelt abgezogen. Wie in Version 0.3.0 bleibt Noxy beim Laufen auf dem aktuellen Monitor; manuelles Ziehen auf andere Monitore bleibt möglich.

Diese Alpha ist eine separate Fortsetzung der Version 0.3.0. Die portable Alpha verwendet dieselben lokalen Einstellungen und dasselbe Instanzlimit wie eine bereits installierte App. Vor dem Test eine bereits laufende Noxy-Version über deren Menü beenden.

Automatische Prüfungen: `dotnet run --project ../Noxy.Tests/Noxy.Tests.csproj -c Release -- ../../outputs`. Sie prüfen Bewegung, Wurfgeschwindigkeit, Rückpraller, Ruhemodi, 250 zufällige Flugbahnen und das echte WPF-Fenster einschließlich der Verbindung zwischen Positionsereignissen und Wurfstart. KI-Chat und Einstellungen werden geöffnet; CPU-, GPU- und RAM-Abfragen sowie die PC-Statusmeldung werden geprüft. Bezahlte Chat-Anfragen, Wetter, Updates, Installation und Autostart werden nicht Ende zu Ende getestet. Ein manueller Mauswurf sowie gemischte DPI-Skalierungen auf mehreren physischen Monitoren bleiben zur Abnahme am Ziel-PC.

Einstellungen öffnen sich nun auch bei gesperrtem Windows-Autostartzugriff; in diesem Fall ist nur die Autostart-Option deaktiviert. Unveränderte Autostart-Einstellungen werden beim Speichern nicht erneut in die Registry geschrieben.

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

In den Einstellungen kannst du außerdem Kommentare bei Programmwechsel aktivieren. Noxy erkennt dafür ausschließlich den Namen des aktiven Programms (nach 3 Sekunden) und kommentiert höchstens alle 3 Minuten. Fenstertitel und Inhalte werden nicht gelesen; es werden keine Programmnamen übertragen.

Optional kann Noxy einmal täglich einen Wetter-Spruch anzeigen. Die Funktion ist standardmäßig ausgeschaltet. Bei Aktivierung wird die öffentliche IP an ipwho.is gesendet, um eine ungefähre Stadt und Koordinaten zu bestimmen; die Koordinaten werden für die Wetterdaten an Open-Meteo übermittelt. Die Stadtbestimmung kann ungenau sein. Wetterdaten von Open-Meteo, lizenziert unter CC BY 4.0.

Unter **Noxys auf dem Desktop** stellst du ein, ob höchstens ein, zwei oder drei Noxys gleichzeitig laufen dürfen. Klicke zum Starten eines weiteren Noxy erneut auf das Desktop-Symbol. Exemplare oberhalb eines verringerten Limits schließen sich wie bisher. Jedes Noxy lässt sich im RGB-Farbmenü einzeln einfärben; treffen sie sich auf dem Desktop, tauschen sie einen kurzen Spruch aus.


Noxy dreht beim Herumlaufen an den Rändern des aktuellen Monitors um. Mit gedrückter linker Maustaste kannst du ihn auf einen anderen Monitor ziehen.

Einstellungen liegen lokal unter `%LOCALAPPDATA%\Noxy`. Vorhandene Einstellungen der Noxvyr-Alpha werden beim ersten Start übernommen.

