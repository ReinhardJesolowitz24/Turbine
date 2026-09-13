# AI on the defender's side: how AI-assisted cryptanalysis made a hobby cipher safer

*A realistic account — no hype. With a crypto tool, the honesty is the point.*

Turbine is a small, self-written, open-source file-encryption tool (a personal
project, not a company product). Over several sessions an AI assistant reviewed
it, **attacked it**, and helped harden it — acting entirely as a *defender's*
tool. Here is what that looked like, and — just as important — what it does and
does not prove.

---

## What the AI actually did

- **Found real, previously-undocumented issues** in the author's own code: a
  key-file mode that could derive a near-public key from a degenerate
  (solid-colour/synthetic) file, and a documented "tamper detection" property
  that **did not actually hold**.
- **Ran an attack — for real.** It reimplemented the cipher's keystream generator
  (validated byte-identical to the compiled code) and mounted an automated
  known-plaintext state-recovery attack (SMT / guess-and-determine).
- **Reported honestly, including against itself.** It confirmed one weakness
  empirically (the false tamper claim — demonstrated with crafted, silent edits)
  and **disproved part of its own hypothesis** about where the cipher was weak.
- **Built and verified fixes:** a real **HMAC-SHA-384** integrity tag
  (Encrypt-then-MAC, verified before decryption, fail-safe), a hardened per-file
  IV, an AES-independent S-box, and an entropy guard against degenerate key files
  — each tested end-to-end (round-trip byte-exact, tampered files rejected,
  backward compatibility preserved).
- **Corrected the documentation** where it had over-promised, and even caught and
  fixed a regression it had introduced.

## What this honestly proves — and what it does not

**It does show:**
- The cipher **resisted** a real, automated known-plaintext attack on a single
  workstation within a bounded compute budget. The load-bearing nonlinearity held.
- AI can do rigorous **defensive** security work — find issues, test them
  empirically, build fixes, verify them — on tools whose authors could never
  afford a professional audit.

**It does not show (and we will not claim):**
- That Turbine is "unbreakable." **Resisting one attack is not proof of
  security.** A bespoke attack, more compute, or a different technique could still
  succeed.
- That it is peer-reviewed or standards-grade. Turbine is a **personal-use** tool,
  not analysed like AES. **For high-stakes data, use an audited authenticated
  cipher** (AES-GCM, ChaCha20-Poly1305) or established tools (VeraCrypt, age,
  GnuPG).
- That AI is "only" for defence. It is dual-use — but defence has real structural
  advantages (below).

## Why this tilts toward the defender

- **Publish once, protect many.** A fix, open-sourced, protects everyone who
  downloads it — permanently. An attack must be re-run against each target.
- **Democratised expertise.** A cryptographer's review, an HMAC integration,
  reasoning about IV/nonce reuse — capabilities that used to need a funded team —
  are now within reach of a hobbyist or a small business, for free.
- **Transparency compounds.** Open source + AI review means anyone can verify —
  the opposite of the closed, unverifiable weakening that has historically burned
  users.
- **Human in the loop.** Every decision — what to fix, when to ship, the
  thresholds — stayed with the author. The AI was the amplifier, not the driver.

## Bottom line

A free, open tool gained a genuine integrity mechanism, a hardened IV, an entropy
guard, and honest documentation — and it withstood a real AI-run attack — because
AI helped the **defender**. That is the story worth telling: not *"unbreakable,"*
but *"made honestly safer — and anyone can do the same for their own tools."*

---

*Full trail (reproducible): the commit history, `CHANGELOG.md`, `SECURITY.md`,
and `IV_HARDENING.md` document each hypothesis, test, result, and fix.*
