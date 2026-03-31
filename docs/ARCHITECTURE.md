# Shadow Clone - Architecture Notes

## Architecture Goal

Keep the implementation understandable for a learning Unity developer while still looking clean and intentional in a portfolio repo.

The architecture should support one reliable gameplay loop, not a reusable engine.

## Scene Structure

Recommended scenes:

- `Bootstrap` or `MainMenu`
- `Level_01_Tutorial`
- `Level_02_ButtonDoor`
- `Level_03_HazardTiming`
- `Level_04_Final`
- `UI_Test` optional during development only

Keep scene count low. Avoid scene variants unless they are clearly necessary.

## System Overview

Core runtime systems:

- Player controller
- Recorder
- Clone replay actor
- Room reset manager
- Puzzle interaction objects
- Simple scene / level flow manager
- UI manager
- Audio manager

## Major Scripts / Classes

Recommended class list:

### PlayerController

Responsibilities:

- Read movement input
- Handle horizontal movement and jump
- Update facing and animation hooks if added
- Expose current world position for recording

### GroundCheck

Responsibilities:

- Determine grounded state
- Keep jump logic readable

### RecordingController

Responsibilities:

- Start and stop recording
- Sample player position at a fixed interval
- Store recorded frames for replay
- Report recording state to UI

### RecordedFrame

Responsibilities:

- Hold sampled time and position
- Optionally store facing direction if needed

### CloneReplayController

Responsibilities:

- Receive recorded frames
- Move the clone through the timeline
- End replay cleanly
- Notify systems when replay finishes

### PuzzleButton

Responsibilities:

- Detect overlap from player and or clone
- Send active state to linked objects

### DoorController

Responsibilities:

- Open or close based on button state
- Update collider and visuals

### Hazard

Responsibilities:

- Detect lethal collision
- Trigger room reset

### GoalZone

Responsibilities:

- Detect successful level completion
- Notify game flow manager

### RoomResetManager

Responsibilities:

- Reset player, clone, doors, buttons, and room state
- Handle manual restart and death reset

### GameFlowManager

Responsibilities:

- Handle menu transitions
- Load next level
- Return to menu
- Show win / completion state

### UIManager

Responsibilities:

- Display prompts and state text
- Show recording / replay feedback
- Handle pause and completion panels

## Object Responsibilities

Keep object responsibilities narrow:

- Player object: movement and collision
- Clone object: playback only
- Room objects: one interaction each
- Managers: coordinate flow, not per-object behavior

Avoid large all-in-one scripts.

## Recording / Replay Data Flow

Recommended MVP flow:

1. `PlayerController` updates movement each frame.
2. `RecordingController` samples the player's transform at a fixed interval.
3. Recorded positions are stored in a list of `RecordedFrame`.
4. When replay starts, the previous clone is cleared.
5. `CloneReplayController` receives the recorded list.
6. The clone moves through the recorded frames in time order.
7. When playback ends, the clone stops or despawns based on room design.

Implementation notes:

- Prefer fixed interval sampling over storing every frame.
- Start with position-only replay.
- Add rotation or facing only if the presentation truly needs it.
- Keep one clone maximum.

## Interaction Flow: Buttons, Doors, Hazards

### Buttons to Doors

1. Player or clone overlaps the button trigger.
2. `PuzzleButton` sets active state.
3. Linked `DoorController` receives state change.
4. Door opens or closes visually and updates collider.

### Hazards

1. Player touches hazard.
2. `Hazard` signals `RoomResetManager`.
3. Room reset manager restores initial state.
4. Player respawns and clone is cleared.

### Goal

1. Player enters goal zone.
2. `GoalZone` calls `GameFlowManager`.
3. Completion UI or next level loads.

## Keep-It-Simple Implementation Notes

- Use direct script references first before adding event systems.
- Use ScriptableObjects only if a real repeated data need appears.
- Keep puzzle logic local to the level unless reuse becomes obvious.
- Do not build save/load architecture for this MVP.
- Do not create abstract base frameworks just to look advanced.
- Favor readable MonoBehaviours over premature patterns.

## Folder Structure Suggestion

Recommended `Assets/` layout:

- `Assets/Art/`
- `Assets/Audio/`
- `Assets/Materials/`
- `Assets/Prefabs/`
- `Assets/Scenes/`
- `Assets/Scripts/Core/`
- `Assets/Scripts/Gameplay/`
- `Assets/Scripts/UI/`
- `Assets/Scripts/Level/`
- `Assets/Settings/`
- `Assets/UI/`

This is enough structure for a small project without making navigation harder.
