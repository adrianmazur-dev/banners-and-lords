# Current Implementation

A snapshot of working systems. Game design belongs in gdd.md; technical rules belong in architecture.md.

## Available Systems

- **Campaign clock:** starts on Day 1 at 00:00 and advances in ten-minute ticks. At ×1, one real second produces one tick; ×2 doubles the rate.
- **Active pause:** freezes campaign time while the interface remains responsive. Fractional progress and selected speed are preserved; paused time is never accumulated.
- **Controls:** Pause/Resume and ×1/×2 buttons. Space toggles pause; 1 and 2 select speed. Changing speed does not resume a paused campaign.
- **Campaign panel:** displays day, time, pause state, and selected speed in English.
- **Simulation boundary:** CampaignSession and CampaignClock live in the engine-independent library under src/Simulation. Bootstrap retains the session; CampaignRunner advances it separately from CampaignController, which handles presentation and input.

## Verification

Unit tests cover fixed ticks, frame-rate independence, day rollover, pause, speed changes, and retained backlog. The scene has also been checked in Godot with mouse and keyboard input.

## Current Limits

There is no world map, army movement, economy, combat, diplomacy, or save/load yet.
