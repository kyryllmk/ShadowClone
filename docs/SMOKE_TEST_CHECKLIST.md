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

## Readability

- HUD text changes for ready, recording, blocked, replay, and reset states.
- Camera framing remains readable while moving and jumping.
- Player facing flips correctly when direction changes.
