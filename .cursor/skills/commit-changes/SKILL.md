---
name: commit-changes
description: >-
  Commit current git changes with a short one-sentence summary. Use when the
  user asks to commit, save work to git, write a commit message, or commit
  current/staged changes.
disable-model-invocation: true
---

# Commit Changes

Commit the current git changes and return a **one-sentence commit summary**.

## Workflow

### 1. Gather context (run in parallel)

```bash
git status -sb
git diff
git diff --cached
git log -10 --format='%s'
```

Also use chat history for **why** the change was made; use the diff for **what** changed.

### 2. Decide what to commit

- Stage only files that belong to the current work.
- **Do not commit** secrets (`.env`, credentials, keys) — warn the user instead.
- **Skip by default** unless the user explicitly wants them:
  - `ProjectSettings/Packages/com.unity.ai.assistant/Settings.json`
  - Other local editor/IDE-only noise unrelated to the task
- If there is nothing to commit, say so and stop — do not create an empty commit.

### 3. Write the commit message

**Format:** exactly **one sentence**, imperative mood, ≤ ~72 characters when possible.

- Focus on **why** or the user-visible outcome, not file names.
- Match recent repo tone when sensible (plain sentence or `fix:` / `feat:` prefix).
- Do not add a body, bullet list, or second paragraph.

**Examples:**

| Change | Message |
|--------|---------|
| Store menu purchase flow fix | Fix store purchase button staying disabled after retry |
| New gesture hint config | Add gesture hint configuration for first-time players |
| Refactor only, same behavior | Refactor store menu controller to simplify tab switching |

### 4. Commit (sequential)

```bash
git add <relevant paths>
git commit -m "$(cat <<'EOF'
One-sentence commit message here.

EOF
)"
git status -sb
```

### 5. Return to the user

Reply with:

1. **Commit summary** — the one-sentence message (same as commit subject).
2. **Commit hash** — short SHA from `git log -1 --format='%h'`.
3. **Files committed** — brief list or count.

Do **not** push unless the user explicitly asks.

## Git safety

- **Never** update git config.
- **Never** run destructive git commands (`push --force`, `reset --hard`, etc.) unless explicitly requested.
- **Never** skip hooks (`--no-verify`, `--no-gpg-sign`) unless explicitly requested.
- **Never** force-push to `main`/`master` — warn if requested.
- **Avoid** `git commit --amend` unless all are true:
  1. User explicitly requested amend, **or** a hook auto-modified files after a successful commit you just made
  2. HEAD was created by you in this conversation
  3. Commit has **not** been pushed
- If commit **fails** or a hook **rejects** it: fix the issue and create a **new** commit — never amend a failed commit.
- **Never** use interactive git flags (`-i`).

## Do not

- Commit without analyzing the diff first.
- Invent changes not present in the diff.
- Push, open PRs, or amend unpushed commits unless explicitly asked.
