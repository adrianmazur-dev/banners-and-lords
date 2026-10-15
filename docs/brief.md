# Current Implementation

A snapshot of working systems. Game design belongs in gdd.md; technical rules belong in architecture.md.

## Available Systems

- **Campaign clock:** starts on Day 1 at 00:00 and advances in one-minute ticks. At ×1, ten ticks per real second preserve ten game minutes per second; ×2 doubles the rate.
- **Active pause:** freezes campaign time while the interface remains responsive. Fractional progress and selected speed are preserved; paused time is never accumulated.
- **Controls:** Pause/Resume and ×1/×2 buttons. Space toggles pause; 1 and 2 select speed. Changing speed does not resume a paused campaign.
- **Valley map:** a fixed 2D region with Fallowfield, Stoneford, a road, bridge, river, forests, and hills. Terrain uses code-drawn prototype artwork and fits the available window space.
- **Villages:** left-click Fallowfield or Stoneford to inspect its full-screen overview. Travel here sets the destination and returns to the map; Enter becomes available within 32 world units.
- **Settlement visits:** Enter opens a modal village view, stops the party, and holds campaign time. The view blocks map and panel input; time controls are disabled. Leave settlement releases the hold and restores the prior pause state, speed, and fractional progress. Arrival alone does not enter.
- **Player party:** right-click to choose or replace a destination. Movement follows a straight line, stops at the destination, respects map bounds, and uses campaign time. Orders can be changed during pause without moving the party.
- **Dynasties and characters:** House Ashford has three named members. Elinor heads the dynasty; Aldric commands the player party. Characters, dynasties, and parties have stable IDs; invalid family and commander references are rejected.
- **Inventory:** party-owned stacks and uniquely identified equipment instances share an item catalog. Starting supplies are 60 Bread, 20 Grain, 8 Fish, 20 Timber, and 2 Tools; equipment includes two separate swords, a knife, shield, helmet, coat, gloves, and boots. Validated commands add/remove stacks and individual items; failed operations preserve state.
- **Equipment:** the commander has head, body, hands, feet, main-hand, and off-hand slots. Equip moves an instance from the party inventory into its compatible slot; replacing equipment returns the previous instance. Unequip returns it to the inventory. Original SVG layers reflect worn equipment on the character.
- **Inventory panel:** Inventory button or I opens a full-screen view with a blank left column, character/equipment in the center, and scrollable item rows on the right. Rows show an icon, name, and actual quantity; tooltips describe the item and compatible slot. Category icons stay above the character. Selection and Equip/Unequip remain functional; empty categories are supported. The panel holds campaign time and blocks map orders; Esc, I, or Close restores the prior pause state.
- **Party panel:** a full-screen view of the commander, travel state, and current order. Clicking the commander opens Dynasty; belongings have their dedicated Inventory panel.
- **Dynasty panel:** a full-screen view of the house, its head, and members, identifying the player's character. Party, Dynasty, and village overviews keep focus within the active view and close with Esc or Close. Opening Inventory replaces the overview. Navigation preserves campaign pause and speed; visits and Inventory retain their mandatory pauses.
- **Presentation:** a banner marks the party; a dashed line and crosshair show its destination. Movement interpolates between simulation ticks and freezes during pause. The HUD shows the party's commander, time, speed, and travel state.
- **Simulation boundary:** CampaignSession advances registered tick systems, including PartyTravel, in a fixed order. WorldMap supplies scenario data; Parties owns movement. Godot handles drawing and input independently of simulation state.

## Verification

Unit tests cover time, movement, dynasties, settlement visits, stacks, unique item ownership, equipment swaps, and immutable snapshots. Godot checks cover full-screen geometry, stable columns, item rows/tooltips, equipment commands, focus, navigation, resizing, and campaign travel/visits.

## Current Limits

Terrain is visual only and travel follows straight lines. Village visits show descriptions; trade, recruitment, pathfinding, camera navigation, economy, combat, diplomacy, and save/load are not implemented. Inventory equips only the current commander; additional party members, combat stats, consumption, and carry limits remain future work. Dynasty membership and leadership are fixed scenario data; genealogy, marriage, aging, succession, and commander changes remain future work.
