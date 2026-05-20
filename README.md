# Turbine V5.0

A free, open-source file encryption tool for personal use.
Originally developed in 2012 by **Reinhard Jesolowitz** as a way to protect
personal data — passwords, login files, sensitive documents — against
casual access (e.g., a lost USB stick).

> **All project files are inside the [`Turbine_Release/`](Turbine_Release/) folder.**

---

## Quick links

| What | Where |
|---|---|
| Project overview (English) | [`Turbine_Release/README.md`](Turbine_Release/README.md) |
| Projekt-Übersicht (Deutsch) | [`Turbine_Release/README_DE.md`](Turbine_Release/README_DE.md) |
| Security disclosure | [`Turbine_Release/SECURITY.md`](Turbine_Release/SECURITY.md) |
| Full cryptanalytic review | [`Turbine_Release/CRYPTANALYSIS.md`](Turbine_Release/CRYPTANALYSIS.md) |
| Comparison with RC4 | [`Turbine_Release/COMPARISON_WITH_RC4.md`](Turbine_Release/COMPARISON_WITH_RC4.md) |
| NIST test results | [`Turbine_Release/NIST_TEST_RESULTS.md`](Turbine_Release/NIST_TEST_RESULTS.md) |
| Version history | [`Turbine_Release/CHANGELOG.md`](Turbine_Release/CHANGELOG.md) |
| Source code | [`Turbine_Release/src/`](Turbine_Release/src/) |
| Pre-built executable | [`Turbine_Release/binary/Turbine.exe`](Turbine_Release/binary/) |
| License (MIT) | [`Turbine_Release/LICENSE`](Turbine_Release/LICENSE) |

---

## What is Turbine?

A Windows file encryption tool with a self-developed stream cipher:

- **1280-bit internal state** distributed across 4 parallel gear groups (Trivium family)
- **PBKDF2-SHA512** key derivation for password mode (1,200,000 iterations) — V5.0
- **SHA-512 whitening** for key-file mode — V5.0
- **Cryptographically random IV** per file, BMP container
- **No backdoor, no master key** — verifiable by reading the source

## Threat model

Designed for:
- Protecting personal files on USB sticks against casual access
- "Lost device" scenarios where finders shouldn't be able to read your data

Not designed for:
- Resistance against state-level cryptanalysts (use AES-GCM or VeraCrypt for that)
- Long-term archival where cipher faces decades of future analysis

## License

[MIT License](Turbine_Release/LICENSE) — use it for any purpose, including commercial.
Copyright © 2012-2026 Reinhard Jesolowitz.
