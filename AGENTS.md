# Agent Instructions

## Working Rules

- Use English for code, documentation and game text.
- Use docs/brief.md for implemented systems, docs/gdd.md for game design, and docs/architecture.md for technical rules.
- Deliver small, complete increments. Preserve existing user changes and keep unrelated work out of scope.
- Use Godot MCP for editor inspection and scene operations; verify the active project and scene before editing. Preserve unsaved user work.
- After C# changes, run `dotnet build banners-and-lords.sln`. Check scene and gameplay changes in Godot; add meaningful tests for important rules and edge cases.
- Use focused Conventional Commits when committing. Keep .temp/ and generated files out of Git; retain source assets, licenses, and Godot .uid files.

## Completing Each Iteration or Plan

- Remove temporary files, obsolete code, unused assets, and debugging scaffolding introduced by the task. Preserve user-owned files and archives.
- Review the complete change for correctness, scope, architectural consistency, and unnecessary complexity. Resolve findings before reporting completion.
- Update docs/brief.md when implemented systems change, docs/gdd.md when design decisions change, and docs/architecture.md when technical rules change. Keep brief.md a current snapshot, not a changelog.
- Update AGENTS.md when the user establishes or changes a lasting working rule. Replace outdated instructions; keep instructions and project documents concise and free of task history.
- Report the outcome, verification performed, and any remaining limitations.
