# Technical Architecture

These rules govern implementation. The current project contains only the main scene and C# bootstrap; simulation modules are introduced incrementally.

## Boundaries

- Use Godot .NET and C#. Engine and SDK versions are defined by the project files and global.json.
- Build a modular monolith organized by gameplay feature. Add layers and abstractions only for concrete responsibilities.
- Keep simulation in an engine-independent C# library under src/Simulation; keep Godot adapters under src/Godot. When adding the library, reference it explicitly and exclude its sources and test sources from the Godot project's compilation.
- Within simulation features, use Contracts for public APIs, Domain for state and rules, and Application for coordination. Add Infrastructure for technical adapters when needed.
- Features access other features only through Contracts. Keep dependencies acyclic; composition code may reference concrete implementations to wire them together.
- Construct dependencies explicitly at startup. Keep bootstrap code limited to composition and lifecycle management.

## State and Execution

- Simulation owns game state independently of nodes and scenes. Each piece of state has one owner; other modules refer to it by stable identifiers.
- Godot handles input, presentation, audio, and editor integration. Removing a view or failing an animation must not change simulation results.
- Advance the campaign through fixed simulation ticks with explicit system ordering. Schedule daily economy separately from movement; frame rate must not change the rules.
- Keep campaign pause and speed in the session. Initially, campaign time stops during tactical battles while UI and battle presentation remain active.
- Change state through validated commands; expose read-only data through queries. Player and AI actions use the same rules. Publish events only after changes are committed.
- Coordinate operations spanning modules so they either complete together or leave state unchanged. Apply each battle result to the campaign exactly once.

## Data and Persistence

- Keep content definitions separate from mutable instances. Convert Godot Resources to plain C# definitions at the engine boundary.
- Save versioned data snapshots, not scene trees or engine objects. Include stable IDs, simulation time, pending work, and serializable random generator state.
- Use integer units for money and goods with explicit rounding. Use controlled randomness and stable ordering for repeatable simulation.
- Run simulation on the main thread initially. Introduce background computation only after profiling; workers must not mutate the live session or scene tree.
