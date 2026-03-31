# Shadow Clone - Repo Workflow

## Universal Session Prompt

Use the following operating prompt at the start of every work session and review it again before ending the session.

### Session Start Prompt

```
Shadow Clone session start

1. Read docs/REPO_WORKFLOW.md before making changes.
2. Review the current issue or task and keep scope limited to the smallest shippable result.
3. Confirm the branch matches the task. If needed, create or switch to a feature branch from dev.
4. Check which docs must be updated alongside the implementation.
5. Work in a portfolio-first way: prioritize clarity, finish, and reliability over extra features.
6. Before ending the session, leave the branch in a professional state:
   - update relevant docs
   - update docs/CHANGELOG.md if the change is meaningful
   - commit with the project commit convention
   - push the branch
   - open or update a PR into dev
```

### Session End Prompt

```
Shadow Clone session end

Before closing the session:

1. Verify the task is actually done or clearly note what remains.
2. Run a quick self-check on the changed work.
3. Update any affected documentation.
4. Add a concise changelog note if the work changed the project meaningfully.
5. Create a clean commit using type(scope): short description.
6. Push the branch to the remote repository.
7. Open a PR to dev, or update the existing PR for that branch.
8. Write a short PR summary that explains:
   - what changed
   - why it matters
   - how it was checked
   - any known follow-up work
```

## Branching Strategy

Recommended branches:

- `main`: Always reflects the latest stable state
- `dev`: Integration branch for current work
- `feature/<area>-<task>`: Short-lived work branches

Feature branch examples:

- `feature/player-movement`
- `feature/clone-recording`
- `feature/button-door-logic`
- `feature/ui-main-menu`
- `feature/level-tutorial`

How to merge:

- Branch from `dev` for active feature work
- Keep branches focused on one issue or one tightly related task
- Merge into `dev` after local testing
- Merge `dev` into `main` at the end of a milestone or release-ready checkpoint
- At the end of each completed session, commit, push, and create or update a PR into `dev`

Tags and releases:

- Create a lightweight tag at milestone-complete builds if useful
- Create a release tag for the public portfolio version, for example `v0.1.0`

## Commit Message Convention

Format:

`type(scope): short description`

Recommended types:

- `feat`
- `fix`
- `docs`
- `refactor`
- `test`
- `ui`
- `chore`

Examples:

- `feat(player): add horizontal movement and jump`
- `fix(clone): correct replay timing drift`
- `docs(readme): add build instructions`
- `ui(menu): create pause screen layout`
- `feat(level): add tutorial room blockout`
- `fix(reset): clear clone on room restart`
- `feat(puzzle): add pressure button trigger`
- `docs(roadmap): add milestone plan`
- `chore(unity): update project settings`
- `test(build): verify windows export flow`

## Lightweight Working Rules

- One issue per branch when possible
- One logical change per commit
- Update docs when scope changes
- Keep README current once the game becomes playable
- End every completed session with a clean commit, push, and PR update

## Pull Request Template

GitHub PRs should use the repository template at:

- `.github/pull_request_template.md`

This keeps every PR aligned with the session-end prompt:

- what changed
- why it matters
- how it was checked
- what follow-up work remains
