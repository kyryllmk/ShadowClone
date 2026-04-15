# Shadow Clone - Coding Standards

## Purpose

These rules cover the first implementation slice for SC-03 and are meant to keep the project readable while it grows from prototype to polished portfolio piece.

## Naming

- Use the root namespace `ShadowClone`.
- Use sub-namespaces by responsibility: `ShadowClone.Core`, `ShadowClone.Gameplay`, `ShadowClone.Clone`, `ShadowClone.UI`.
- Use `PascalCase` for class names, public methods, and serialized property labels.
- Keep scene names explicit and ordered: `MainMenu`, `Level_01_Tutorial`, `Level_02_ButtonDoor`, `Level_03_HazardTiming`, `Level_04_Final`.

## Script Ownership

- `Core/`: scene flow, camera framing, spawn points, room reset coordination.
- `Gameplay/`: player movement, ground checks, and direct player-facing visuals.
- `Clone/`: recording storage, recording input flow, clone spawn/replay, clone cleanup.
- `Level/`: puzzle objects such as buttons, doors, hazards, and goals.
- `UI/`: simple HUD and player-readable mechanic feedback.

## Practical Rules

- Prefer one clear responsibility per MonoBehaviour.
- Keep managers as coordinators, not feature dumping grounds.
- Use direct references in the scene before introducing event buses or generic frameworks.
- Only add a new manager when two or more scene objects truly need a shared coordinator.
- Use prefabs for reusable gameplay actors such as the replay clone.
- Keep replay data simple: sampled positions first, richer state only if the mechanic needs it.

## Current Ownership Boundaries

- `PlayerController` owns movement and jump.
- `GroundCheck` owns grounded detection only.
- `RecordingController` owns clip capture and timing.
- `CloneReplayController` owns clone lifecycle.
- `CloneActor` owns movement along recorded frames.
- `CloneMechanicController` owns player inputs for record and replay.
- `RoomResetManager` owns room restart coordination and spawn restoration.
- `PuzzleButton` owns trigger overlap detection and pressed-state reporting.
- `MechanicHudController` owns simple text feedback to the player.

## Guardrails

- Avoid all-in-one `GameManager` scripts.
- Avoid adding ScriptableObjects until repeated data or authoring pain appears.
- Avoid adding animation, audio, or VFX coupling inside movement or recording scripts.
- Prefer reliable and debuggable behavior over feature-rich behavior.
