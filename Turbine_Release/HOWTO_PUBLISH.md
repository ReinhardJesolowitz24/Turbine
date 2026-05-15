# How to Publish Turbine — A Guide for Non-Developers

This release package is **self-contained**. You can share it as a ZIP file
via any channel you choose, or upload it to a hosting platform. Below are
several options, ordered from "lowest barrier to entry" to "best long-term
visibility".

---

## Step 0: Prepare your release

Before publishing, do these one-time steps:

### Replace placeholders

Search and replace the following in **all** documents:

| Placeholder | Replace with |
|---|---|
| `<DEIN VOLLER NAME>` | Your full name |

Files containing the placeholder:
- `LICENSE`
- `README.md`
- `README_DE.md`

You can do this with any text editor (Notepad, Notepad++, VS Code, etc.).

### Verify the package

Open `binary/Turbine.exe` once to make sure it works. Try encrypting a small
test file and decrypting it back. This confirms the package is intact.

### Create a ZIP

Right-click on the `Turbine_Release` folder → "Send to" → "Compressed
(zipped) folder". Result: `Turbine_Release.zip` (~2 MB).

---

## Option 1: Just share the ZIP (easiest, ~5 minutes)

**For:** Sharing with friends, family, small communities

**Channels:**
- E-mail attachment
- Cloud storage (Dropbox, OneDrive, Google Drive) with public link
- USB stick
- Personal website upload

**Pros:** No account needed anywhere, full control, no third party
**Cons:** Limited reach, no version tracking, manual updates

---

## Option 2: Internet Archive (good for long-term preservation, ~20 minutes)

**For:** Permanent archival, lasting reach, no commercial dependency

**Steps:**
1. Go to https://archive.org and create a free account (only e-mail needed)
2. Click "Upload" → "Upload Files"
3. Choose your ZIP file
4. Fill in metadata:
   - **Title:** `Turbine — Personal File Encryption Tool`
   - **Subject tags:** `encryption`, `cryptography`, `windows`, `dotnet`, `open-source`
   - **Description:** Copy a few sentences from `README.md`
   - **Creator:** Your name
   - **Date:** 2026-05-15 (or whenever you publish)
   - **License:** MIT
5. Upload (takes 2-5 minutes for 2 MB)
6. You get a permanent URL like `https://archive.org/details/turbine-encryption-tool`

**Pros:** Permanent archival (Archive.org keeps things forever),
no maintenance needed, search-engine-indexed
**Cons:** Less developer-focused than GitHub

---

## Option 3: SourceForge (classic open-source hosting, ~30 minutes)

**For:** Reaching the established Windows-software user community

**Steps:**
1. Go to https://sourceforge.net/user/registration and create an account
2. Click "Create Project"
3. Project name: `turbine-encryption` (or similar)
4. Upload your ZIP file under "Files"
5. Paste your README content into the project description

**Pros:** Indexed by every Windows-software directory,
users find it via search engines, download statistics included
**Cons:** Older platform, sometimes flagged by browsers (cosmetic)

---

## Option 4: GitHub (best developer reach, ~45 minutes if new to Git)

**For:** Maximum visibility in developer/security community,
issue tracking, version control, collaboration

**Account creation (one-time, ~5 minutes):**
1. Go to https://github.com/signup
2. Free account is sufficient (no credit card needed)
3. Verify your e-mail

**Repository creation (~10 minutes):**
1. Click the "+" in the top-right → "New repository"
2. Repository name: `turbine`
3. Description: from `README.md` first paragraph
4. Public (so others can see and download)
5. Skip "Initialize this repository with a README" — you already have one
6. Click "Create repository"

**Upload (~15 minutes if Git is new to you):**

GitHub now offers the easiest method:
1. On your new repository page, click "uploading an existing file"
2. Drag the **contents** (not the parent folder) of `Turbine_Release` into
   the upload zone:
   - `LICENSE`, `README.md`, `README_DE.md`, etc. (all files at top level)
   - The `src/` folder
   - The `binary/` folder
   - The `docs/` folder
3. Wait for upload (~1 minute for ~2 MB)
4. Scroll down, write commit message: "Initial release"
5. Click "Commit changes"

**Optional polish:**
- Edit the `README.md` directly on GitHub to add a project image / logo
- Use GitHub Releases to mark this as "v1.0" with a download
- Enable "Issues" so users can report problems

**Pros:** Best discoverability, version history, issue tracker,
people can suggest changes via Pull Requests
**Cons:** New account and concept (Git) to learn

---

## Recommendation

For you specifically, given you're not currently registered on GitHub:

**Easiest first step:** Internet Archive. 20 minutes, permanent URL,
maintenance-free.

**If you want to grow it over time:** Internet Archive *now* +
GitHub *later* when you're comfortable. Both can coexist with the same code.

---

## After publishing

Some optional things to do:

- **Tell people who might benefit.** A short post on a forum where USB-stick
  encryption is discussed (e.g., r/privacy on Reddit) can reach the right
  audience.
- **Keep the source on your own backup.** Never rely solely on a hosting
  platform — keep your own copy.
- **If new findings emerge** about the cipher: update `SECURITY.md` and
  `CRYPTANALYSIS.md` and republish.
- **If you fix bugs or add features:** Increment the version in
  `CHANGELOG.md` and release a new version.

---

## What you do NOT need to do

- You do NOT need to respond to every issue or e-mail. This is your
  personal project, you owe the public nothing beyond the code itself.
- You do NOT need to provide commercial support.
- You do NOT need to compete with VeraCrypt or other professional tools.
  Your value proposition is honest transparency for personal use.

---

## Final note

The most important thing about publishing this:

> **You are providing a transparent, no-strings-attached tool that any
> person on Earth can use to protect their personal files.** The technical
> details matter less than this fact. Many people lack the resources to
> evaluate commercial encryption claims. Open-source releases like this one
> give them an option they can verify themselves (or have a trusted person
> verify for them).
>
> That is a contribution worth making.
