# Current Implementation

A snapshot of working systems. Game design belongs in gdd.md; technical rules belong in architecture.md.

## Available Systems

- **Campaign clock:** starts on Day 1 at 00:00 and advances in one-minute ticks. At ×1, ten ticks per real second preserve ten game minutes per second; ×2 doubles the rate.
- **Active pause:** freezes campaign time while the interface remains responsive. Fractional progress and selected speed are preserved; paused time is never accumulated.
- **Controls:** Pause/Resume and ×1/×2 buttons. Space toggles pause; 1 and 2 select speed. Changing speed does not resume a paused campaign.
- **Valley map:** a fixed 2D region with Fallowfield, Stoneford, a road, bridge, river, forests, and hills. Terrain uses code-drawn prototype artwork and fits the available window space.
- **Villages:** left-click Fallowfield or Stoneford to inspect its description. Travel here sets its position as the destination; Enter becomes available within 32 world units. The village panel shares the side-panel area with Party and Dynasty.
- **Settlement visits:** Enter opens a modal village view, stops the party, and holds campaign time. The view blocks map and panel input; time controls are disabled. Leave settlement releases the hold and restores the prior pause state, speed, and fractional progress. Arrival alone does not enter.
- **Player party:** right-click to choose or replace a destination. Movement follows a straight line, stops at the destination, respects map bounds, and uses campaign time. Orders can be changed during pause without moving the party.
- **Dynasties and characters:** House Ashford has three named members. Elinor heads the dynasty; Aldric commands the player party. Characters, dynasties, and parties have stable IDs; invalid family and commander references are rejected.
- **Cargo:** a shared catalog defines Grain, Bread, Fish, Timber, and Tools. The player party starts with 20 Grain, 8 Fish, 5 Timber, and 2 Tools. Simulation supports adding, checking, and removing goods; invalid operations leave cargo unchanged. Each inventory belongs to one party.
- **Party panel:** shows the commander, travel state, current order, and a Cargo list with quantities and category tooltips. The list updates when inventory changes and shows an empty state when needed. Clicking the commander opens Dynasty.
- **Dynasty panel:** shows the house, its head, and members, identifying the player's character. Party and Dynasty have separate buttons; only one panel opens at a time. Panel navigation preserves campaign pause and speed.
- **Presentation:** a banner marks the party; a dashed line and crosshair show its destination. Movement interpolates between simulation ticks and freezes during pause. The HUD shows the party's commander, time, speed, and travel state.
- **Simulation boundary:** CampaignSession advances registered tick systems, including PartyTravel, in a fixed order. WorldMap supplies scenario data; Parties owns movement. Godot handles drawing and input independently of simulation state.

## Verification

Unit tests cover time, movement, dynasties, settlement visits, and inventory validation, ownership, quantities, and immutable snapshots. Godot checks cover travel, visits, panel navigation, Cargo updates and empty state, modal input, pause controls, and window resizing.

## Current Limits

Terrain is visual only and travel follows straight lines. Village visits show descriptions; trade, recruitment, pathfinding, camera navigation, economy, combat, diplomacy, and save/load are not implemented. Cargo has no consumption, spoilage, carry limits, or character equipment yet. Dynasty membership and leadership are fixed scenario data; genealogy, marriage, aging, succession, and commander changes remain future work.
