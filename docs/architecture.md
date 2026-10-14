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
- Godot handles input, presentation, audio, and editor integration. Removing a view or failing an animation must not change simulation results.
- Advance the campaign through fixed simulation ticks with explicit system ordering. Bound work per frame without dropping pending ticks. Schedule daily economy separately from movement; frame rate must not change the rules.
- Register tick systems through composition and invoke them once per completed tick in registration order. Expose read-only fractional progress for view interpolation; campaign pause must freeze interpolation too.
- Store movement in engine-free world coordinates. Validate destinations in simulation; convert pointer coordinates only in Godot. Visual terrain does not implicitly define passability.
- Keep campaign time, speed, pause, and fractional progress in the session. Pausing must preserve progress and speed without accumulating paused time. Initially, campaign time stops during tactical battles while UI and battle presentation remain active.
- Change state through validated commands; expose read-only data through queries. Player and AI actions use the same rules. Publish events only after changes are committed.
- Coordinate operations spanning modules so they either complete together or leave state unchanged. Apply each battle result to the campaign exactly once.

## Data and Persistence

- Keep content definitions separate from mutable instances. Convert Godot Resources to plain C# definitions at the engine boundary.
- Save versioned data snapshots, not scene trees or engine objects. Include stable IDs, simulation time, pending work, and serializable random generator state.
- Use integer units for money and goods with explicit rounding. Use controlled randomness and stable ordering for repeatable simulation.
- Run simulation on the main thread initially. Introduce background computation only after profiling; workers must not mutate the live session or scene tree.
