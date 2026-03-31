# Shadow Clone - Development Roadmap

## Project Goal

Ship a polished, portfolio-ready Unity mini-game in roughly 14 days by focusing on one standout mechanic: record movement, replay it as a single clone, and solve simple logic rooms.

## Development Principles

- Optimize for finished quality, not feature count
- Prefer simple systems that are easy to debug
- Build one playable path first
- Keep every phase shippable
- Avoid over-architecting

## Phases

### Phase 0 - Foundation

Objective:
Create a clean repo, define scope, choose Unity version, and establish project structure.

Why it matters:
A small project benefits from clarity early. Clean structure prevents confusion and makes the repo look professional.

Key outputs:

- Repository docs
- Unity project setup
- Folder conventions
- Initial scenes and naming rules

### Phase 1 - Core Player

Objective:
Implement stable 2D movement and room reset flow.

Why it matters:
The clone mechanic only feels good if basic movement is readable and reliable.

Key outputs:

- Horizontal movement
- Jump
- Ground check
- Camera follow if needed
- Death / reset baseline

### Phase 2 - Clone System

Objective:
Build the MVP record and replay loop.

Why it matters:
This is the portfolio hook and the entire project should center on it.

Key outputs:

- Position sampling
- Recording state
- Single clone spawn
- Replay timing
- Reset-safe cleanup

### Phase 3 - Puzzle Objects

Objective:
Add simple interactive objects that make the clone meaningful.

Why it matters:
Without room logic, the clone is only a tech demo.

Key outputs:

- Button system
- Door system
- Hazard logic
- Exit goal

### Phase 4 - Game Flow and UI

Objective:
Wrap the mechanic in a playable user flow.

Why it matters:
Recruiters and players should be able to launch, play, restart, and finish without friction.

Key outputs:

- Main menu
- Pause menu
- Win / completion flow
- HUD prompts

### Phase 5 - Level Design

Objective:
Build 3 to 4 short levels that teach and test the mechanic.

Why it matters:
The portfolio value comes from demonstrating design progression, not just systems.

Key outputs:

- Tutorial room
- Mid-complexity rooms
- Final challenge room

### Phase 6 - Audio and Polish

Objective:
Improve clarity, feel, and presentation without expanding scope.

Why it matters:
Small polish passes make portfolio work feel dramatically more credible.

Key outputs:

- Sound effects
- Basic particles / feedback
- Better UI readability
- Consistent visual tone

### Phase 7 - QA, Packaging, and Publish

Objective:
Stabilize the game and package it for portfolio presentation.

Why it matters:
A finished build, strong README, and captured media matter as much as the code.

Key outputs:

- Final bug pass
- Windows build
- Screenshots and GIF/video
- Portfolio-ready GitHub presentation

## Timeline Guidance

Suggested 14-day pacing:

- Days 1 to 2: Foundation
- Days 3 to 4: Core Player
- Days 5 to 7: Clone System
- Days 8 to 9: Puzzle Objects and flow
- Days 10 to 11: Levels
- Day 12: Audio and polish
- Days 13 to 14: QA, build, media, publish

## Working Rules

- If a feature adds complexity without improving the core mechanic, cut it.
- If a bug threatens the replay loop, fix it before adding content.
- If a level idea needs a new system, redesign the level instead of growing the codebase.

See [docs/ISSUES_BACKLOG.md](docs/ISSUES_BACKLOG.md) for the full implementation backlog.
