# Shadow Clone - Game Design Document

## Overview

Shadow Clone is a 2D puzzle-platformer / arcade logic prototype for Windows PC. The player records a short movement sequence and replays it as a clone to solve minimalist sci-fi test chambers.

This is a mechanic-first portfolio game. The project should feel polished, readable, and complete, even at small scope.

## Target Experience

The intended player experience is:

- Learn one mechanic quickly
- Understand rooms at a glance
- Experiment with timing and path planning
- Reset instantly after failure
- Finish the prototype in one short sitting

The tone should feel like a training simulation: clean, controlled, futuristic, and focused.

## Gameplay Loop

1. Enter a room and identify the obstacle.
2. Decide where the clone needs to be.
3. Record a short player path.
4. Spawn the replay clone.
5. Move with or around the clone to solve the room.
6. Reach the exit.
7. Restart quickly if the attempt fails.

## Rules

- Only one active clone exists at a time.
- The clone only replays recorded movement.
- The clone does not adapt or make decisions.
- Recording length is limited.
- Player death or failure resets the room.
- Puzzle objects respond to player, clone, or both depending on design.

## Mechanics

### Player Movement

- Left and right movement
- Single jump
- Grounded collision
- Fast and readable handling over realism

### Recording

- Record sampled player positions over a fixed duration
- Keep the timeline short for clarity and reliability
- Provide visual feedback while recording

### Replay Clone

- Spawn one clone after a successful recording
- Replay recorded samples in order
- Use fixed timing to reduce drift
- Clone disappears when replay ends or on room reset

### Puzzle Interactions

- Pressure buttons
- Doors linked to buttons
- Hazards
- Goal / exit zone

## Controls

- `A / D` or `Left / Right`: Move
- `Space`: Jump
- `R`: Record
- `E`: Spawn replay clone
- `Esc`: Pause
- `Tab` or `Backspace`: Restart room

## Level Progression

Recommended structure:

1. Level 1: Learn movement and basic recording
2. Level 2: Use clone to hold a button and open a door
3. Level 3: Combine movement timing, hazard avoidance, and clone placement
4. Level 4: Final mixed challenge with the full ruleset

Each room should teach one idea before combining it with another.

## UI Flow

- Main Menu
- Level Start / first room
- In-game HUD
- Pause Menu
- Level Complete state
- Final Completion screen

The UI should remain light and functional.

## Win Conditions

- Reach the room exit
- Complete all planned levels

## Lose Conditions

- Touch a hazard
- Fall out of bounds if used
- Get stuck and choose manual restart

## Strict Scope Limits

This project must stay intentionally small.

Included in scope:

- One player controller
- One clone recording / replay system
- 3 to 4 short levels
- Basic UI flow
- Simple audio feedback
- Windows build and portfolio presentation

Out of scope:

- Multiple clones
- Combat
- Complex enemy AI
- Story campaign
- Inventory systems
- Procedural generation
- Advanced animation pipeline
- Save system beyond simple scene progress if needed

## Success Criteria

The project is successful if:

- The clone mechanic works reliably
- The full game can be finished in one short session
- The repo is easy to understand
- The build feels clean enough for portfolio review
