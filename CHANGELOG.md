# Änderungsprotokoll

## [0.3.5] - 2026-10-16

- Wenn Noxy beim Festhalten schnell hin und her geschüttelt wird, wird er für kurze Zeit sichtbar wütend und kommentiert das frech.
- Ein erkanntes Schütteln löst beim Loslassen keinen versehentlichen Flummi-Wurf aus; kleine Mausbewegungen reichen dafür nicht.

## [0.3.4] - 2026-10-16

- Noxy teilt etwa alle vier bis sechs Minuten CPU-, GPU- und RAM-Auslastung mit, sofern PC-Statusmeldungen aktiviert sind.
- CPU und GPU werden mit Windows-Leistungsdaten ermittelt. Nicht verfügbare GPU-Werte werden als solche angezeigt; Sensorwerte verlassen den PC nicht.

## [0.3.3] - 2026-10-16

- Bildschirmgrenzen werden über WPFs DPI-Umrechnung aus den tatsächlichen Arbeitsbereich-Ecken des Monitors berechnet. Noxy springt beim Loslassen in einer Bildschirmecke nicht mehr durch vermischte Pixel- und DIP-Maße weg.

## [0.3.2] - 2026-10-16

- Bewegung und Flummi-Flug werden mit etwa 60 statt 30 Bildern pro Sekunde aktualisiert.
- Sanftere Würfe: Spitzen beim Loslassen sind begrenzt und die Schwerkraft ist etwas niedriger. Noxy bewegt sich dadurch weicher über den Bildschirm und bleibt beim Fliegen etwas länger in der Luft.

## [0.3.1] - 2026-10-16

- Flummi-Wurf aus der echten Ziehgeschwindigkeit: Flug in Zugrichtung, Schwerkraft, Rückpraller an allen vier Monitorrändern und gedämpftes Ausrollen. Langsames Ziehen und Festhalten bleiben normales Platzieren.
- Zeitbasierte Spaziergänge und sichtbare Schritt-, Körper-, Gesichts- und Schlafanimationen. Körper und Gesicht bleiben beim Wippen zusammen.
- Korrigierte Bewegungsgrenzen; die Fensterbreite wird nicht mehr zweimal abgezogen. Startpositionen außerhalb des verfügbaren Monitors werden zurückgeholt.
- Ruhe- und Schlafmodus stoppen Flug und Bewegung. Kontextmenü und Einstellungen pausieren das Pet.
- Programme und Vollbildzustand werden einmal pro Sekunde statt in jedem Animationsbild abgefragt.
- Einstellungen öffnen auch bei verweigertem Registry-Lesezugriff; unveränderter Autostart wird beim Speichern nicht neu geschrieben.
- Automatische Bewegungs-, Physik- und Windows-WPF-Integrationstests hinzugefügt.

## [0.3.0] - 2026-10-16

- Noxy dreht beim selbstständigen Laufen an den Rändern des aktuellen Monitors um und läuft nicht mehr von selbst auf einen anderen Bildschirm.
- Mit gedrückter linker Maustaste kann Noxy weiterhin auf jeden verbundenen Monitor gezogen werden.

## [0.2.9] - 2026-10-16

- Die Darstellung des Pets bleibt bei der bisherigen Form; der Schriftzug „Noxy“ auf dem Pet wurde entfernt.
- Änderungen an der Anzahl von ein bis drei Pets werden nach dem Speichern sofort übernommen.
- Eigenständiges kleines Pet-Symbol für Desktop- und Startmenü-Verknüpfungen; Updates erneuern auch das Verknüpfungssymbol.
- Die Sprechblase übernimmt Rand- und Textakzentfarbe des jeweiligen Pets.
- Treffen sich zwei Pets, begrüßt das kleinere sie und das andere antwortet nach einem kurzen Moment mit einem frechen Spruch.

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

