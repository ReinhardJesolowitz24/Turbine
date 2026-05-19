# Turbine

Ein kostenloses, quelloffenes Werkzeug zur Datei-Verschlüsselung für den
persönlichen Gebrauch. Ursprünglich 2012 entwickelt von **\<DEIN VOLLER NAME\>**,
um persönliche Daten — Passwörter, Login-Dateien, sensible Dokumente — vor
zufälligem Zugriff zu schützen (z.B. bei einem verlorenen USB-Stick).

> *English version: see [README.md](README.md).*

---

## Was es macht

Turbine verschlüsselt Dateien mit einem selbst entwickelten Stream-Cipher,
der einen 1280-Bit-internen Zustand verwendet, verteilt auf vier parallele
"Getriebe"-Gruppen (daher der Name). Die verschlüsselte Ausgabe wird in
einem BMP-Container verpackt — das macht die Datei für naive Inhaltsfilter
unverdächtig und unauffällig auf der Festplatte.

- **Passwort-basierte Verschlüsselung** (6 bis 1024 Zeichen)
- **PBKDF2-SHA512 Schlüssel-Ableitung** (V2, 1.200.000 Iterationen) —
  macht Brute-Force-Angriffe ~1,2 Millionen mal teurer bei schwachen
  Passwörtern. Alte V1-Dateien bleiben weiterhin lesbar.
- **Kryptografisch zufälliger IV** für jede Datei (keine zwei Outputs identisch)
- **Stop-Go-getakteter Schieberegister-Cipher**, strukturell verwandt mit Trivium
- **CFB-ähnliche Block-Verkettung** — jede Manipulation am Chiffretext
  zerstört alle nachfolgenden Klartext-Bytes (faktische
  Manipulationserkennung ohne expliziten MAC)
- **Keine Hintertür, kein Generalschlüssel** — durch Quellcode-Lektüre überprüfbar

---

## Schnellstart (mit fertiger EXE)

1. `binary/setup_turbine.exe` ausführen und dem Installer folgen
2. Turbine über das Startmenü starten
3. Quelldatei auswählen, Zieldatei (`.tur`-Endung) festlegen, Passwort 2× eingeben
4. **Encrypt** klicken — fertig

Zum Entschlüsseln: Gleicher Ablauf, mit **Decrypt** und demselben Passwort.

**Voraussetzung:** Windows + .NET Framework 4.8 (bei Windows 10/11 bereits installiert).

---

## Aus dem Quellcode bauen

1. `src/Turbine.sln` mit Doppelklick in Visual Studio 2019 oder neuer öffnen
2. Build → Projektmappe erstellen (oder F6 drücken)
3. Die kompilierte `Turbine.exe` liegt dann in `src/Turbine/bin/Release/`

Die Datei `src/turbine_icon.Spp` ist das editierbare Original
des Anwendungs-Icons (Greenfish Icon Editor Format) — nur enthalten,
falls du das Icon verändern möchtest.

---

## Bedrohungsmodell — wovor es schützt

| Szenario | Geschützt? |
|---|---|
| Jemand findet deinen USB-Stick | **Ja** (mit jedem vernünftigen Passwort) |
| Ein Freund will heimlich deine Dateien sehen | **Ja** |
| Daten-Klau aus einem verlorenen Laptop | **Ja** |
| Ein motivierter Hacker mit eigenen Werkzeugen | Hängt von der Passwortstärke ab |
| Geheimdienst mit Kryptanalytikern | **Dafür nicht gedacht** |

Für den Alltagsbedarf an persönlichem Schutz ist Turbine geeignet.
Für staatliche Bedrohungen sind etablierte Standards (AES-GCM, age) zu wählen.

---

## Empfehlungen für Anwender

- **Lange Passwörter verwenden.** 16+ zufällige Zeichen, oder eine merkbare
  Phrase aus 4-5 zusammenhanglosen Wörtern. Je länger, desto besser.
- **Passwörter nicht wiederverwenden.** Jede Datei sollte ihr eigenes haben.
- **Den Quellcode aufheben.** Die Transparenz gehört zur Sicherheit:
  wer den Code lesen kann (oder jemandem vertraut, der das tut), kann
  überprüfen, dass keine versteckten Überraschungen drin sind.

---

## Dokumentation

- **[SECURITY.md](SECURITY.md)** — Ehrliche Sicherheitsbewertung, bekannte Grenzen
- **[CRYPTANALYSIS.md](CRYPTANALYSIS.md)** — Vollständige Kryptanalyse (2026)
- **[NIST_TEST_RESULTS.md](NIST_TEST_RESULTS.md)** — Rohwerte der NIST-STS-Tests
- **[COMPARISON_WITH_RC4.md](COMPARISON_WITH_RC4.md)** — Detaillierter Vergleich mit RC4 (wichtigste Lektüre)
- **[CHANGELOG.md](CHANGELOG.md)** — Versionshistorie

---

## Lizenz

[MIT-Lizenz](LICENSE) — für jeden Zweck nutzbar, auch kommerziell.
Namensnennung des Autors gewünscht, aber nicht zwingend.

---

## Danksagung

Die kryptanalytische Überprüfung und die Dokumentation 2026 wurden mit
Unterstützung eines AI-Coding-Assistenten (Anthropic Claude) erstellt.
Alle Code-Änderungen (explizite Klammerung zur Lesbarkeit, Bias-Dokumentation
als Kommentare) sind im Quellcode erkennbar. Der Cipher-Entwurf und die
ursprüngliche Implementierung stammen vollständig vom Autor.
