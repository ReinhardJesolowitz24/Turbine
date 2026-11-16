# Turbine V6.0

Ein kostenloses, quelloffenes Werkzeug zur Datei-Verschlüsselung für den
persönlichen Gebrauch. Ursprünglich 2012 entwickelt von **Reinhard Jesolowitz**,
um persönliche Daten — Passwörter, Login-Dateien, sensible Dokumente — vor
zufälligem Zugriff zu schützen (z.B. bei einem verlorenen USB-Stick).

**Aktuelle Version: V6.0** (2026-11) — mit Integritätsschutz (HMAC-SHA-384,
Encrypt-then-MAC, Dateiversionen `0x08`/`0x09`), einer zur AES-S-Box disjunkten
S-Box, gehärtetem IV pro Datei und einem Schutz gegen entartete Schlüsseldateien.
**Vollständig abwärtskompatibel:** V6.0 entschlüsselt jede ältere Turbine-Datei.
**Nicht vorwärtskompatibel:** Mit V6.0 erzeugte Dateien lassen sich mit V5.x
**nicht** öffnen — bitte alle genutzten Installationen aktualisieren, bevor
Dateien ausgetauscht werden.

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
- **Schlüsseldatei-Modus mit SHA-512-Whitening** (V3) — eine Datei mit
  viel Zufall (echte Fotografie, Archiv) als 1024-Byte-Schlüssel verwendbar.
  Für höchste statistische Qualität empfiehlt sich eine bereits verschlüsselte
  .tur-Datei als Schlüsseldatei. Entartete Schlüsseldateien (einfarbige oder
  synthetische Bilder, mit Nullen aufgefüllte Dateien) werden beim
  Verschlüsseln abgelehnt.
  *Archive:* nur ein **komprimiertes** Archiv verwenden. Ein ZIP im Modus
  „Speichern" („Store", ohne Kompression) enthält die Originaldateien
  unverändert — sind das Texte oder andere vorhersagbare Daten, liest Turbine
  womöglich Klartext. Der Entropie-Wächter fängt nur extreme Fälle ab, nicht
  jede vorhersagbare Datei.
- **Kryptografisch zufälliger IV** für jede Datei (keine zwei Outputs identisch)
- **Byteorientierter Schieberegister-Cipher** mit festem Taktschema (pro
  Ausgabebyte wird reihum genau eine Zahnradgruppe getaktet) und
  zustandsabhängiger nichtlinearer Rückkopplung, strukturell verwandt mit Trivium
- **Integritätsschutz: HMAC-SHA-384, Encrypt-then-MAC** (aktueller
  Quellstand, Dateiversionen `0x08`/`0x09`) — der Tag wird geprüft, bevor
  Klartext geschrieben wird; eine manipulierte Datei oder ein falscher
  Schlüssel wird ohne Ausgabe abgewiesen.
  *Korrektur:* Frühere Fassungen dieser README behaupteten, die
  Block-Verkettung allein liefere eine „faktische Manipulationserkennung".
  Tests im September 2026 haben gezeigt, dass das **nicht stimmt** — gezielte
  Änderungen können unbemerkt bleiben. Dateien älterer Versionen
  (`0x00`–`0x07`) haben **keine** Manipulationserkennung.
  Siehe [SECURITY.md](SECURITY.md) und [CHANGELOG.md](CHANGELOG.md).
- **Keine Hintertür, kein Generalschlüssel** — durch Quellcode-Lektüre überprüfbar

---

## Schnellstart (mit fertiger EXE)

**Direkter Download:** [Turbine.exe (V6.0, ~300 KB)](https://github.com/ReinhardJesolowitz24/Turbine/releases/download/v6.0.0/Turbine.exe) — bitte mit `SHA256SUMS.txt` auf der [Release-Seite](https://github.com/ReinhardJesolowitz24/Turbine/releases/tag/v6.0.0) abgleichen

1. `Turbine.exe` in einen beliebigen Ordner speichern
2. Per Doppelklick starten (keine Installation nötig — portabel)
3. Quelldatei auswählen, Zieldatei (`.tur`-Endung) festlegen, Passwort 2× eingeben
   (oder eine Schlüsseldatei laden)
4. **START** klicken — fertig

Zum Entschlüsseln: die `.tur`-Datei als Quelle wählen — Turbine erkennt sie und
schaltet in den *Decryption Mode* —, Zieldatei festlegen, dasselbe Passwort
eingeben (oder dieselbe Schlüsseldatei laden) und **START** klicken.

**Voraussetzung:** Windows + .NET Framework 4.8 (bei Windows 10/11 bereits installiert).

> *Alternativ kannst du das gesamte Repository klonen oder den Quellcode-ZIP von der [Releases-Seite](https://github.com/ReinhardJesolowitz24/Turbine/releases/tag/v6.0.0) laden — die Binärdatei liegt zusätzlich unter `Turbine_Release/binary/`.*

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
- **Key-File-Modus: das Schlüsselfoto getrennt von den verschlüsselten Dateien
  aufbewahren.** Im Key-File-Modus *ist* das Foto der Schlüssel — es gibt kein
  Passwort und keine langsame Schlüsselableitung dazwischen. Wer Foto und
  `.tur`-Datei hat, kann sofort entschlüsseln. Das Foto auf einem anderen
  Datenträger als die Daten lagern und nie beides über denselben Weg verschicken.
- **Alte Dateien mit V6 neu verschlüsseln.** Dateien früherer Versionen
  (Formatbytes `0x00`–`0x07`) haben keine Integritätsprüfung. Kommt beim
  Entschlüsseln einmal nur Datenmüll heraus, die `.tur`-Datei behalten — siehe
  [SECURITY.md](SECURITY.md).
- **Den Quellcode aufheben.** Die Transparenz gehört zur Sicherheit:
  wer den Code lesen kann (oder jemandem vertraut, der das tut), kann
  überprüfen, dass keine versteckten Überraschungen drin sind.

---

## Dokumentation

- **[SECURITY.md](SECURITY.md)** — Ehrliche Sicherheitsbewertung, bekannte Grenzen
- **[CRYPTANALYSIS.md](CRYPTANALYSIS.md)** — Vollständige Kryptanalyse (2026)
- **[NIST_TEST_RESULTS.md](NIST_TEST_RESULTS.md)** — Rohwerte der NIST-STS-Tests
- **[COMPARISON_WITH_RC4.md](COMPARISON_WITH_RC4.md)** — Detaillierter Vergleich mit RC4 (wichtigste Lektüre)
- **[IV_HARDENING.md](IV_HARDENING.md)** — Diffusionsstudie und gehärteter IV pro Datei (englisch)
- **[AI_DEFENSIVE_REVIEW.md](AI_DEFENSIVE_REVIEW.md)** — Wie ein KI-gestütztes Review echte Schwachstellen fand und behob, und was es nicht beweist (englisch)
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
