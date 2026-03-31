# Shadow Clone - GitHub Issue Backlog

## Priority Scale

- `P0`: Critical for MVP
- `P1`: Important for quality and completion
- `P2`: Nice to have if time allows

## Effort Scale

- `S`: Small
- `M`: Medium
- `L`: Large

## Phase 0 - Foundation

### SC-01 - Set up Unity project structure and repository docs

- Goal: Create the initial repo layout, documentation system, and Unity folder conventions.
- Tasks:
  - Add README, docs pack, license, and Unity `.gitignore`
  - Define `Assets/` folder structure
  - Confirm naming conventions for scenes and scripts
- Deliverables:
  - Professional repository foundation
  - Documented project scope and roadmap
- Acceptance Criteria:
  - Core documentation files exist
  - Repo is understandable to a first-time visitor
  - Unity project structure is defined
- Dependencies: None
- Priority: `P0`
- Effort: `S`

### SC-02 - Create initial Unity scenes and bootstrap flow

- Goal: Set up the first menu scene, placeholder gameplay scene, and scene ordering plan.
- Tasks:
  - Create main menu scene
  - Create first gameplay scene
  - Add scenes to build settings
- Deliverables:
  - Basic scene skeleton
  - Clear scene naming
- Acceptance Criteria:
  - Project opens to a valid menu scene
  - Gameplay scene can be loaded from menu or editor
- Dependencies: `SC-01`
- Priority: `P0`
- Effort: `S`

### SC-03 - Define coding standards and script ownership boundaries

- Goal: Keep future implementation readable and avoid oversized scripts.
- Tasks:
  - Document script responsibility rules
  - Define namespaces or naming style if used
  - Confirm manager usage stays minimal
- Deliverables:
  - Lightweight architecture agreement
- Acceptance Criteria:
  - Team-of-one rules are written and practical
  - Future systems can be added without confusion
- Dependencies: `SC-01`
- Priority: `P1`
- Effort: `S`

## Phase 1 - Core Player

### SC-04 - Implement horizontal movement and jump controller

- Goal: Build a responsive 2D player controller suitable for puzzle-platforming.
- Tasks:
  - Add move input
  - Add jump
  - Tune movement values for readability
- Deliverables:
  - Playable movement controller
- Acceptance Criteria:
  - Player can move and jump reliably
  - Controls feel stable enough for puzzle timing
- Dependencies: `SC-02`
- Priority: `P0`
- Effort: `M`

### SC-05 - Add grounded check, spawn point, and room reset baseline

- Goal: Support stable jumping and fast reset behavior.
- Tasks:
  - Implement grounded detection
  - Add spawn point logic
  - Add manual reset input
- Deliverables:
  - Core fail-and-retry loop
- Acceptance Criteria:
  - Player respawns at the correct point
  - Manual reset works without reloading the editor
- Dependencies: `SC-04`
- Priority: `P0`
- Effort: `M`

### SC-06 - Create simple camera framing and player feedback hooks

- Goal: Keep player visibility clear and prepare for polish.
- Tasks:
  - Add basic camera follow or locked framing
  - Add placeholders for landing, facing, and state visuals
- Deliverables:
  - Clear room presentation
- Acceptance Criteria:
  - Player remains readable during normal gameplay
  - Camera behavior does not distract from puzzle logic
- Dependencies: `SC-04`
- Priority: `P1`
- Effort: `S`

## Phase 2 - Clone System

### SC-07 - Build record state controller with timed sampling

- Goal: Record the player path using a simple, dependable sampled timeline.
- Tasks:
  - Start and stop recording
  - Sample player positions at a fixed interval
  - Store the recording in a reusable format
- Deliverables:
  - MVP recording system
- Acceptance Criteria:
  - Recording captures a short movement path correctly
  - Recording length is limited and visible in debug form
- Dependencies: `SC-05`
- Priority: `P0`
- Effort: `L`

### SC-08 - Implement single clone replay actor from recorded positions

- Goal: Replay the recorded path with one clone actor.
- Tasks:
  - Spawn clone prefab
  - Move clone along recorded positions
  - End replay cleanly
- Deliverables:
  - Working replay clone
- Acceptance Criteria:
  - Clone follows the recorded route closely
  - Replay works repeatedly without duplicate clones
- Dependencies: `SC-07`
- Priority: `P0`
- Effort: `L`

### SC-09 - Add recording and replay input flow with visual feedback

- Goal: Make the core mechanic understandable to the player.
- Tasks:
  - Bind inputs for record and replay
  - Add simple UI or effects for recording state
  - Add blocked-state handling when replay is unavailable
- Deliverables:
  - Usable mechanic UX
- Acceptance Criteria:
  - Player can understand when recording starts, ends, and replays
  - Invalid input states are handled cleanly
- Dependencies: `SC-08`
- Priority: `P0`
- Effort: `M`

### SC-10 - Stabilize clone cleanup and reset integration

- Goal: Ensure clone state behaves correctly during retries and failures.
- Tasks:
  - Clear clone on reset
  - Reset recording state
  - Handle replay interruption safely
- Deliverables:
  - Reliable iteration loop
- Acceptance Criteria:
  - Repeated resets do not leave stale clone objects
  - Recording and replay states recover correctly after death
- Dependencies: `SC-08`, `SC-05`
- Priority: `P0`
- Effort: `M`

## Phase 3 - Puzzle Objects

### SC-11 - Create pressure button interaction

- Goal: Add a simple trigger object that can be activated by gameplay actors.
- Tasks:
  - Create button prefab
  - Detect player or clone overlap
  - Expose active state
- Deliverables:
  - Reusable button object
- Acceptance Criteria:
  - Button changes state when activated
  - Button can be used by at least one level prototype
- Dependencies: `SC-08`
- Priority: `P0`
- Effort: `M`

### SC-12 - Create linked door system

- Goal: Build a door that responds to button state and blocks progression when closed.
- Tasks:
  - Create door prefab
  - Link door to button
  - Update collider and visuals on state change
- Deliverables:
  - Functional button-door puzzle pair
- Acceptance Criteria:
  - Door opens and closes correctly from linked inputs
  - Collision matches visible door state
- Dependencies: `SC-11`
- Priority: `P0`
- Effort: `M`

### SC-13 - Implement hazard death and automatic room reset

- Goal: Add failure states that support quick retry.
- Tasks:
  - Create hazard prefab
  - Detect lethal contact
  - Trigger reset flow
- Deliverables:
  - Consistent fail state
- Acceptance Criteria:
  - Hazard contact resets the room every time
  - Reset restores gameplay objects correctly
- Dependencies: `SC-10`
- Priority: `P0`
- Effort: `S`

### SC-14 - Create level goal and completion trigger

- Goal: Define how a room is won and how the next state is reached.
- Tasks:
  - Add goal zone
  - Trigger level complete state
  - Connect to scene flow
- Deliverables:
  - Room completion logic
- Acceptance Criteria:
  - Reaching the goal clearly completes the room
  - Completion state is visible and stable
- Dependencies: `SC-12`
- Priority: `P0`
- Effort: `S`

## Phase 4 - Game Flow and UI

### SC-15 - Build main menu and scene start flow

- Goal: Let players launch the prototype from a polished entry point.
- Tasks:
  - Create title screen
  - Add play and quit actions
  - Route into the first level
- Deliverables:
  - Main menu scene
- Acceptance Criteria:
  - Game starts from menu without editor setup
  - Buttons function in standalone build
- Dependencies: `SC-02`
- Priority: `P1`
- Effort: `S`

### SC-16 - Add pause menu, restart option, and basic HUD prompts

- Goal: Improve usability during gameplay.
- Tasks:
  - Add pause toggle
  - Add restart button
  - Display controls and mechanic prompts
- Deliverables:
  - Functional in-game UI
- Acceptance Criteria:
  - Pause and restart work from gameplay
  - HUD communicates the mechanic clearly
- Dependencies: `SC-09`
- Priority: `P1`
- Effort: `M`

### SC-17 - Create level complete and final completion screens

- Goal: Provide closure and a clear end state for the prototype.
- Tasks:
  - Add per-level completion flow or next-level prompt
  - Add final completion screen
  - Return to menu option
- Deliverables:
  - End-to-end game loop
- Acceptance Criteria:
  - Prototype can be completed from start to finish
  - Final screen feels intentional and presentable
- Dependencies: `SC-14`, `SC-15`
- Priority: `P1`
- Effort: `S`

## Phase 5 - Level Design

### SC-18 - Build tutorial level teaching movement and first replay

- Goal: Introduce the main mechanic without friction.
- Tasks:
  - Block out tutorial room
  - Add onboarding text or visual cues
  - Validate first successful clone interaction
- Deliverables:
  - Level 1 playable room
- Acceptance Criteria:
  - A new player can understand the mechanic quickly
  - Room can be completed without hidden knowledge
- Dependencies: `SC-09`, `SC-14`
- Priority: `P0`
- Effort: `M`

### SC-19 - Build button-and-door puzzle level

- Goal: Demonstrate cooperative timing between player and clone.
- Tasks:
  - Block out level
  - Use button and door interaction
  - Tune for quick iteration
- Deliverables:
  - Level 2 playable room
- Acceptance Criteria:
  - Puzzle requires the clone in a readable way
  - Completion feels satisfying, not confusing
- Dependencies: `SC-12`, `SC-18`
- Priority: `P0`
- Effort: `M`

### SC-20 - Build hazard timing level

- Goal: Increase difficulty using fail states and timing pressure.
- Tasks:
  - Block out level
  - Integrate hazards with clone timing
  - Tune reset points
- Deliverables:
  - Level 3 playable room
- Acceptance Criteria:
  - The room escalates challenge without new systems
  - Death and retry loop feels fast
- Dependencies: `SC-13`, `SC-19`
- Priority: `P0`
- Effort: `M`

### SC-21 - Build final mixed challenge level and progression order

- Goal: Deliver a short final test that uses all learned ideas.
- Tasks:
  - Block out final room
  - Sequence all levels in final order
  - Tune difficulty curve
- Deliverables:
  - Final level and complete campaign path
- Acceptance Criteria:
  - The game has a coherent beginning, middle, and end
  - Final level feels like a capstone, not a new tutorial
- Dependencies: `SC-20`, `SC-17`
- Priority: `P1`
- Effort: `M`

## Phase 6 - Audio and Polish

### SC-22 - Add core gameplay audio feedback

- Goal: Improve interaction clarity with a small, high-value sound set.
- Tasks:
  - Add jump, record, replay, button, door, death, and win sounds
  - Balance volume levels
- Deliverables:
  - Basic soundscape
- Acceptance Criteria:
  - Major interactions have clear audio feedback
  - Sounds are readable and not distracting
- Dependencies: `SC-16`, `SC-17`
- Priority: `P1`
- Effort: `M`

### SC-23 - Apply visual polish to clone, doors, hazards, and UI

- Goal: Raise presentation quality without expanding systems.
- Tasks:
  - Improve color coding
  - Add small effects or animations
  - Tighten UI layout and readability
- Deliverables:
  - Cohesive visual pass
- Acceptance Criteria:
  - Game looks consistent across all levels
  - Core interactables are readable at a glance
- Dependencies: `SC-21`
- Priority: `P1`
- Effort: `M`

### SC-24 - Tune feel, readability, and difficulty across all levels

- Goal: Make the prototype feel deliberate and complete.
- Tasks:
  - Tune movement values
  - Tune recording duration if needed
  - Adjust puzzle clarity and reset speed
- Deliverables:
  - Final gameplay tuning pass
- Acceptance Criteria:
  - Difficulty curve feels fair
  - Core mechanic remains the focus in every room
- Dependencies: `SC-21`, `SC-23`
- Priority: `P0`
- Effort: `M`

## Phase 7 - QA, Packaging, and Publish

### SC-25 - Run structured QA pass and fix release-blocking bugs

- Goal: Verify the game is stable enough to present professionally.
- Tasks:
  - Run through the QA checklist
  - Log and fix high-priority bugs
  - Verify reset and replay stability
- Deliverables:
  - Release candidate build state
- Acceptance Criteria:
  - No major blocker remains in the core loop
  - Full playthrough succeeds consistently
- Dependencies: `SC-24`
- Priority: `P0`
- Effort: `M`

### SC-26 - Create Windows build and verify standalone playthrough

- Goal: Produce the portfolio-ready playable build.
- Tasks:
  - Configure build settings
  - Export Windows build
  - Test on standalone executable
- Deliverables:
  - Windows build package
- Acceptance Criteria:
  - Game launches and completes outside the editor
  - Scene order and inputs work in build
- Dependencies: `SC-25`
- Priority: `P0`
- Effort: `S`

### SC-27 - Capture screenshots, gameplay GIF/video, and final README updates

- Goal: Make the repository and portfolio page attractive to reviewers.
- Tasks:
  - Capture 3 strong screenshots
  - Record short gameplay clip or GIF
  - Update README status, media, and build details
- Deliverables:
  - Portfolio-ready repository presentation
- Acceptance Criteria:
  - README shows the mechanic clearly
  - Media assets reflect the final build
- Dependencies: `SC-26`
- Priority: `P1`
- Effort: `S`

### SC-28 - Publish release build and project page

- Goal: Finish the project with a clean public presentation.
- Tasks:
  - Create tagged release
  - Upload build to itch.io or portfolio host
  - Add release notes and project summary
- Deliverables:
  - Public release package
- Acceptance Criteria:
  - Build is downloadable
  - Repository and portfolio page feel complete and professional
- Dependencies: `SC-27`
- Priority: `P1`
- Effort: `S`

## Milestone Mapping

- Milestone 1: `SC-01` to `SC-06`
- Milestone 2: `SC-07` to `SC-10`
- Milestone 3: `SC-11` to `SC-17`
- Milestone 4: `SC-18` to `SC-21`
- Milestone 5: `SC-22` to `SC-28`
