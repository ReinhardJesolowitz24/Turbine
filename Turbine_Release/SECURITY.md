# Security Disclosure

This document gives an honest, evidence-based summary of what Turbine
protects against and where its limits lie. It is based on a comprehensive
2026 cryptanalytic review (see [CRYPTANALYSIS.md](CRYPTANALYSIS.md) for
full details).

---

## Threat model

Turbine was designed for **personal protection against casual access**.
Concrete examples it guards against:

- A lost USB stick falling into someone's hands
- A roommate or coworker peeking at your files
- Casual data theft (lost laptop, abandoned old hard drive)
- Recovery of deleted-but-not-overwritten files by basic forensic tools

It is **not** designed for:

- Defense against intelligence agencies or well-funded cryptanalysts
- High-stakes commercial secret protection
- Long-term archival where ciphers may face decades of future analysis

### Integrity / tamper detection (corrected 2026-09)

**Earlier versions of this document claimed the block feedback provided "de-facto
tamper detection". That claim was wrong and is retracted.** Empirical testing
(2026-09) showed that crafted, *checksum-neutral* changes — a matched 2-bit flip
or a byte swap within one block — and **any** change in the file tail decrypt
**silently and locally**, with no propagation; even a generic bit flip
re-synchronises before end-of-file rather than destroying "all subsequent
plaintext". The block feedback is **not** an integrity mechanism.

**Since V6 (version bytes `0x08` / `0x09`, the default for new files) Turbine
appends a real authenticator** — an **HMAC-SHA-384** tag over the header +
ciphertext (Encrypt-then-MAC), verified **before any plaintext is written**. On
mismatch nothing is output and the user is told the file was tampered with or the
key is wrong (this also turns a wrong key from "silently decrypts to garbage"
into a clear error). Files from older versions (0x00–0x07) remain unauthenticated.
See the dedicated section below.

### Endpoint trust — read this before relying on Turbine in a hostile environment

Turbine assumes it runs on a **trustworthy machine**. No cipher can protect you
on a computer an adversary controls, and it is important to be honest about this
because getting it wrong can put people at real risk.

On a compromised endpoint the attack does **not** target the cipher — it targets
the inputs and the binary, *before or around* the encryption:

- a keylogger or screen/memory capture reads your **password / key-file and the
  plaintext** as you type them — the quality of the encryption is then irrelevant;
- a swapped or trojaned `Turbine.exe` can weaken key generation or exfiltrate
  data directly. The published **SHA-256 checksums** and open source let you
  verify a build, but only on a system you trust to run the check honestly.

**Scope of the IV hardening (see [IV_HARDENING.md](IV_HARDENING.md)):** mixing an
independent timing-jitter source into the IV is defense-in-depth against a
*subverted-but-otherwise-standard* OS RNG (the Dual_EC_DRBG class of problem) on
**hardware you trust**. Because most manipulation is deployed in a standardized,
one-size-fits-all way, a non-standard IV construction can also fail to line up
with off-the-shelf attack tooling — a genuine but **narrow** and **not
guaranteed** benefit. It is **not** a license to encrypt sensitive material on
public or untrusted machines.

**For high-risk users** (journalists, lawyers, activists, or ordinary citizens
under a surveilling or repressive state): assume any public or borrowed computer
is compromised. Encrypt sensitive material only on **your own trusted device**,
ideally from a live/amnesiac operating system (e.g. Tails) running on your own
hardware. For adversaries at this level, prefer audited tools with a formal
security record (VeraCrypt, age, GnuPG) over a personal-use cipher. Note also
that **coercion** ("hand over the password") defeats every encryption tool
equally; that is a threat for operational security (data minimization, plausible
deniability), not cryptography.

---

## Verified properties (as of 2026-05-15)

### What works well

- **No backdoor or master key** — verifiable by reading
  `src/Window1.xaml.cs`. The cipher state derives entirely from the user's
  password plus a random IV.
- **Cryptographically random, hardened IV** — the 16 IV bytes are derived from
  two independent sources (OS CSPRNG `RNGCryptoServiceProvider` + independent
  timing jitter) combined via a SHA-256 extractor, as defense-in-depth against a
  manipulated OS RNG. Format-transparent (no version-byte change; old files stay
  decryptable). Same plaintext + same password produces different ciphertexts.
  See [IV_HARDENING.md](IV_HARDENING.md).
- **Authenticated integrity (V6, new files)** — new encryptions (version bytes
  0x08/0x09) carry an **HMAC-SHA-384** Encrypt-then-MAC tag, verified before any
  plaintext is written. Detects any tampering (including crafted, checksum-neutral
  edits) and wrong keys; fail-safe (no output on mismatch).
- **Large internal state** — 1280 bits across 4 parallel gear groups,
  larger than AES-256's 256-bit key.
- **Wide password range** — 6 to 1024 bytes. With a 32-byte random password
  and full byte alphabet, the search space is ~2^256, equivalent to AES-256.
- **8 of 10 NIST Statistical Tests passed** on 100 MB of keystream output
  (full results in [NIST_TEST_RESULTS.md](NIST_TEST_RESULTS.md)).

### Known weaknesses

#### 1. Work Factor in key derivation — RESOLVED in V2 (2026-05-19)
**Status: addressed.** Earlier versions of Turbine processed passwords through
a multi-round mixing function with a few hundred thousand iterations — not a
hardened KDF. Since V2 (file format version byte 0x01), Turbine uses
**PBKDF2-SHA512 with 1,200,000 iterations** before the gear initialization.

Practical impact:
- **V2 files (default since 2026-05-19):** weak passwords now require
  ~1,200,000× more work per brute-force candidate than V1. A 6-character
  password that was crackable in seconds on a GPU now takes weeks/months.
- **V1 files (created with older Turbine builds):** still readable, but the
  original brute-force limitation applies. Re-encrypt important V1 files
  with the new V2 build to upgrade their protection.

**Recommendation:** Use long, random passwords regardless. PBKDF2 helps
against weak passwords but is not a substitute for proper passphrase choice.
A 16-character random password plus PBKDF2 gives effectively unbreakable
security against all known attack methods.

#### 2. Localized bit-bias (Bit 6 ↔ Bit 7)
Two specific bit positions within each output byte show a small
correlation (~0.05 % deviation from random). Detectable as a
"Distinguisher" with 100 MB of output:

- Caused by asymmetric XOR masks (`0x55` / `0xAA`) in the gear-shift
  conditional logic (lines 2451-2452, 2699, 2710 in `Window1.xaml.cs`)
- Deliberately left unchanged for backward compatibility — fixing it
  would break decryption of all existing `.tur` files
- **Practical impact:** None for the stated threat model. The bias allows
  identifying that a file is Turbine-encrypted, but not recovering the
  password or plaintext.

#### 3. Algorithm not externally peer-reviewed
The cipher design is original to the author. Unlike AES (analyzed by
hundreds of cryptographers over 25+ years), Turbine has not undergone
broad public cryptanalysis. The 2026 review is the first external
analysis on record.

**Mitigation:** Threat model explicitly excludes high-stakes scenarios
where this matters.

---

## What was tested in 2026

Comprehensive cryptanalytic review on 100 MB of keystream output
(plaintext = all zeros), plus comparison samples:

| Test category | Result |
|---|---|
| Header / metadata leakage | Original file extension and size visible (BMP container). No password leakage. |
| Shannon entropy | 7.999976 bit/byte (ideal: 8.000) |
| Index of Coincidence | 0.0039063 (ideal random: 0.0039063) |
| Chi² byte frequency | Passed |
| Block repetitions (8/16/32 byte) | Zero repetitions |
| Auto-correlation (all lags 1..1024) | All within noise (\|corr\| < 0.007) |
| NIST STS | 8/10 passed; 2 failures (Runs Test, Longest Run of Ones) caused by single localized bias |
| Bit-pair correlation matrix (28 pairs within byte) | One significant pair (Bit 6 ↔ Bit 7), all others within noise |
| Cross-byte bit correlation matrix (64 pairs) | Zero significant correlations — bias does not cross byte boundaries |
| 3-bit linear approximations (56 triples) | None significant (all \|Z\| < 3.5) |
| Bias independence test | Bit 6↔7 and Bit 2↔3 biases are statistically **independent** (Z = -0.09) — they cannot be combined into a stronger attack |
| **Tamper test (revised 2026-09)** | A generic bit flip garbles only a **bounded** region, then re-synchronises (not "to end of file"); **checksum-neutral edits and tail edits decrypt silently** → block feedback is NOT reliable integrity. Real integrity added as an **HMAC-SHA-384 MAC** in V6 (0x08/0x09). |

### Integrity: HMAC-SHA-384 MAC (V6, version 0x08/0x09)

The block feedback does **not** provide tamper detection (see the corrected note
in the threat model and the revised tamper test above). Real integrity is
provided from V6 on:

- **Encrypt-then-MAC** with **HMAC-SHA-384** (48-byte tag) over the header + IV +
  ciphertext, appended to the file.
- **Verify-before-decrypt:** the tag is checked (constant-time comparison) before
  any plaintext is written. On mismatch the decryption aborts with a clear
  "integrity check failed — file tampered or wrong key" message and writes **no
  output** (fail-safe). This also removes the old data-loss hazard where a wrong
  key "decrypted successfully" into garbage next to the secure-delete feature.
- **Key separation:** the MAC key is `HMAC-SHA-384(master_key, "TURBINE-MAC-v1")`,
  derived from the master key **after** the slow KDF — so the MAC is not a fast
  password-guessing oracle.
- **Verified (2026-09):** on the shipping build — encrypt→0x09+MAC (tag
  independently recomputed, bit-identical), round-trip byte-exact, and tampered
  files rejected with no output.
- **Residual (downgrade):** an attacker can strip the MAC by editing the version
  byte back to 0x06/0x07 — this *removes* authentication but cannot *forge* a
  valid tag. An optional future "strict mode" (accept only 0x08/0x09) would close
  this.

Older files (0x00–0x07) remain unauthenticated; re-encrypt important files with
V6 to gain integrity protection.

---

## Reporting new findings

The author currently does not maintain a private contact channel.
If you discover a previously unreported security issue:

1. **Best:** Fork the repository and publish a write-up
2. **Alternative:** Open a public issue on whatever platform hosts the
   project copy you obtained
3. **For coordinated disclosure:** Public disclosure is acceptable;
   this is a personal-use tool, not infrastructure software

---

## Recommendations for users

| Use case | Recommendation |
|---|---|
| Protecting personal files on a USB stick | **Turbine is suitable** — use a strong password |
| Encrypting tax returns or medical records | **Turbine is suitable** — use a strong password |
| Sending encrypted files to others | OK, but exchange password via separate secure channel |
| Long-term archival (10+ years) | Consider re-encrypting periodically with current standards |
| Protecting trade secrets / IP | Use established AEAD ciphers (AES-GCM, ChaCha20-Poly1305) |
| Resisting government adversaries | Use VeraCrypt, age, or similar audited tools |

---

## Improvements completed in V2/V3 (2026-05-19/20)

The following improvements have been integrated:

1. ✓ **PBKDF2-SHA512 with 1,200,000 iterations** added for password derivation
   (eliminates weak-password brute-force risk) — V2, version byte 0x01
2. ⚠ **Bit 6→7 bias** — investigated extensively across 7 NIST samples.
   The bias is **structural** (caused by asymmetric `0x55`/`0xAA` XOR
   masks in the gear update logic, lines 2451-2452 and 2699-2710). Neither
   PBKDF2 (V2) nor SHA-512 key-file whitening (V3) eliminates it,
   though both reduce its magnitude somewhat. Full elimination would
   require V4 with symmetric masks (breaking change to all existing files).
3. ✓ **Version byte at BMP header position 6** allows multiple file format
   versions to coexist:
   - `0x00` = Legacy V1 (no KDF)
   - `0x01` = V2 password with PBKDF2-SHA512
   - `0x02` = V2 key-file (raw bytes, legacy)
   - `0x03` = V3 key-file with SHA-512 whitening
   - `0x04` / `0x05` = V4 password / key-file (symmetric masks + AES-class S-box, poly 0x11D)
   - `0x06` / `0x07` = V5.2 password / key-file (improved password-info bytes)
   - `0x08` / `0x09` = **V6 password / key-file + HMAC-SHA-384 MAC + AES-disjoint S-box (poly 0x1F3)** — default for new files
4. ✓ **SHA-512 whitening for key-file mode** (V3) added 2026-05-20.
   Empirically improves the Approximate Entropy NIST test when used with
   high-entropy key-file inputs (e.g., previously encrypted .tur files).
   ~2 ms processing overhead.

## Possible future improvements (V3 candidates)

3. ✓ **DONE (V6, 2026-09):** authentication tag added — **HMAC-SHA-384**
   Encrypt-then-MAC (version 0x08/0x09), verified before decryption. Replaces the
   (incorrect) earlier claim of implicit tamper detection.
5. **Argon2id** instead of PBKDF2 for memory-hard derivation (resistant to
   ASIC/GPU attacks)
6. **Authenticated Encryption with Associated Data (AEAD) wrapper** for
   protocol-level integration
7. **Native ARM64 / hardware-accelerated SHA-512** for faster KDF on
   modern systems

V2 (current) is suitable for the stated threat model. V3 would be an
optional further improvement for users with higher-grade security
requirements.
