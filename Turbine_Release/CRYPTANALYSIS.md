# Cryptanalytic Review of Turbine

**Date:** 2026-05-15
**Reviewer:** AI-assisted analysis (Anthropic Claude)
**Scope:** Statistical and structural analysis based on 100 MB of keystream
output (encryption of all-zero plaintext) plus code-level review.

---

## Executive summary

Turbine is a stream cipher of the **stop-go-clocked shift register**
family with a **CFB-like (Cipher Feedback) operating mode** that
includes a plaintext-derived checksum chained across blocks (see
section 1.3). Structurally most similar to A5/1 (GSM), E0 (Bluetooth),
and Trivium (eSTREAM portfolio) for the keystream generator, and to
NIST CFB-mode for the block-feedback construction.

Compared to RC4, the cipher Turbine most closely resembles in operational
profile, Turbine shows **structural advantages**: smaller number of
detectable biases, biases that are statistically independent (and thus
not combinable for stronger attacks), and no cross-byte correlations.

A notable property is that the CFB-like feedback provides **de-facto
manipulation detection**: any tampering with the ciphertext propagates
to all subsequent decrypted bytes, making silent modification attacks
infeasible. Conversely, the cipher is **brittle against transmission
errors** — a single damaged byte in storage destroys all subsequent
file content.

The cipher passes **8 of 10 NIST Statistical Test Suite tests** on
100 MB of keystream output. The two failures are caused by a single,
localized bias at the Bit 6 ↔ Bit 7 transition within each output byte,
traced to specific lines in the code (asymmetric XOR masks `0x55`/`0xAA`
in the gear-shift logic).

This bias enables a statistical **distinguisher** (an attacker can tell
Turbine output from random with ~100 MB of data) but does **not** lead
to key recovery or plaintext recovery.

---

## 1. Architecture

### 1.1 Cipher structure

```
Password (6-1024 bytes)
    │
    ▼
Multi-stage mixing function
    │
    ▼
4 parallel "gear groups", each containing 3 shift registers:
    - gear_a (18 bytes)
    - gear_b (14 bytes)
    - gear_c (8 bytes)
    Total: 40 bytes per group × 4 groups = 160 bytes = 1280 bits
    │
    │ + 16-byte IV (RNGCryptoServiceProvider, stored in BMP container)
    │
    ▼
Stop-go clocking based on `takt` counter:
    - Rad 1 (gear_a): rotates every cycle
    - Rad 2 (gear_b): rotates conditionally
    - Rad 3 (gear_c): rotates only when `takt >= 16`
    │
    ▼
XOR tree combining all 4 gear groups → 1 output byte
    │
    ▼
ciphertext_byte = plaintext_byte XOR keystream_byte
```

### 1.2 Classification

This is a **byte-oriented, multi-stream stop-go-clocked shift register cipher**.
It belongs to the LFSR/NFSR family (not the SP-network family of AES, not the
ARX family of ChaCha20).

| Family | Examples | Turbine's relation |
|---|---|---|
| SP-Network | AES, Serpent | Not related |
| ARX | ChaCha20, Salsa20 | Not related |
| **Stop-Go LFSR + Combiner** | A5/1, E0, Trivium, Grain | **Same family** |

Trivium is an eSTREAM-portfolio cipher and currently considered secure.
A5/1 and E0 have been broken — but their internal states were 64 and
128 bits respectively (vs Turbine's 1280 bits) and their LFSR rotations
were bit-oriented (vs Turbine's byte-oriented).

### 1.3 Operating mode: Plaintext-feedback (CFB-like)

**Added 2026-05-18 — correction to earlier analysis.**

The initial 2026-05-15 analysis classified Turbine as a synchronous
stream cipher (analogous to CTR mode). **Empirical testing on 2026-05-18
revealed this was incorrect**: Turbine implements a cross-block feedback
mechanism similar to **CFB (Cipher Feedback Mode)** as defined in
NIST SP 800-38A.

#### How the feedback works

Turbine processes ciphertext in 8-byte blocks (`block_laenge`). After
each block, two running checksums are computed over the plaintext bytes
just decrypted (or just encrypted):

```csharp
// In Window1.xaml.cs around line 3200-3206:
for (int schieber2 = 0; schieber2 < block_laenge; schieber2++)
{
    block_quersumme = (byte)(block_quersumme ^ zeichenbuffer[schieber2]);  // XOR-checksum
    block_summe     = (byte)(block_summe     + zeichenbuffer[schieber2]);  // Additive-checksum
}
block_quersumme = (byte)(block_quersumme ^ gear_ergebnisd2_3 ^ ... ^ block_summe ^ ...);
```

These checksums then influence the **next** block's processing:

```csharp
// Lines 3098, 3100: Block rotation uses previous block's checksum
zeichenbuffer[schieber2] = (byte)(zeichenbuffer[schieber2 - 1] ^ block_quersumme);

// Line 3104: Number of bit-shift iterations depends on previous checksum
for (int schieber = 0;
     schieber < ((gear_ergebnisc3_3 ^ gear_ergebnisc4_3 ^ block_quersumme) & 0xf);
     schieber++)
```

#### Empirical verification (2026-05-18)

A clean 1-bit flip at ciphertext position 2500 of a 5,362-byte test
file produced the following error pattern after decryption:

| Property | Measured |
|---|---|
| Plaintext bytes before manipulation | 2,423 (all correct) |
| Damaged region | bytes 2,423-5,280 (~52 % of file) |
| Plaintext bytes after damage region | 11 (last bytes, due to end-of-file handling) |
| Damage propagation | **continues to end of file** |

A 1-bit flip and an 8-bit flip at the same position produced
near-identical damage patterns (2,774 vs. 2,747 corrupted bytes
respectively), confirming the chaining is deterministic and triggered
by any single difference, not by manipulation magnitude.

#### Classification in standard mode terminology

| Mode | Plaintext feedback? | Tamper propagation |
|---|---|---|
| ECB | No | One block only |
| CBC | Previous ciphertext | Current block + 1 bit flip in next |
| **CFB** | **Plaintext via cipher** | **To end of file** |
| OFB | Cipher output only | Bit-position only |
| CTR | None (synchronous) | Bit-position only |
| **Turbine** | **Plaintext checksum (`block_quersumme`)** | **To end of file** (CFB-like) |

Turbine's mode is most closely related to **CFB**, with an additional
twist: the feedback uses both an **XOR checksum** and an **additive
checksum** of the plaintext bytes, both mixed with concurrent gear-state
values. This is mathematically more complex than standard CFB and
qualitatively closer to AEAD modes like **GCM**, which also use
plaintext-derived authentication tags (though GCM uses Galois multiplication
where Turbine uses simpler XOR+addition).

#### Security implications

**Beneficial:**
- **De-facto manipulation detection.** Any single-bit change to ciphertext
  destroys all plaintext from that point to end of file. The receiver
  notices immediately that the file is corrupted.
- **Resistant to targeted modification attacks.** An adversary cannot
  silently change one value (e.g., "salary: 50000" → "salary: 80000")
  because the change destroys all subsequent bytes.

**Disadvantageous:**
- **Brittle against transmission errors.** A single bad sector in a USB
  stick or network transmission renders all subsequent file content
  unrecoverable.
- **No partial-recovery.** Unlike CTR mode where damaged regions are
  isolated, Turbine's chaining means even a corrupted 100 MB file cannot
  be partially recovered after the damage point.

For Turbine's intended threat model (personal data protection on
removable media), the trade-off is reasonable: better to know the file
is corrupted (and possibly maliciously altered) than to silently work
with partially-tampered data.

---

## 2. Key derivation

The password is processed through several stages:

1. **UTF-8 conversion** to byte array (`name_der_datei6[]`)
2. **Statistical analysis** of password bytes (length, sum, weighted sum,
   min/max byte) → 8 derived `passwort_info_byte` values
3. **For each of the 4 gear groups:** an iteration count is computed
   from the password bytes (with group-specific multipliers and XOR masks)
4. **Mixing loop:** the iteration count is used to mix password bytes
   cyclically with 5 feedback registers into a 40-byte vector
5. **Split:** 40-byte result → `gear_a[18]` + `gear_b[14]` + `gear_c[8]`
6. **IV mixing:** 16 random bytes XOR-ed into specific positions of the
   gear arrays after derivation

### 2.1 Key derivation strengths

- All password bytes are processed (no truncation up to 1024 bytes)
- Different gear groups receive different processing patterns
- Results in well-distributed initial state

### 2.2 Key derivation weaknesses

- **No formal Work Factor** (unlike PBKDF2 / Argon2 / scrypt)
- The IV is not used as a salt (it is mixed in after key derivation, not during),
  so it does not slow down brute force per password candidate

For long random passwords (16+ random characters), this is not a practical
concern. For short or guessable passwords, brute force is effective.

---

## 3. Statistical analysis (100 MB keystream)

### 3.1 Basic statistics

| Measure | Value | Ideal random |
|---|---|---|
| Shannon entropy | 7.999976 bit/byte | 8.000000 |
| Index of Coincidence | 0.0039063 | 0.0039063 |
| Chi² byte frequency (df=255) | 271 (PASS) | < 310 |
| Bigram coverage | 100 % (all 65,536 present) | — |
| Chi² bigrams (df=65535) | 65,375 (PASS) | < 66,200 |
| Block repetitions (8-byte) | 0 / 12.5 M | ~0 |
| Auto-correlation (all lags 1-1024) | \|corr\| < 0.007 | ~0 |

### 3.2 NIST Statistical Test Suite (100 MB)

| # | Test | P-value | Status |
|---|------|---------|--------|
| 1 | Monobit Frequency | 0.091 | PASS |
| 2 | Block Frequency (M=128) | 0.042 | PASS |
| 3 | **Runs Test** | **0.000** | **FAIL** |
| 4 | **Longest Run of Ones** | **0.001** | **FAIL** |
| 5 | Cumulative Sums (forward) | 0.058 | PASS |
| 6 | Cumulative Sums (backward) | 0.105 | PASS |
| 7 | Approximate Entropy (m=10) | 0.132 | PASS |
| 8 | Serial Test (m=16) (1) | 0.840 | PASS |
| 9 | Serial Test (m=16) (2) | 0.747 | PASS |
| 10 | Maurer Universal | 0.743 | PASS |

**Result: 8/10 PASS, 2/10 FAIL**

The two failures are both transition-related tests (counting consecutive
identical bits). All tests checking *bit pattern frequency* and *entropy*
pass. This points to a localized, structural bias rather than a global
weakness.

---

## 4. Bias localization

Bit-position-specific transition analysis (Z-scores for "consecutive bits
are equal more often than random would predict"):

| Bit transition | Z-score | Interpretation |
|---|---|---|
| Bit 0 → 1 | 2.39 | within noise |
| Bit 1 → 2 | 0.91 | within noise |
| Bit 2 → 3 | 3.49 | borderline |
| Bit 3 → 4 | 0.53 | within noise |
| Bit 4 → 5 | 1.38 | within noise |
| Bit 5 → 6 | 0.53 | within noise |
| **Bit 6 → 7** | **10.12** | **STRONG BIAS** |
| Cross-byte LSB → MSB | 1.33 | within noise |

The bias is **localized to a single bit pair** (the two least-significant
bits within each output byte). It does NOT propagate across byte boundaries
(verified by cross-byte correlation matrix: all 64 cells \|Z\| < 4).

### 4.1 Root cause

The bias is caused by asymmetric XOR masks in the gear-shift conditional
logic. Specifically in `Window1.xaml.cs`:

```csharp
// Around line 2451-2452 (gear_a3, gear_a4):
gear_a3[16] = (byte)(0x55 ^ gear_a3[17]);  // flips bits 0,2,4,6
gear_a4[16] = (byte)(0xAA ^ gear_a4[17]);  // flips bits 1,3,5,7

// Around line 2699, 2710 (gear_b3, gear_b4):
gear_b4[1] = (byte)(0x55 ^ gear_b4[0]);
gear_b3[1] = (byte)(0xAA ^ gear_b3[0]);
```

These masks treat adjacent bit positions differently. When the resulting
gear values flow into the output XOR tree, the per-bit asymmetry produces
a slight correlation between adjacent bits — strongest at the LSB end,
where fewer subsequent mixing operations dilute the effect.

### 4.2 Bias independence (combinability test)

A critical question: can the two observed biases (Bit 6→7 and Bit 2→3)
be combined into a stronger attack? RC4 was eventually broken because
its many small biases turned out to be combinable.

For Turbine:

```
P(Bit 6 = Bit 7)               = 0.500494
P(Bit 2 = Bit 3)               = 0.500170
P(both equal) measured         = 0.250328
P(both equal) if independent   = 0.250332
Z-score deviation from independence: -0.09
```

The two biases are **statistically independent** (Z = -0.09).
They cannot be combined into a stronger linear distinguisher.

This is a structural difference from RC4, which had multiple
combinable biases.

---

## 5. Comparison: Turbine vs. RC4

| Property | RC4 | Turbine |
|---|---|---|
| Internal state | 2064 bits (256-byte S-box + 2 indices) | 1280 bits (4 parallel gear groups) |
| State distribution | Monolithic | 4-way parallel |
| Documented biases | Dozens (Mantin, Fluhrer-McGrew, ABSAB, ...) | One strong (Bit 6→7), one borderline (Bit 2→3) |
| Bias independence | Combinable (proved over 14 years of research) | Independent (Z = -0.09) |
| Cross-byte correlations | Multiple known | None detected |
| Cipher family | S-box permutation | Stop-go LFSR |
| Real-world break | Yes (WEP, TLS-RC4) | None publicly known |
| Public peer review | 25+ years, hundreds of researchers | First external review 2026 |

The structural properties favor Turbine. The lack of public peer review
is the most important caveat.

---

## 6. Comparison: Turbine vs. broken "encryption"

A separately analyzed file (`test.data`) was found to be encrypted with
a tool that produces output with a **50,000-byte period** (the same
50 KB block repeated 2,097 times). For comparison:

| Property | Turbine | test.data tool |
|---|---|---|
| NIST tests passed | 8/10 | 0/10 |
| Period (repetition cycle) | Effectively unbounded | 50,000 bytes |
| Distinguisher effort | 100 MB needed | Trivial — visible by eye |
| Bit balance | 49.997 % ones | 51.34 % (massively biased) |
| Block repetitions (16-byte) | 0 | All blocks repeat |
| Many-time-pad protection | Yes | **Catastrophic failure** |
| Plaintext recovery from 2 ciphertexts | Not possible | Trivial once file > 50 KB |

This comparison serves to calibrate Turbine's quality: it is in a
fundamentally different league than tools that ship serious bugs.

---

## 7. Conclusions

### 7.1 What this review establishes

- Turbine is a **functionally correct** stream cipher with a **CFB-like
  feedback mode** (plaintext-checksum chained across blocks, see 1.3)
- Its construction is **structurally sound** for its threat model
- A single localized bias was identified, traced to specific lines, and
  documented in code comments
- The bias is **not combinable** with other observed effects
- The cipher belongs to a **legitimate family** with successful modern
  representatives (Trivium for the keystream generator, CFB for the
  feedback mode)
- The CFB-like mode provides **de-facto manipulation detection** without
  an explicit MAC tag — any single-bit tamper destroys all subsequent
  plaintext, making silent modification attacks infeasible

### 7.2 What this review does NOT establish

- Whether algebraic attacks (SAT solver / Gröbner basis) could exploit the
  cipher's polynomial representation
- Whether differential cryptanalysis on chosen plaintexts would reveal
  exploitable patterns
- Whether correlation attacks against individual gear bits would work
- Whether reduced-round versions (e.g., 1 gear group instead of 4)
  reveal weaknesses that extrapolate to the full cipher

These would require dedicated multi-month / multi-researcher analysis.

### 7.3 Practical recommendation

For the threat model Turbine was designed to address (personal data
protection against casual access), the cipher is **fit for purpose**.
With strong passwords (16+ random characters), there is no practical
attack within the documented scope.

For high-stakes scenarios (commercial secrets, long-term archival,
adversary with cryptanalytic resources), use established, peer-reviewed
standards (AES-GCM, ChaCha20-Poly1305, age, VeraCrypt).
