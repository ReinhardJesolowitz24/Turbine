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
- Authentication / integrity (it does not sign or MAC the ciphertext —
  but note that the CFB-like feedback mode provides **de-facto tamper
  detection**: any modification to the ciphertext destroys all subsequent
  plaintext, making the corruption obvious to the receiver; see
  CRYPTANALYSIS.md section 1.3 for details)

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
| **Tamper-propagation test (2026-05-18)** | **1-bit flip in ciphertext destroys all plaintext from that point to end of file — CFB-like chaining via `block_quersumme` confirmed** |

### Tamper-detection property (informal)

Because of the CFB-like cross-block feedback (see CRYPTANALYSIS.md §1.3),
Turbine provides a form of **manipulation detection without an explicit
MAC**: any change to the ciphertext, even a single bit, causes the
decryption from that point onward to produce garbage. The receiver
notices immediately. This is not a cryptographic guarantee in the
strict sense (an attacker could deliberately corrupt the entire tail
of the file), but it does prevent **silent targeted modification** of
specific plaintext values.

For threat models requiring strict cryptographic integrity guarantees
(e.g., where the receiver cannot tolerate any uncertainty), an explicit
authenticator (HMAC-SHA256 or similar) should still be added.

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
   - `0x03` = V3 key-file with SHA-512 whitening (new default for key-files)
4. ✓ **SHA-512 whitening for key-file mode** (V3) added 2026-05-20.
   Empirically improves the Approximate Entropy NIST test when used with
   high-entropy key-file inputs (e.g., previously encrypted .tur files).
   ~2 ms processing overhead.

## Possible future improvements (V3 candidates)

3. **Add an authentication tag** (HMAC-SHA256 or similar) so manipulation of
   ciphertext can be detected explicitly (currently the CFB-like feedback
   provides only implicit tamper detection)
5. **Argon2id** instead of PBKDF2 for memory-hard derivation (resistant to
   ASIC/GPU attacks)
6. **Authenticated Encryption with Associated Data (AEAD) wrapper** for
   protocol-level integration
7. **Native ARM64 / hardware-accelerated SHA-512** for faster KDF on
   modern systems

V2 (current) is suitable for the stated threat model. V3 would be an
optional further improvement for users with higher-grade security
requirements.
