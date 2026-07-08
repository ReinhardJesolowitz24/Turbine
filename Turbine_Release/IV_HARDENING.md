# IV Hardening — Investigation and Decision

**Date:** 2026-07-08
**Reviewer:** AI-assisted analysis (Anthropic Claude — *Fable 5* for the fresh
framing, *Opus 4.8* for execution)
**Scope:** A differential/diffusion study of Turbine's plaintext-feedback path,
and the design decision it motivated: hardening the per-file IV generation.

---

## Executive summary

A fresh high-level look at Turbine (Fable 5) raised one concrete, testable
observation: the existing statistical evidence (NIST SP 800-22 on **all-zero**
plaintext, see `NIST_TEST_RESULTS.md`) never exercises the cipher's
**plaintext-derived block feedback** — with all-zero input the block checksums
(`block_quersumme`, `block_summe`) are trivial, so their contribution to
diffusion is never measured. The open question: **how does a single-bit
plaintext change actually diffuse into the ciphertext, and is any weakness
structural (fixed location) or data-dependent (moves with the plaintext)?**

To answer this we built an isolated test environment, ran a large differential
sweep, and instrumented the cipher's internal feedback state. The findings:

1. **No structural defect / no fixed "hole"** in diffusion (0 of 512 positions
   weak across all 32 baselines).
2. Diffusion is **data-dependent and bounded**: a single-bit plaintext error
   propagates over a limited, data-dependent window (median ≈ 1.4 KB) and then
   the internal state **re-synchronizes** — the signature of a
   **self-synchronizing (CFB-like)** feedback design, consistent with the
   documented CFB mode (`CRYPTANALYSIS.md`, changelog "CFB 2026-05-18").
3. **Crucially, this whole analysis was only possible because the test build
   replaces the random IV with a constant.** A differential attack needs the
   *same keystream* for both encryptions. In the real cipher every file draws a
   fresh random IV, so a different keystream per file makes this
   differential/window analysis **infeasible**. The constant-IV test is, in
   effect, a simulation of **IV reuse**.

**The IV is the load-bearing protection that stands in this attack's way.**
That is precisely why we chose to *harden the thing the attack must defeat*: the
IV generation. See "Decision" below.

---

## What was investigated (and how)

All experimental work was done in a **separate test copy**
(`C:\Data\Turbine_Fable_Tests`), never in this repository. The test copy
contains changes that must **never** ship in production — a **constant IV**
(to make the keystream reproducible) plus a headless batch/trace harness.

- **Structured plaintext generator** — baseline files plus single-bit "flip"
  variants at block-aligned positions, periodic patterns, and ramps.
- **Headless encryption harness** — drives the *real* `DoWork` cipher routine
  (no re-implementation); validated as **byte-identical** to the GUI on 20 files.
- **32-baseline sweep** — 32 baselines × 512 flip positions = **16,384**
  encryptions. Metric: fraction of differing ciphertext bits from the first
  differing byte (`C_baseline XOR C_flip`). Ideal ≈ 50 %.
- **Internal instrumentation** — a trace hook (no-op in normal operation;
  output verified bit-identical) logging per-block `length, quersumme, summe` to
  compare a baseline against each flip at the level of internal feedback state.
- **Window distribution** — first-to-last differing ciphertext byte per flip,
  aggregated over all baselines.

## Findings (evidence)

| Category (of 512 positions) | Count |
|---|---|
| structurally weak (< 20 % in *all* 32 seeds) | **0** |
| data-dependent (weak for some seeds, strong for others) | 473 (92 %) |
| strong/ok (≥ 20 % in all 32 seeds) | 39 (8 %) |
| **global mean diffusion (from onset)** | **36.2 %** |

Internal trace (4096-byte file = 712 blocks):

| Case | Diverging blocks | Contiguous | Re-synchronizes afterward |
|---|---|---|---|
| strong @ byte 0 | all 712 | yes | — (permanent) |
| weak @ byte 16 | 83 (blocks 1–83) | yes | **yes** |
| weak @ byte 1024 | **4** (blocks 164–167) | yes | **yes** |

Error-propagation window over 16,384 flips: **median 1392 B, mean 1593 B**
(~39 % of a 4096 B file); only **0.2 %** propagate to end-of-file (permanent);
**14 %** stay ≤ 256 B (effectively local).

**Interpretation:** This is not a bug or a break. It is a *characterization* of
the mixing behavior that the all-zero statistic could never show: bounded,
data-dependent error propagation with re-convergence, i.e. self-synchronizing
(CFB-like) feedback — as opposed to the full, permanent avalanche of an ideal
design. Notably, the same feedback gives Turbine its **de-facto tamper
detection** (`CRYPTANALYSIS.md`): the property is a deliberate trade-off, not a
flaw.

---

## Decision: harden the IV generation

The study makes the dependency explicit: **the random per-file IV is what makes
the above analysis (and IV-reuse-style attacks in general) infeasible.** So we
strengthen exactly that.

**Threat considered (defense-in-depth):** a manipulated or predictable OS
CSPRNG. The historical precedent is **Dual_EC_DRBG** — a NIST-standardized RNG
with a suspected NSA backdoor, shipped as the default in RSA BSAFE (Reuters/
Snowden 2013; NIST withdrawal 2014). Windows CNG does **not** use Dual_EC, so
the realistic risk here is low — but the hedge is cheap and the failure mode
(single source of randomness) is a classic **common-cause failure**.

**Approach:** derive the 16-byte IV from **two independent sources** combined by
a SHA-256 randomness extractor:

```
IV = SHA-256( CNG(32 B)  ‖  Jitter(32 B) )[0..15]
       │ source 1: OS CSPRNG          │ source 2: timing jitter + environment
       │ (BCryptGenRandom)            │ (independent of the CNG pool)
```

- **Independence is the point.** Source 2 is timing jitter (nanosecond
  `Stopwatch` deltas across deterministic work) plus environment values —
  deliberately **not** `Guid.NewGuid()`, which is itself CNG-based on Windows and
  therefore *not* independent. As long as **one** source is unpredictable, the
  SHA-256 output is unpredictable, even if the other source is fully compromised.
- **Low-tech by design.** Timing jitter is a *weak* entropy source in absolute
  terms, but it is clearly independent and requires no native interop. A stronger
  independent source (RDRAND hardware RNG) was considered and rejected for now: it
  needs native interop (no managed intrinsic on .NET Framework 4.8) for marginal
  benefit. Source 2 is a **hedge, not a replacement** — CNG remains primary.
- **Format-transparent.** No new version/variant byte. Only the *generation* of
  the 16 IV bytes changes; the IV is written to the file and read back on
  decryption exactly as before. **All existing files remain decryptable.**

**Implementation:** `GenerateIV16()` in `Window1.IvHardening.cs`; call site at
`Window1.xaml.cs` (the single per-file IV generation point).

**Validation** (self-test in the test copy, `--rngtest 20000`):

| Metric | Result | Ideal |
|---|---|---|
| per-byte mean | 127.48 | 127.50 |
| χ²(255 df) | 240.5 | ~200–310 |
| duplicate IVs | 0 | 0 |
| **backdoor demo:** CNG forced to 0 → IVs still all differ | ✔ | — |

The backdoor demo is the key check: with the CNG contribution pinned to a
constant (simulating a fully predictable OS RNG), the generated IVs still differ
across calls — the independent jitter source neutralizes the compromise.

---

## Limitations / honest scope

- Sweep covered **Bit 0** and **block-aligned** positions only.
- Timing jitter is a weak (if independent) source; this is defense-in-depth, not
  a claim that the OS RNG is untrustworthy.
- The bounded-propagation property still exists internally; it is simply not
  observable/exploitable via ciphertext differences without IV reuse.
- The constant-IV build and all test hooks live **only** in the separate test
  environment and are **not** part of this repository.

---

*Reproducibility (test environment):* `generator.py --sweep` →
`Turbine.exe --batch` → `analyzer.py --sweep` / `--window`; single trace via
`Turbine.exe --trace`; IV self-test via `Turbine.exe --rngtest`.
