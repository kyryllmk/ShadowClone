# Shadow Clone - QA Checklist

## Core Gameplay

- Player can move left and right reliably
- Jump works consistently from grounded state
- Ground detection does not flicker on flat surfaces
- Respawn / reset returns the player to the intended start point

## Clone Recording

- Recording starts only when allowed
- Recording duration respects the intended limit
- Player position samples are captured consistently
- Starting a new recording replaces the old one cleanly

## Replay Sync

- Clone follows the expected recorded path
- Replay timing remains stable across multiple tests
- Clone clears correctly on reset
- Only one clone exists at a time

## Puzzle Objects

- Buttons activate when player stands on them
- Buttons activate when clone stands on them if intended
- Doors open and close correctly from linked buttons
- Hazards kill the player consistently
- Goal zone completes the room correctly

## Level Reset

- Manual restart works in every level
- Death reset restores room state
- Doors, buttons, and clone state reset correctly
- No leftover objects remain after repeated retries

## Menus and UI

- Main menu loads the game correctly
- Pause menu stops gameplay as intended
- Resume returns control cleanly
- Restart from pause works
- Completion screen appears correctly
- UI text remains readable at target resolution

## Audio

- Jump sound plays at the correct moment
- Recording and replay sounds do not overlap badly
- Door and button sounds match interaction states
- Hazard and victory sounds are clearly distinguishable
- Overall volume feels balanced

## Build Test

- Windows build launches successfully outside the Unity Editor
- Scene order is correct in the build
- Input works in the standalone build
- Finish-to-menu flow works in the build

## Portfolio Assets

- README reflects the current project state
- Screenshots are current and readable
- Gameplay GIF or video shows the clone mechanic clearly
- Credits and asset attributions are complete

## Test Pass Guidance

Before release, run:

- One fast smoke test after every major feature merge
- One full playthrough after each level is added
- One final full build verification pass before publishing
