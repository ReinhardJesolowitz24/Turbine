# Turbine V6.0

A free, open-source file encryption tool for personal use.
Originally developed in 2012 by **Reinhard Jesolowitz** as a way to protect
personal data — passwords, login files, sensitive documents — against
casual access (e.g., a lost USB stick).

**Current version: V6.0** (2026-11) — adds authenticated integrity
(HMAC-SHA-384, Encrypt-then-MAC, file versions `0x08`/`0x09`), an AES-disjoint
S-box, a hardened per-file IV and a guard against degenerate key files.
**Fully backward compatible:** V6.0 decrypts every older Turbine file.
**Not forward compatible:** files written by V6.0 can **not** be opened with
V5.x — update every installation you use before you exchange files.

> *Looking for the German version? See [README_DE.md](README_DE.md).*

---

## What it does

Turbine encrypts files using a self-developed stream cipher with a 1280-bit
internal state, distributed across four parallel "gear" groups (hence the name).
The encrypted output is wrapped in a BMP container, which lets it bypass
naive content filters and looks unremarkable on disk.

- **Password-based encryption** (6 to 1024 characters)
- **PBKDF2-SHA512 key derivation** (V2, 1,200,000 iterations) —
  makes brute-force attacks ~1.2 million times more expensive for weak
  passwords. Old V1 files remain readable without changes.
- **Key-file mode with SHA-512 whitening** (V3) — use a high-entropy file
  (a real photograph, an archive) as a 1024-byte cryptographic key. For
  highest statistical quality, use a previously encrypted .tur file as the
  key. Degenerate key files (solid-colour or synthetic images, zero-padded
  files) are rejected on encryption.
- **Cryptographically random IV** for every file (no two outputs are identical)
- **Byte-oriented shift-register** design with a fixed clocking schedule
  (one gear group per output byte, round-robin) and state-dependent
  nonlinear feedback, structurally related to Trivium
- **Integrity protection: HMAC-SHA-384, Encrypt-then-MAC** (current source,
  file versions `0x08`/`0x09`) — the tag is verified before any plaintext is
  written; a tampered file or a wrong key is rejected with no output.
  *Correction:* earlier versions of this README claimed that the cross-block
  feedback alone gave "de-facto manipulation detection". Testing in 2026-09
  showed that this is **not true** — crafted edits can pass silently. Files
  written by older versions (`0x00`–`0x07`) have **no** tamper detection.
  See [SECURITY.md](SECURITY.md) and [CHANGELOG.md](CHANGELOG.md).
- **No backdoor, no master key** — verifiable by reading the source

---

## Quick start (using the pre-built binary)

**Direct download:** [Turbine.exe (V6.0, ~300 KB)](https://github.com/ReinhardJesolowitz24/Turbine/releases/download/v6.0.0/Turbine.exe) — verify it against `SHA256SUMS.txt` on the [release page](https://github.com/ReinhardJesolowitz24/Turbine/releases/tag/v6.0.0)

1. Save `Turbine.exe` to any folder
2. Double-click to launch (no installation needed — portable)
3. Select source file, target file (`.tur` extension), enter a password twice
   (or load a key file)
4. Click **START** — done

To decrypt: select the `.tur` file as source — Turbine detects it and switches
to *Decryption Mode* — choose a target file, enter the original password (or
load the same key file) and click **START**.

**Requires:** Windows + .NET Framework 4.8 (already installed on Windows 10/11).

> *Alternatively, you can clone the whole repository or download the source-code ZIP from the [Releases page](https://github.com/ReinhardJesolowitz24/Turbine/releases/tag/v6.0.0) — the binary is also included under `Turbine_Release/binary/`.*

---

## Building from source

1. Open `src/Turbine.sln` in Visual Studio 2019 or later (double-click)
2. Build → Build Solution (or press F6)
3. The compiled `Turbine.exe` appears in `src/Turbine/bin/Release/`

The icon source file `src/turbine_icon.Spp` is the editable original
of the application icon (Greenfish Icon Editor format) — included only
if you want to modify the icon.

---

## Threat model — what this protects against

| Scenario | Protected? |
|---|---|
| Someone finds your USB stick | **Yes** (with any reasonable password) |
| A friend tries to peek at your files | **Yes** |
| Casual data theft from a lost laptop | **Yes** |
| A determined hacker with custom tools | Depends on password strength |
| State-level adversary with cryptanalysts | **Not designed for this** |

For everyday personal protection, Turbine is fit for purpose.
For nation-state-level threats, use established standards (AES-GCM, age, etc.).

---

## Recommendations for users

- **Use a long password.** 16+ random characters, or a memorable passphrase
  of 4-5 unrelated words. The longer, the better.
- **Don't reuse passwords.** Each file should have its own.
- **Keep the source code close.** The transparency is part of the security:
  if you can read the source (or have someone you trust read it), you can
  verify there are no hidden surprises.

---

## Documentation

- **[SECURITY.md](SECURITY.md)** — Honest security disclosure, known limits
- **[CRYPTANALYSIS.md](CRYPTANALYSIS.md)** — Full cryptanalytic review (2026)
- **[NIST_TEST_RESULTS.md](NIST_TEST_RESULTS.md)** — Raw NIST STS test data
- **[COMPARISON_WITH_RC4.md](COMPARISON_WITH_RC4.md)** — Detailed comparison with RC4 (most important read)
- **[CHANGELOG.md](CHANGELOG.md)** — Version history

---

## License

[MIT License](LICENSE) — use it for any purpose, including commercial.
Attribution to the original author appreciated but not required.

---

## Acknowledgments

The 2026 cryptanalytic review and documentation were prepared with the
assistance of an AI coding assistant (Anthropic Claude). All code-level
findings (operator-precedence parentheses for readability, bias documentation
comments) are reflected in the source. The cipher design and original
implementation are entirely the author's own.
