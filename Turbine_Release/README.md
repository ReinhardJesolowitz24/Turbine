# Turbine

A free, open-source file encryption tool for personal use.
Originally developed in 2012 by **Reinhard Jesolowitz** as a way to protect
personal data — passwords, login files, sensitive documents — against
casual access (e.g., a lost USB stick).

> *Looking for the German version? See [README_DE.md](README_DE.md).*

---

## What it does

Turbine encrypts files using a self-developed stream cipher with a 1280-bit
internal state, distributed across four parallel "gear" groups (hence the name).
The encrypted output is wrapped in a BMP container, which lets it bypass
naive content filters and looks unremarkable on disk.

- **Password-based encryption** (6 to 1024 characters)
- **Cryptographically random IV** for every file (no two outputs are identical)
- **Stop-go-clocked shift register** design, structurally related to Trivium
- **CFB-like cross-block feedback** — any tampering with the ciphertext
  destroys all subsequent plaintext (de-facto manipulation detection
  without an explicit MAC)
- **No backdoor, no master key** — verifiable by reading the source

---

## Quick start (using the pre-built binary)

1. Open `binary/setup_turbine.exe` and follow the installer
2. Launch Turbine from the Start menu
3. Select source file, target file (`.tur` extension), enter a password twice
4. Click **Encrypt** — done

To decrypt: same procedure, with **Decrypt** selected and the original password.

**Requires:** Windows + .NET Framework 4.8 (already installed on Windows 10/11).

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
