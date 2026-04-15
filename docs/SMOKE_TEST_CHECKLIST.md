# Shadow Clone - Smoke Test Checklist

Use this checklist after wiring the scripts in the Unity editor.

## Foundation

- `MainMenu` opens and Play loads `Level_01_Tutorial`.
- `Level_01_Tutorial` can also be run directly from the editor.

## Movement

- Player moves left and right on keyboard input.
- Player jumps only when grounded.
- Player respawns at the spawn point after manual reset.

## Replay Loop

- Recording starts with `R`.
- Recording stops with `R` or when the max duration is reached.
- Replay starts with `E` only when a valid recording exists.
- Only one replay clone exists at a time.
- Reset removes the active clone and clears the last recording.

## Puzzle Button

- A `PuzzleButton` object can be placed in the tutorial room.
- Walking the player onto the button changes it to its active state.
- Stepping off the button returns it to its inactive state.
- A replay clone can also activate the same button.

## Linked Door

- A `DoorController` object can be linked to a `PuzzleButton`.
- The door is visually closed and physically blocking when the button is idle.
- The door becomes visually open and non-blocking while the button is pressed.
- The replay clone can hold the button while the player passes through the door.

## Readability

- HUD text changes for ready, recording, blocked, replay, and reset states.
- Camera framing remains readable while moving and jumping.
- Player facing flips correctly when direction changes.
