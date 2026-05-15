# Changelog

All notable changes to Turbine are documented in this file.

The format is loosely based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

---

## [Unreleased / V1 Documentation Update] — 2026-05-15

### Added
- `LICENSE` (MIT)
- `README.md` and `README_DE.md` — bilingual project overview
- `SECURITY.md` — honest threat model and known limitations
- `CRYPTANALYSIS.md` — full cryptanalytic review
- `NIST_TEST_RESULTS.md` — raw test data from 2026 NIST STS run
- `CHANGELOG.md` (this file)
- `.gitignore` for future Git use
- `HOWTO_PUBLISH.md` — guide for publishing this release

### Changed
- Operator-precedence parentheses added at lines 1957, 1973, 1989, 2006
  in `Window1.xaml.cs` for code readability. **No behavior change** —
  the C# compiler produces identical IL with or without these parens
  (tested: existing `.tur` files decrypt correctly).
- Documentation comments added at lines ~2432 and ~2691 in
  `Window1.xaml.cs` describing the localized Bit 6↔7 bias.
  These comments explain why the asymmetric `0x55`/`0xAA` masks were
  not changed (backward compatibility with existing encrypted files).

### Notes
- **No cryptographic algorithm change.** All `.tur` files created with
  any prior version of Turbine continue to decrypt correctly with this
  release.
- The bit-6/bit-7 bias was discovered during the 2026 cryptanalytic
  review. It is a small statistical distinguisher (~0.05 % deviation
  from random) without practical attack consequences for the documented
  threat model.

---

## [Earlier .NET 4.8 migration] — 2026-04 / 2026-05

### Added
- Reference to `System.Xaml` assembly (separated from `WindowsBase` in
  .NET 4.0+)
- `app.config` with `<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" />`
- `BootstrapperPackage` for .NET Framework 4.8

### Changed
- Target framework: `.NET 3.5` → `.NET Framework 4.8`
- ToolsVersion in csproj: from `3.5` → `4.0`
- Random number generator: replaced insecure `System.Random` with
  `RNGCryptoServiceProvider` for IV generation. Uses `using` block
  (valid since .NET 4.0).
- Cancellation support: added `CancellationPending` checks in both
  `BackgroundWorker` instances for proper cancellation handling.
- `prozess_laueft` field declared `volatile` for thread-safe access
  across UI and worker threads.
- `button3_Click` (target file selection) refactored to remove
  unnecessary `StreamWriter` (which was causing "Access not possible!"
  errors when writing to read-only paths).
- Initial directory in file dialogs: from hardcoded German path to
  `Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)`.

### Fixed
- "Access not possible!" error when selecting target file
- Compile error: `IComponentConnector` not found in
  `System.Windows.Markup` (resolved by adding `System.Xaml` reference)

---

## [Original Release] — ~2012

### Added
- Initial implementation of Turbine stream cipher
- 4 parallel gear groups, each with 3 shift registers (gear_a / b / c)
- Stop-go clocking based on internal `takt` counter
- Multi-stage password-based key derivation
- 16-byte cryptographic IV stored in BMP container header
- WPF user interface (.NET 3.5)
- File-type preservation: original extension stored in BMP header
- Password strength indicator
- Backup feature (file deletion of source after encryption)

### Designed for
- Personal data protection (passwords, login credentials, sensitive files)
- Use case: protection of files on portable storage (USB sticks)
- Threat model: casual access by file finders, not state-level adversaries

### Notable design choices
- **No backdoor, no master key** — verifiable by source-code inspection
- BMP container as steganographic camouflage (looks like an unremarkable
  image file on disk, bypasses naive content filters)
- Password range 6 to 1024 bytes (very long passwords supported)
- Self-developed cipher rather than wrapping AES (transparency goal:
  user can read every line that touches their data)
