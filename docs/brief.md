# Current Implementation

A snapshot of working systems. Game design belongs in gdd.md; technical rules belong in architecture.md.

## Available Systems

- **Campaign clock:** starts on Day 1 at 00:00 and advances in ten-minute ticks. At ×1, one real second produces one tick; ×2 doubles the rate.
- **Active pause:** freezes campaign time while the interface remains responsive. Fractional progress and selected speed are preserved; paused time is never accumulated.
- **Controls:** Pause/Resume and ×1/×2 buttons. Space toggles pause; 1 and 2 select speed. Changing speed does not resume a paused campaign.
- **Valley map:** a fixed 2D region with Fallowfield, Stoneford, a road, bridge, river, forests, and hills. Terrain uses code-drawn prototype artwork and fits the available window space.
- **Player company:** right-click to choose or replace a destination. Movement follows a straight line, stops at the destination, respects map bounds, and uses campaign time. Orders can be changed during pause without moving the company.
- **Presentation:** a banner marks the company; a dashed line and crosshair show its destination. Movement interpolates between simulation ticks and freezes during pause. The HUD shows time, speed, and travel state.
- **Simulation boundary:** CampaignSession advances registered tick systems, including PartyTravel, in a fixed order. WorldMap supplies scenario data; Parties owns movement. Godot handles drawing and input independently of simulation state.

## Verification

Unit tests cover time, movement, arrival, redirection, map bounds, pause, speed, and frame-rate independence. Godot checks cover mouse orders, interpolation, pause controls, arrival, and input after window resizing.

## Current Limits

Terrain and settlements are visual landmarks only. Travel ignores terrain and follows straight lines; there is no pathfinding, camera navigation, settlement interaction, economy, combat, diplomacy, or save/load yet.
