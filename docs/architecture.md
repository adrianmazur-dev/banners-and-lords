# Technical Architecture

These rules govern implementation.

## Boundaries

- Use Godot .NET and C#. Engine and SDK versions are defined by the project files and global.json.
- Build a modular monolith organized by gameplay feature. Add layers and abstractions only for concrete responsibilities.
- Keep simulation in the engine-independent C# library under src/Simulation and Godot adapters under src/Godot. The Godot project references the library and excludes simulation and test sources from its own compilation. Hide these directories from Godot imports with .gdignore.
- Within simulation features, use Contracts for public APIs, Domain for state and rules, and Application for coordination. Add Infrastructure for technical adapters when needed.
- Features access other features only through Contracts. Keep dependencies acyclic; composition code may reference concrete implementations to wire them together.
- Construct dependencies explicitly at startup. Keep bootstrap code limited to composition and lifecycle management.

## State and Execution

- Simulation owns game state independently of nodes and scenes. Each piece of state has one owner; other modules refer to it by stable identifiers.
- Dynasties owns noble characters, family membership, and dynasty heads. Parties references commanders by character ID and validates them through Dynasties Contracts; derive the commander's dynasty from the roster instead of duplicating it on the party.
- Inventory owns item definitions, party stacks, individual instances, and equipment. Storage type is explicit; weapons and armor are individual, with a compatible slot. Keep stable instance IDs separate from definition IDs and integer stack quantities.
- Each inventory belongs to a party and currently equips its commander, referenced by character ID. An instance occupies exactly one location: carried or equipped. Slot replacement atomically returns the previous instance to carried items; equipped instances cannot be removed directly.
- Expose immutable inventory snapshots with stable ordering. Commands return explicit results; reject unknown definitions, invalid quantities, overdraw, overflow, duplicate instance IDs, and invalid equipment operations without changing the snapshot. Godot reads snapshots and issues commands; it never owns item state.
- Godot handles input, presentation, audio, and editor integration. Removing a view or failing an animation must not change simulation results.
- Major panels fill the viewport and contain keyboard focus and pointer input. Keep inventory columns stable across personal and future trade views; the counterparty space remains empty in personal inventory. Rows and tooltips derive from authoritative snapshots; layout must not introduce simulated stock, prices, or transactions.
- Advance the campaign through fixed simulation ticks with explicit system ordering. Bound work per frame without dropping pending ticks. Schedule daily economy separately from movement; frame rate must not change the rules.
- Register tick systems through composition and invoke them once per completed tick in registration order. Expose read-only fractional progress for view interpolation; campaign pause must freeze interpolation too.
- Store movement in engine-free world coordinates. Validate destinations in simulation; convert pointer coordinates only in Godot. Visual terrain does not implicitly define passability.
- Settlements owns definitions with stable IDs and world positions, entry radii, and the active visit. WorldMap and Godot reuse those definitions. Validate entry against the party's authoritative position in simulation; entering cancels travel.
- Keep campaign time, speed, pause, and fractional progress in the session. Pausing must preserve progress and speed without accumulating paused time. Initially, campaign time stops during tactical battles while UI and battle presentation remain active.
- Mandatory pauses use session-owned holds. Manual pause changes cannot override a hold or alter the prior manual state; only releasing all holds restores it. Active visits own their hold independently of views, and release it once on leaving.
- Change state through validated commands; expose read-only data through queries. Player and AI actions use the same rules. Publish events only after changes are committed.
- Coordinate operations spanning modules so they either complete together or leave state unchanged. Apply each battle result to the campaign exactly once.

## Data and Persistence

- Keep content definitions separate from mutable instances. Convert Godot Resources to plain C# definitions at the engine boundary.
- Save versioned data snapshots, not scene trees or engine objects. Include stable IDs, simulation time, pending work, and serializable random generator state.
- Use integer units for money and goods with explicit rounding. Use controlled randomness and stable ordering for repeatable simulation.
- Run simulation on the main thread initially. Introduce background computation only after profiling; workers must not mutate the live session or scene tree.
