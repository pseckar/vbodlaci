# AGENTS Guide

## Primary Rule

All implementation work in this repository is **spec-driven**.

1. `SPECIFICATION.md` is the canonical product contract.
2. For MVP implementation, the specification is frozen.
3. Code must follow spec; code must not redefine product behavior.
4. If a new requirement appears, stop implementation and start a new spec update + planning cycle.

## Required Workflow For Any Change

1. Read relevant spec sections before editing code.
2. Link each change to a specific spec section in the PR/commit notes.
3. Keep public-facing copy and labels in Czech.
4. Do not expand scope beyond MVP without explicit spec update.

## Quality Gates

Every substantial change should pass:

- `dotnet build .\Vbodlaci.sln -c Debug`
- `dotnet test .\Vbodlaci.sln -c Debug`
- browser smoke check (`tests/browser-smoke`)
