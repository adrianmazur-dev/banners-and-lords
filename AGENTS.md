# Agent Instructions

## Working Rules

- Use English for code, documentation, game text, and project communication.
- Use docs/gdd.md for gameplay decisions and docs/architecture.md for technical decisions.
- Deliver small, complete increments. Preserve existing user changes and keep unrelated work out of scope.
- Use Godot MCP for editor inspection and scene operations; verify the active project and scene before editing. Preserve unsaved user work.
- After C# changes, run `dotnet build banners-and-lords.sln`. Check scene and gameplay changes in Godot; add meaningful tests for important rules and edge cases.
- Use focused Conventional Commits when committing. Keep .temp/ and generated files out of Git; retain source assets, licenses, and Godot .uid files.

## Completing Each Iteration or Plan

- Remove temporary files, obsolete code, unused assets, and debugging scaffolding introduced by the task. Preserve user-owned files and archives.
- Review the complete change for correctness, scope, architectural consistency, and unnecessary complexity. Resolve findings before reporting completion.
- Update docs/gdd.md when gameplay decisions change and docs/architecture.md when technical rules or architecture change. Keep intended design distinct from implemented behavior.
- Update AGENTS.md when the user establishes or changes a lasting working rule. Replace outdated instructions; keep all three documents concise and free of task history.
- Report the outcome, verification performed, and any remaining limitations.
