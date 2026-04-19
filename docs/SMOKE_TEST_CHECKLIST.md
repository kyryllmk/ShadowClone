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

## Campaign Levels

- `Level_01_Tutorial` introduces the first safe button-door solve.
- `Level_02_ButtonDoor` requires a longer button hold and run to the goal.
- `Level_03_HazardTiming` adds a hazard strip after the door interaction.
- `Level_04_Final` combines button-door timing and the widest hazard strip in the campaign.
- `Next Level` can carry the player from level `1` through level `4`.
- `Level_04_Final` ends on the prototype completion screen rather than loading another room.

## Presentation

- Jump and landing both produce short readable audio feedback.
- Start recording, stop recording, replay, button press, door open/close, hazard hit, and level completion all produce distinct audio cues.
- Main menu `Play` and `Quit` produce a confirmation sound.
- Pause toggle produces a dedicated sound.
- Restarting a room produces a dedicated restart sound.
- Entering a gameplay level produces a level-start sound.
- `MainMenu`, `Level_01_Tutorial`, `Level_02_ButtonDoor`, `Level_03_HazardTiming`, and `Level_04_Final` each have their own matching ambient loop or background music layer.
- Player feedback transitions smoothly between idle, airborne, and recording states.
- Buttons visibly compress and pulse while held.
- Doors visually shrink/fade open instead of only toggling collision.
- Hazards pulse clearly enough to read as dangerous at a glance.
- Goals pulse while idle and brighten on completion.

## Tuning

- Tutorial text and mechanic status no longer overlap.
- HUD remains readable during record / replay / reset states.
- Level escalation still feels fair after the polish pass.

## Readability

- HUD text changes for ready, recording, blocked, replay, and reset states.
- Camera framing remains readable while moving and jumping.
- Player facing flips correctly when direction changes.
