---
name: ado-bug-to-draft-pr
description: Track a repository bug in Azure DevOps, prepare a fix branch, and open a draft GitHub pull request using the repository template.
---

When asked to track a bug and prepare its fix branch and pull request:

1. Inspect the repository, its Azure DevOps project, the default branch, pull request template, and current Git status. Preserve unrelated work. If the working tree has unrelated edits, use a separate worktree.
2. Create an Azure DevOps Bug with a concise title and clear expected and actual behavior. Assign it to the current signed-in user; verify the identity rather than guessing. Record its ID and URL.
3. Create a branch from the repository's current default branch using `ep/YYYY-MM-<kebab-case-bug-description>`.
4. Implement the requested fix and run the smallest relevant checks. Do not include unrelated changes.
5. Use `.github/PULL_REQUEST_TEMPLATE.md` as the PR body, filling every relevant section. Link the Azure DevOps item using `AB#<ID>`, use the repository's required title prefix, and target the repository's default branch.
6. Push the branch and create the pull request as a draft. Verify the bug assignment, branch, PR URL, draft status, and work-item link.

If Azure DevOps or GitHub write tools or authentication are unavailable, do not claim remote work was completed. Finish safe local preparation where possible and report exactly which remote actions remain blocked.
