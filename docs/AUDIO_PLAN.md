# Shadow Clone - Audio Plan

## Audio Goal

Use a small set of clear sound effects to improve feedback and make the prototype feel more finished without creating extra production complexity.

## Audio Priorities

Highest value sounds:

- Jump
- Landing
- Start recording
- Stop recording
- Clone spawn
- Button press
- Door open / close
- Hazard death
- Level complete
- Menu confirm

## Music

Optional for MVP.

If added:

- Use one subtle ambient sci-fi loop for gameplay
- Use one lightweight menu loop if available

Music is lower priority than responsive sound effects.

## Mixing Guidelines

- Keep effects short and readable
- Avoid loud or harsh sounds
- Make hazards and success states distinct
- Lower music volume under UI and gameplay feedback

## Implementation Notes

- Centralize playback through a simple audio manager if helpful
- Use inspector-assigned audio clips
- Avoid complex dynamic music systems
- Ship with a tiny, polished set instead of many weak sounds

## Asset Sourcing

- Prefer self-made placeholder sounds if possible
- If using third-party audio, track source and license in README credits
