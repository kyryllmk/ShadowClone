# Shadow Clone - Session Prompt

Use this file as the universal session checklist for all future work on the project.

## Start of Session

```
Shadow Clone session start

1. Read docs/REPO_WORKFLOW.md and follow the branch/commit rules.
2. Identify the exact issue being worked on and keep scope tight.
3. Confirm the current branch is correct, or create a new feature branch from dev.
4. Review which gameplay docs or repo docs may need updates.
5. Build the smallest finished improvement that moves the project forward.
6. Avoid scope creep. If a new idea needs a new system, defer it unless it is required for the current issue.
```

## End of Session

```
Shadow Clone session end

1. Verify the current task is done, or leave a clear note about what remains.
2. Perform a quick quality check on the changed work.
3. Update affected docs, including docs/CHANGELOG.md when appropriate.
4. Commit using the repo convention: type(scope): short description.
5. Push the branch to remote.
6. Open a PR to dev, or update the existing PR.
7. Include a short PR summary:
   - what changed
   - why it matters
   - how it was checked
   - what follow-up work remains
8. Use `.github/pull_request_template.md` for the PR body.
```

## Example Closeout

Example commit:

`feat(clone): add single replay actor prototype`

Example PR title:

`SC-08 Implement single clone replay actor from recorded positions`
