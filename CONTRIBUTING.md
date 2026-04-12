# Contributing

## Scope

This project is implemented against `SPECIFICATION.md`.

Before proposing behavior changes:

- verify they are in scope,
- if not in scope, propose a spec update first.

## Development Setup

1. Start PostgreSQL (`docker compose up -d`).
2. Build and test solution:
   - `dotnet build .\Vbodlaci.sln -c Debug`
   - `dotnet test .\Vbodlaci.sln -c Debug`
3. Run app:
   - `dotnet run --project .\Vbodlaci.Web\Vbodlaci.Web.csproj`

## Pull Request Expectations

Every PR should include:

- concise summary of behavior changes,
- mapping to spec section(s),
- test evidence (`build`, `test`, smoke where relevant),
- explicit list of assumptions if any temporary decisions were made.

## Guardrails

- Keep `mockup/` untouched.
- Keep website copy in Czech.
- Do not introduce non-MVP features unless explicitly requested.
- Avoid destructive git operations.
