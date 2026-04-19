# Shadow Clone - Smoke Test Checklist

Use this checklist after wiring the scripts in the Unity editor.

## Foundation

- `MainMenu` opens and Play loads `Level_01_Tutorial`.
- `Level_01_Tutorial` can also be run directly from the editor.

## Main Menu

- `MainMenu` shows a clear title and entry buttons.
- `PlayButton` loads `Level_01_Tutorial`.
- `QuitButton` exits Play Mode in the editor.
- Build Settings scene order starts with `MainMenu` then `Level_01_Tutorial`.

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

## Hazard Reset

- A `Hazard` object can be placed in the tutorial room.
- Touching the hazard resets the room every time.
- Hazard reset returns the player to the spawn point.
- Hazard reset clears the active clone and recorded mechanic state.
- Hazard reset restores linked buttons and doors to their default closed / idle state.

## Goal Zone

- A `GoalZone` object can be placed in the tutorial room.
- Reaching the goal with the player displays a stable completion message.
- Player movement locks after completion.
- Replay clones do not accidentally complete the room on their own.

## Pause And Restart

- `Esc` opens a pause menu in gameplay scenes.
- Pause blocks player movement and record / replay input.
- `Resume` returns to live gameplay cleanly.
- `Restart Room` reloads the current level.
- `Return To Menu` loads `MainMenu`.

## Completion Flow

- Reaching the goal opens a clear completion overlay.
- The current prototype can be replayed or exited back to the menu after completion.
- Final completion state feels intentional rather than like a debug message.
- When multiple campaign levels exist, completion can route to the next scene in order.

## Readability

- HUD text changes for ready, recording, blocked, replay, and reset states.
- Camera framing remains readable while moving and jumping.
- Player facing flips correctly when direction changes.
