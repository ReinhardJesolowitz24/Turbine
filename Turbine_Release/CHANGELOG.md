# Changelog

All notable changes to Turbine are documented in this file.

The format is loosely based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

---

## [Unreleased]

### Code (no format change, no binary rebuilt yet)
- `GatherJitterEntropy` now disposes the `Process` object it creates for the
  process id (it held an OS handle until garbage collection). IV output is
  unchanged in kind; verified with the IV self-test (20 000 IVs, χ² 256.3,
  0 duplicates, backdoor demo still varies) and 5 000 IVs from the production
  build without handle growth.
- Comment added at the BackgroundWorker setup: cancellation is prepared but never
  triggered (no `CancelAsync`, no cancel button); deliberately left unchanged.

### Documentation
- **Downgrade (SECURITY):** stripping the MAC and setting the version byte back
  to 0x06/0x07 yields random garbage, not attacker-chosen content (different
  S-box); verified 2026-11 (0.9 % matching bytes). Strict mode deliberately not
  built; users are told to keep the `.tur` if a decryption produces garbage.
- **README / README_DE:** new recommendations — store the key photo apart from
  the encrypted files (key-file mode has no password and no slow KDF), and
  re-encrypt old files with V6. Documentation list now links `IV_HARDENING.md`
  and `AI_DEFENSIVE_REVIEW.md`. Key-file section warns that archives must be
  compressed: a ZIP in "store" mode can expose plain text to the key reader.
- **Clocking description corrected** (README, README_DE, CRYPTANALYSIS,
  COMPARISON_WITH_RC4): Turbine is not stop-go clocked. The `takt` counter
  clocks exactly one wheel type per output byte in a fixed round-robin
  schedule (Rad 1: takt 0–8, Rad 2: 9–15, Rad 3: 16–19; period 19 after the
  first 20-byte cycle). The schedule does not depend on key or data; the state
  dependence lies in the feedback values. No code change.
- **CRYPTANALYSIS §1.3:** the block feedback checksums (`block_quersumme`,
  `block_summe`) are computed over the **ciphertext**, not the plaintext. Body
  text, heading, mode table and §7.1 now say so; the GCM comparison is
  withdrawn and the source line references are updated.
- **IV_HARDENING:** correction box on the motivation (with all-zero plaintext
  the checksums are *not* trivial, since ciphertext = keystream; the study's
  question and results stand) and the old "de-facto tamper detection" sentence
  replaced by a correction note pointing to the MAC.

## [V6.0 — Release] — 2026-11-16

First binary release containing all V6 changes listed below (integrity MAC,
AES-disjoint S-box, key-file entropy guard, review fixes) plus the IV hardening
from V5.2.1.

- **Compatibility:** V6.0 reads every older Turbine file (`0x00`–`0x09`).
  New files are written as `0x08` (password) / `0x09` (key file).
  **V5.x cannot read `0x08`/`0x09`** — update all installations before
  exchanging files.
- Version: window title, logo, About dialog and assembly version 5.2 → 6.0;
  the About dialog now describes the MAC and the key-file guard.
- README / README_DE: version header, download link (now `v6.0.0`) and the
  quick start (single **START** button, automatic decryption detection)
  updated.
- **Release acceptance (GUI, build of commit `da6e5df`, 2026-11-16 — the V6.0
  build differs from it only in version strings and logo):** low-entropy key
  file rejected and START usable afterwards without restart; tampered `0x09`
  file → integrity dialog, no output, START usable afterwards; password mode
  `0x08` round trip byte-exact and wrong password → integrity dialog, no
  output; 0-byte file → 118-byte `.tur` → 0-byte plaintext.
- **Large files (GUI, released V6.0 binary, 2026-11-16):** 3 GiB and 4.5 GiB
  key-file round trips are byte-exact (SHA-256 identical), MAC verified over
  the whole file. Throughput is linear in file size: encryption ≈ 2.4 MB/s,
  decryption ≈ 1.4 MB/s (4.5 GiB: ~33 min / ~58 min); the MAC check before
  decryption takes well under a minute.
- **Known cosmetic limit:** the 32-bit size fields of the BMP container header
  wrap above 4 GiB. Turbine itself takes the length from the file system, so
  such files still decrypt correctly; only the BMP header is no longer a valid
  image description (image viewers cannot open BMPs of this size anyway).

---

## [Review fixes] — 2026-11-15

Three findings from a follow-up review of the V6 changes.

- **Fixed: program stuck after a rejected operation.** After an integrity
  failure on decryption, or a rejected low-entropy key file on encryption,
  the internal "process running" flag stayed set. START and the wipe function
  then did nothing until the program was restarted. The state and the display
  are now reset before the message is shown.
- **Fixed: MAC over large files.** The HMAC was computed by loading the whole
  file into memory, which failed for files of 2 GB and more and needed as much
  RAM as the file is large. It is now computed as a stream in 64 KB blocks.
  The tag is unchanged: files written before this fix verify as before.
- **Documentation:** the withdrawn "de-facto tamper detection" claim was still
  present in `README.md`, `README_DE.md`, `CRYPTANALYSIS.md` and
  `COMPARISON_WITH_RC4.md`. Those passages are now corrected (the V6 entry
  below said `CRYPTANALYSIS.md` had already been corrected; it had not).

---

## [Key-file entropy guard] — 2026-09-13

Encrypting with a **degenerate key file** — solid-color or synthetic images,
zero-padded or silent files — is now **rejected**. Such files have three XOR
windows that collapse to a near-constant key, making the derived key a public
function of the stored IV.

- On **encryption**, rejected if the derived 1023-byte key material has **< 64
  distinct byte values OR < 6.0 bits/byte** Shannon entropy. Real photographs
  (especially JPEG, ~7.9 bits/byte) pass easily — this only stops the degenerate
  misuse.
- The check runs **only on encryption** (in DoWork, before any output). Loading a
  key file and **decryption are never blocked**, so any existing file — even one
  made with a weak key file — stays decryptable.
- The key-file prompt now recommends a **real photograph** and warns against
  solid-color/synthetic images.
- **Non-breaking:** the key derivation itself is unchanged.

---

## [V6 — Integrity MAC + AES-disjoint S-box] — 2026-09-13

### Authenticated integrity: HMAC-SHA-384 (version bytes `0x08` / `0x09`)

New encryptions now carry a real authenticator, and this replaces the previous
(incorrect) claim that the block feedback provided "de-facto tamper detection".

- **Encrypt-then-MAC** with **HMAC-SHA-384** (48-byte tag) over header + IV +
  ciphertext, appended to the file. On decryption the tag is verified
  (constant-time) **before any plaintext is written**; on mismatch the operation
  aborts with a clear "integrity check failed — file tampered or wrong key"
  message and produces **no output** (fail-safe). This also fixes the data-loss
  hazard where a wrong key previously "decrypted successfully" into garbage.
- **Key separation:** MAC key = `HMAC-SHA-384(master_key, "TURBINE-MAC-v1")`,
  derived **after** the slow KDF → the MAC is not a fast password-guessing oracle.
- **New version bytes:** `0x08` = password + MAC, `0x09` = key-file + MAC (default
  for new files). Old versions (0x00–0x07) decrypt unchanged (backward compatible).
- **Residual (downgrade):** stripping the MAC via a version-byte edit removes
  authentication but cannot forge a valid tag; an optional strict mode would close it.

### Correction: block feedback is NOT tamper detection

Empirical testing (2026-09) disproved the earlier claim: crafted checksum-neutral
edits (matched 2-bit flip, byte swap within a block) and any tail edit decrypt
**silently and locally**; a generic bit flip re-synchronises before end-of-file.
`SECURITY.md` and `CRYPTANALYSIS.md` references corrected. Real integrity is now
the HMAC above.

### New S-box for V6 (irreducible polynomial `0x1F3`)

Version 0x08/0x09 use a new AES-class S-box built from GF(2^8)/`0x1F3` (affine
constant 0x4A) with **zero entries in common with the AES S-box** — same proven
strength (nonlinearity 112, differential uniformity 4), maximal independence from
AES. Versions 0x04–0x07 keep the previous S-box (poly `0x11D`). Purely a
distinctness choice; no security difference between the two S-boxes.

### Verification
Shipping build (GUI acceptance, 2026-09-13): encrypt → 0x09 + MAC (tag
independently recomputed, bit-identical) + random IV; round-trip byte-exact;
tampered file → integrity dialog + no output.

---

## [IV Hardening] — 2026-07-08

### Hardened per-file IV generation (defense-in-depth)

The 16-byte initialization vector is now derived from **two independent
randomness sources** combined by a SHA-256 extractor, instead of the OS CSPRNG
alone:

- **Source 1:** OS CSPRNG (`RNGCryptoServiceProvider` / BCryptGenRandom) — primary.
- **Source 2:** timing jitter (`Stopwatch` deltas) + environment values —
  independent of the CNG entropy pool (deliberately *not* `Guid.NewGuid()`, which
  is itself CNG-based on Windows).

**Rationale:** guards against a manipulated or predictable OS RNG (historical
precedent: Dual_EC_DRBG). As long as one source is unpredictable, the IV is
unpredictable — a diversified hedge against a common-cause failure of the single
entropy source. Timing jitter is intentionally low-tech: weak but clearly
independent, and requires no native interop (RDRAND was considered and deferred).

**Format-transparent:** no new version/variant byte. Only IV *generation*
changes; the IV is stored and read back as before, so **all existing files
remain decryptable**.

**Impl.:** `Window1.IvHardening.cs` (`GenerateIV16`), call site in `Window1.xaml.cs`.
**Background/investigation:** see `IV_HARDENING.md` — a differential/diffusion
study (Fable-framed, Opus-executed) that made the IV's load-bearing role explicit.

Self-test (20,000 IVs): per-byte mean 127.48, χ²(255)=240.5, 0 duplicates;
with the CNG contribution forced to a constant the IVs still differ (independent
source confirmed).

---

## [V5.2 — Key Generation Improvements] — 2026-06-12

### Improved key generation quality (version bytes `0x06` / `0x07`)

Three targeted optimizations to the password-based key generation that
eliminate a measurable byte-distribution bias (Chi-Squared test) previously
observable with very short, purely numeric passwords like `123456`:

1. **Password-info-byte calculation rewritten** (V5.2 mode only):
   - Removed right-shift operations (`>> 3,4,5,7,8`) that produced near-zero
     bytes for short passwords (e.g., `>> 7` on a 6-character password yields
     only 1 bit of information)
   - Replaced with chaining and XOR 0xFF inversions that preserve full
     8-bit entropy regardless of password length

2. **Shift register seeding** (V5.2 mode only):
   - The five `temp_passwort_gen` variables and the first element of each
     of the four key generation vectors (`passwort_gen_vector` 1–4) are now
     pre-initialized with password-derived info bytes
   - Previously all started at 0x00, causing the first iterations to produce
     weak output ("cold start" problem)
   - Each vector uses a different permutation of info bytes to avoid
     identical initial states

3. **Bit rotation replaces left-shift** (V5.2 mode only):
   - `temp_passwort_gen3 << 3` → `(temp_passwort_gen3 << 3) | (temp_passwort_gen3 >> 5)`
   - `temp_passwort_gen3 << 2` → `(temp_passwort_gen3 << 2) | (temp_passwort_gen3 >> 6)`
   - Left-shift discards 2–3 bits per iteration and fills with zeros;
     over millions of iterations this accumulates into a systematic bias
   - Rotation preserves all bit information — no data loss per round

### Measured impact (NIST SP 800-22, 10 MB all-zeros, password `123456`)

| Version | Chi-Squared p-value | Result |
|---------|:---:|:---:|
| V5.1 (before) | 0.0000001 | FAIL (16/17) |
| V5.2 (after) | 0.092 | **PASS (17/17)** |

With a strong password (e.g., `123Test(x)456ARBq`), both V5.1 and V5.2
score 17/17 — the improvements specifically help the worst case.

### Important: password strength still matters

Passing all 17 NIST tests does **not** make a weak password secure.
NIST tests measure the statistical quality of the cipher output, not the
resistance against brute-force or dictionary attacks. The password `123456`
has only 10^6 possible combinations and will be cracked in milliseconds
regardless of NIST scores. Always use passwords with at least 12 characters
mixing uppercase, lowercase, digits, and special characters — or use a
passphrase of 5–6 unrelated words (25+ characters).

### Backward compatibility

- All previous file formats (`0x00`–`0x05`) remain fully decryptable
- New encryptions write version byte `0x06` (password) or `0x07` (key-file)
- The three optimizations are guarded by the `use_improved_pw_info` flag
  and only activate for V5.2 files

### Other changes
- Window title, UI label, About dialog: V5.1 → V5.2
- Assembly version: 5.1.0.0 → 5.2.0.0
- Updated logo graphics

---

## [App version V5.0 — finalization release] — 2026-05-20

This is the **consolidated V5.0 release** of the Turbine application,
covering all updates since V4.1. File format markers V2 (0x01) and V3 (0x03)
referenced below are the **cipher format** version bytes inside `.tur` files,
not the application version number.

### App-version-level changes (visible to user)
- **Application version bumped from V4.1 to V5.0**:
  - Title bar: `Turbine V5.0`
  - UI label: `Turbine V5.0`
  - About dialog: rewritten with V5.0 description and feature list
  - Assembly version: `5.0.0.0`
- **Logo "burning effect" cleanup**:
  - Removed obsolete `Tur_effekt2.bmp` overlay (which permanently showed "V4.1")
  - The burning effect now works purely via WPF text rendering
    (label foreground = red + emboss effect) — version-independent
  - Result: future version bumps automatically update the burning text too

### UI/UX fixes
- **Pop-up windows now reliably come to foreground** after encryption,
  decryption, or any error. Previously (post-.NET 3.5→4.8 migration),
  MessageBoxes called from BackgroundWorker threads stayed hidden behind
  other windows. Fixed via central `ShowFg()` helper that marshals to UI
  thread + Activate() + Topmost trick + Focus() + Owner parameter.
  Applied to **21 MessageBox call sites** consistently.
- **Progress bar moves during KDF phase** (previously appeared frozen ~5 seconds).
  PBKDF2 now runs in a `Task.Run()` while the main DoWork thread polls and
  reports progress every 500 ms (1% → 8% over 5 seconds).
- **Progress bar no longer jumps backward** after KDF. Sets
  `fortschritt_merker = 8` before encryption loop starts, so the encryption
  loop only reports progress when it exceeds the KDF's 8 %.
- **Mighty Mouse virtual keyboard** extended from 30 hex characters to
  **all 95 printable ASCII characters** (added in earlier sub-release, kept).
- **Debug MessageBox removed** (showed source file path after selection).

### Cipher / KDF improvements (file format)
All of these are documented in detail in CRYPTANALYSIS.md and
NIST_TEST_RESULTS.md:
- **V2 (0x01)**: PBKDF2-SHA512 with 1,200,000 iterations for password mode
- **V3 (0x03)**: SHA-512 whitening for key-file mode
- All previous file format markers (0x00, 0x01, 0x02) remain decryptable

### Code organization
- Code-level comments at lines 2451-2452, 2699-2710 documenting the
  Bit 6→7 bias source in gear update masks (left unchanged for backward
  compatibility; would be a V4 cipher change)
- Operator-precedence parentheses added at lines 1957, 1973, 1989, 2006
  for readability (no behavior change)

### Statistical study
- 7-sample NIST Statistical Test Suite study (see NIST_TEST_RESULTS.md):
  V1, V2 weak/strong password, V2 key-file JPG/ZIP, V3 key-file ZIP/.tur
- Empirical findings transparently documented including corrected claims

### Build / packaging
- `binary/Turbine.exe` rebuilt and updated to V5.0
- Old `Tur_effekt2.bmp` resource removed from project + repository

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
