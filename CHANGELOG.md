# Änderungsprotokoll

## [0.2.3] - 2026-10-16

- Doppelklick auf Noxy öffnet direkt die Einstellungen; Rechtsklick öffnet weiterhin das Menü.
- Fehler beim Öffnen der Einstellungen zeigen jetzt eine verständliche Meldung.
- Monitorgrenzen werden relativ zur tatsächlichen Position und Skalierung des jeweiligen Bildschirms berechnet, damit Noxy nach dem Verschieben auf anderen Monitoren nicht zurückspringt.
- Vor dem Beenden wartet Noxy auf eine Bestätigung, dass der Update-Helfer wirklich läuft. Bei einem Startfehler bleibt Noxy geöffnet und zeigt den Pfad zum Fehlerprotokoll.

## [0.2.2] - 2026-10-16

- Update-Helfer überarbeitet: Er wartet sicher auf das Schließen von Noxy, ersetzt die Programmdatei, startet Noxy erneut und stellt bei einem Fehler die vorherige Version wieder her.
- Update-Fehler werden lokal unter `%LOCALAPPDATA%\Noxy\update.log` protokolliert.
- Noxy weist mit seiner Sprechblase auf verfügbare Updates hin.
- Der Update-Knopf wird bei einem verfügbaren Update grün hervorgehoben.
- Die Versionshinweise des neuesten GitHub-Releases werden in den Einstellungen angezeigt.

