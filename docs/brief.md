# Current Implementation

A snapshot of working systems. Game design belongs in gdd.md; technical rules belong in architecture.md.

## Available Systems

- **Campaign clock:** starts on Day 1 at 00:00 and advances in one-minute ticks. At ×1, ten ticks per real second preserve ten game minutes per second; ×2 doubles the rate.
- **Active pause:** freezes campaign time while the interface remains responsive. Fractional progress and selected speed are preserved; paused time is never accumulated.
- **Controls:** Pause/Resume and ×1/×2 buttons. Space toggles pause; 1 and 2 select speed. Changing speed does not resume a paused campaign.
- **Valley map:** a fixed 2D region with Fallowfield, Stoneford, a road, bridge, river, forests, and hills. Terrain uses code-drawn prototype artwork and fits the available window space.
- **Player party:** right-click to choose or replace a destination. Movement follows a straight line, stops at the destination, respects map bounds, and uses campaign time. Orders can be changed during pause without moving the party.
- **Dynasties and characters:** House Ashford has three named members. Elinor heads the dynasty; Aldric commands the player party. Characters, dynasties, and parties have stable IDs; invalid family and commander references are rejected.
- **Party panel:** shows the commander, travel state, and current movement order. Clicking the commander opens Dynasty.
- **Dynasty panel:** shows the house, its head, and members, identifying the player's character. Party and Dynasty have separate buttons; only one panel opens at a time. Panel navigation preserves campaign pause and speed.
- **Presentation:** a banner marks the party; a dashed line and crosshair show its destination. Movement interpolates between simulation ticks and freezes during pause. The HUD shows the party's commander, time, speed, and travel state.
- **Simulation boundary:** CampaignSession advances registered tick systems, including PartyTravel, in a fixed order. WorldMap supplies scenario data; Parties owns movement. Godot handles drawing and input independently of simulation state.

## Verification

Unit tests cover time, movement, pause, speed, frame-rate independence, dynasty membership, head and commander references, and read-only roster queries. Godot checks cover travel, arrival, Party/Dynasty navigation, panel input, pause controls, and window resizing.

## Current Limits

Terrain and settlements are visual landmarks only. Travel ignores terrain and follows straight lines; there is no pathfinding, camera navigation, settlement interaction, economy, combat, diplomacy, or save/load yet. Dynasty membership and leadership are fixed scenario data; genealogy, marriage, aging, succession, and commander changes are not implemented.
