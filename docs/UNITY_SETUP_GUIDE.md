# Shadow Clone - Unity Setup Guide

This repo now includes the first gameplay script pass for SC-02 through SC-10, but the scene assets still need to be authored in the Unity editor so Unity can serialize valid `.unity`, `.prefab`, and `.meta` files.

## What Was Prepared In Repo

- `Assets/` folder structure matching the architecture notes
- scene-name constants and a simple menu controller
- core movement, ground check, spawn, camera, reset, recording, replay, and HUD scripts
- a starter `Packages/manifest.json`
- a starter `ProjectSettings/ProjectVersion.txt`

## Editor Setup For SC-02

1. Open the repo in Unity `2022.3.62f1` or the closest available `2022.3 LTS` editor.
2. Let Unity generate `Library/`, `Temp/`, `.meta`, and solution files.
3. Create these scenes under `Assets/Scenes/`:
   - `MainMenu`
   - `Level_01_Tutorial`
4. Add the scenes to Build Settings in this order:
   - `MainMenu`
   - `Level_01_Tutorial`
5. Make `MainMenu` scene index `0`.

## Suggested Scene Wiring

### MainMenu

- Create a Canvas with title text and two buttons.
- Add `MainMenuController` to an empty `MainMenuBootstrap` object.
- Hook the Play button to `MainMenuController.Play()`.
- Hook the Quit button to `MainMenuController.Quit()`.

### Level_01_Tutorial

- Create a simple flat room with a ground collider.
- Add a `Player` object with:
  - `Rigidbody2D`
  - `BoxCollider2D` or `CapsuleCollider2D`
  - `PlayerController`
  - `PlayerFeedbackView`
- Add a child object under `Player` for ground probing and assign it to `GroundCheck`.
- Add a `SpawnPoint` object at the player start.
- Add an empty `RoomSystems` object with:
  - `RecordingController`
  - `CloneReplayController`
  - `CloneMechanicController`
  - `RoomResetManager`
- Add a `Main Camera` with `SimpleCameraFollow`.
- Add a Canvas text label and assign it to `MechanicHudController`.
- Create a clone prefab from a simple sprite object with `CloneActor`.

## Smoke Tests

### SC-11 Pressure Button

- Create a square button object in `Level_01_Tutorial`.
- Add a `BoxCollider2D` and enable `Is Trigger`.
- Add `PuzzleButton`.
- Assign the button's `SpriteRenderer` to `Target Renderer`.
- Walk the player onto the button.
- Confirm the button changes color while occupied and returns to its idle color after stepping off.
- Trigger replay onto the button path and confirm the clone can also activate it.

### SC-04 Movement And Jump

- Enter Play Mode in `Level_01_Tutorial`.
- Press `A/D` or arrow keys.
- Press `Space` while grounded.
- Confirm horizontal movement works and the jump only fires when grounded.

### SC-05 Ground Check And Reset

- Move the player away from spawn.
- Press `Tab` or `Backspace`.
- Confirm the player snaps back to the `SpawnPoint` and velocity is cleared.

### SC-06 Camera And Feedback Hooks

- Move across the room and jump.
- Confirm the camera stays centered on the player without obvious jitter.
- Confirm the player flips when changing direction.
- Confirm the player color changes while airborne and while recording.

### SC-07 Recording

- Press `R` to start recording.
- Move and jump for up to three seconds.
- Press `R` again to stop recording.
- Confirm the HUD reports a saved recording.

### SC-08 Replay Clone

- Record a path.
- Press `E`.
- Confirm one clone spawns, follows the sampled route, and disappears after replay.

### SC-09 Input Flow And Feedback

- Press `E` before recording.
- Confirm the HUD reports a blocked state.
- Start recording and press `E` before stopping.
- Confirm replay is blocked until recording stops.

### SC-10 Reset Integration

- Record a path, spawn a clone, then press `Tab`.
- Confirm the clone is cleared, the recording is reset, and the HUD reports a room reset.
- Repeat the sequence multiple times and confirm no duplicate clone remains.
