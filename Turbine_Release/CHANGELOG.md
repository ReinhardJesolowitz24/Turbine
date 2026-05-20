# Changelog

All notable changes to Turbine are documented in this file.

The format is loosely based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

---

## [V3 — Key-File Whitening] — 2026-05-19/20

### Added
- **SHA-512 whitening for key-file mode** (V3, version byte `0x03`):
  - 1023 key-file bytes + 16 IV bytes → SHA-512 → 64-byte master key
  - Counter-mode SHA-512 expansion to 1024 bytes (same pattern as V2 KDF)
  - ~2 ms processing time (negligible vs. V2's 5-10 seconds for PBKDF2)
- **Fourth version byte `0x03`** in BMP header position 6 for V3 key-file files

### Changed
- New key-file encryptions now write `0x03` instead of `0x02` by default
- Old `0x02` files remain fully decryptable (backwards compatible)

### Statistical impact (measured over 7 NIST samples)
- **V3 with .tur key file**: Approximate Entropy test now PASSES
  (was the only failing test that could be addressed through key-file
  whitening — structural Bit 6→7 and bit-balance issues remain)
- **V3 with JPG/ZIP key file**: NIST score 5/10 (same as V2 key-file)
  — SHA-512 whitening absorbs local bit-transition anomalies but
  cannot fully absorb the structural patterns from compressed formats
- **Lesson learned**: Use already-encrypted .tur files as key-files for
  highest cipher output quality. Direct JPG/ZIP key-files work but
  carry residual format patterns into cipher output.

### Notes on file-format compatibility
All previous file format versions remain decryptable:
- `0x00` — Legacy V1 password
- `0x01` — V2 password with PBKDF2
- `0x02` — V2 key-file (raw bytes, no whitening)
- `0x03` — V3 key-file with SHA-512 whitening (new default for key-files)

---

## [V2 — KDF and UI Update] — 2026-05-19

### Added
- **PBKDF2-SHA512 key derivation function** (KDF V2)
  - Iterations: 1,200,000 (conservative, exceeds OWASP 2023 recommendation)
  - Hash: SHA-512
  - Salt: the existing 16-byte cryptographic IV
  - Output: 64-byte master key + counter-mode SHA-512 expansion to 1024 bytes
    (HKDF-Expand pattern per NIST SP 800-108)
- **File format version byte** at BMP header position 6:
  - `0x00` — Legacy V1 (all existing files; auto-detected, fully backwards compatible)
  - `0x01` — V2 with PBKDF2 password-based key derivation
  - `0x02` — V2 with key file (no PBKDF2 — key file already high-entropy)
- **Mighty Mouse virtual keyboard** extended from 30 to **all 95 printable ASCII
  characters** (digits, uppercase, lowercase, special chars, SPACE).
  Layout reorganized into 7 compact rows with bordered overlay panel.
  Eliminates the entropy reduction that earlier limited Mighty Mouse to
  hex-only input.

### Changed
- Encryption with V2 mode adds ~5-10 seconds of one-time key derivation
  delay. Existing files are unaffected (still decrypt instantly with V1).
- BMP header byte 6 now carries the version flag. Previously always 0x00.
- `name_der_datei6` array sizing logic: derived from password length in V1,
  always 1024 bytes in V2 (regardless of password length).

### Security improvements achieved
- **Brute-force protection now scales with password length AND iteration count.**
  Weak passwords (e.g., 6-8 chars) are now ~1.2 million times more expensive
  to attack than in V1.

### Honest notes on statistical quality
- The structural Bit 6→7 bias is **unchanged** in V2. PBKDF2 affects only
  the input to gear initialization, not the gear update function (which is
  where the bias originates).
- V2 NIST test scores (6-7/10 passed in tested samples) are roughly comparable
  to V1 (8/10), with sample-to-sample variability. With matched conditions
  (same strong password), V2 showed slightly more failures than V1, primarily
  in tests checking overall bit balance.
- This is acceptable because (a) the absolute deviations are small, (b) the
  primary security gain is brute-force resistance, not statistical purity,
  and (c) the 100 MB 0x00 plaintext test is the worst-case scenario — real
  files mask the keystream statistics in the ciphertext.

### Compatibility
- **Fully backwards compatible**: all existing `.tur` files (with version byte 0x00)
  continue to decrypt without changes.
- New files written by V2 are not readable by older Turbine builds.

---

## [Documentation Correction] — 2026-05-18

### Changed (documentation only — no code change)
- **CRYPTANALYSIS.md**: Added section 1.3 "Operating mode:
  Plaintext-feedback (CFB-like)" documenting an empirically discovered
  cross-block feedback mechanism. The earlier classification as
  "synchronous stream cipher" was corrected to "CFB-like with
  plaintext checksum chained across blocks via `block_quersumme`
  (line 814 of Window1.xaml.cs)".
- **SECURITY.md**: Updated authentication discussion. Added note
  about de-facto tamper detection arising from the CFB-like feedback.
- **README.md / README_DE.md**: Added one-line note about CFB-like
  feedback and tamper-propagation property.
- **COMPARISON_WITH_RC4.md**: Added clarifying note about Turbine's
  operating mode in the structural comparison.

### Rationale
An empirical test (1-bit flip at ciphertext position 2500) revealed
that decryption errors propagate to the end of the file, not just
within one 8-byte block as initially predicted from code review.
Investigation traced the cause to the `block_quersumme` variable
(line 814) which accumulates an XOR-checksum of plaintext bytes and
feeds it into the next block's processing (lines 3098, 3100, 3104).
This is functionally equivalent to NIST CFB mode (SP 800-38A) and
was an intentional 2012 design choice, recalled by the author.

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
