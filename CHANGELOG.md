# Änderungsprotokoll

## [0.2.8] - 2026-10-16

- Noxy verhindert Mehrfachstarts oberhalb des eingestellten Limits; wahlweise sind ein, zwei oder drei gleichzeitig gestartete Noxys möglich.
- Individuelle RGB-Farben für jedes Noxy und eigene Positionen pro Instanz.
- Treffen sich Noxys auf dem Desktop, tauschen sie einen kurzen Spruch aus.
- Neues Noxy-Symbol für Programm und Desktop-Verknüpfung.

## [0.2.7] - 2026-10-16

- Optionaler täglicher Wetter-Spruch mit automatischer, ungefährer Stadtbestimmung über die öffentliche IP.
- Die Funktion ist standardmäßig ausgeschaltet und kann in den Einstellungen aktiviert werden. Wetterabfrage höchstens einmal täglich.
- Transparente Hinweise zu IP-/Standortdaten und Quellen stehen direkt bei der Option; Open-Meteo wird genannt.

## [0.2.6] - 2026-10-16

- Auswahl im Bewegungsmenü und deaktivierte Update-Schaltfläche haben jetzt gut lesbaren Kontrast.
- 30 zusätzliche freche Sprüche für mehr Abwechslung.

## [0.2.5] - 2026-10-16

- Noxy erkennt auf Wunsch das aktive Vordergrund-Programm und kommentiert Programmwechsel.
- Der Programmname muss mindestens 3 Sekunden aktiv sein; Kommentare erscheinen höchstens alle 3 Minuten.
- Die Erkennung bleibt lokal und liest weder Fenstertitel noch Fensterinhalte.


## [0.2.4] - 2026-10-16

- Noxy kann beim Herumlaufen von einem Monitor auf einen direkt angrenzenden Monitor weiterlaufen.
- An äußeren Bildschirmkanten dreht Noxy um; die Position berücksichtigt die Anordnung und Versetzung der Monitore.


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

