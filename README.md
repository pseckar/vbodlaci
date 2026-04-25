# V bodláčí Web (MVP)

Spec-driven ASP.NET Razor Pages application for the `V bodláčí` brand.

## Stack

- .NET 10, ASP.NET Core Razor Pages
- Entity Framework Core + PostgreSQL
- ASP.NET Identity (local admin auth)
- GitHub Actions CI

## Project Structure

- `SPECIFICATION.md` - frozen MVP product contract
- `Vbodlaci.Web` - web application
- `Vbodlaci.Web.Tests` - unit/integration tests
- `tests/browser-smoke` - headless browser smoke checks

## Local Run

1. Start DB:

```powershell
docker compose up -d
```

2. Configure development admin (optional override):

```powershell
dotnet user-secrets set "Admin:Email" "admin@vbodlaci.local" --project .\Vbodlaci.Web\Vbodlaci.Web.csproj
dotnet user-secrets set "Admin:Password" "Admin12345" --project .\Vbodlaci.Web\Vbodlaci.Web.csproj
```

3. Run app:

```powershell
dotnet run --project .\Vbodlaci.Web\Vbodlaci.Web.csproj
```

4. Open:

- `http://localhost:5270`
- admin login: `/Identity/Account/Login`

## Build and Test

```powershell
dotnet build .\Vbodlaci.sln -c Debug
dotnet test .\Vbodlaci.sln -c Debug
```

## Local DB Compatibility Reset

If your local PostgreSQL still contains old prototype schema, the app now auto-detects incompatible `Courses` columns in **Development** and recreates the database.

- The reset terminates active connections for the target DB before `DROP DATABASE`, so the common "cannot drop open database" issue is handled automatically.
- This auto-reset is **Development only**. It is not active in production.

## Browser Smoke Test

```powershell
pwsh .\scripts\run-browser-smoke.ps1
```

## Staging Deploy (GCP VM)

- CI/CD runbook: `docs/staging-cicd.md`
- One-time VM bootstrap helper: `scripts/staging/bootstrap-vm.ps1`

## SMTP Configuration

Configure `Email:Smtp` in appsettings/environment:

- `Enabled`
- `Host`, `Port`, `UserName`, `Password`
- `EnableSsl`
- `From`

When SMTP is disabled, app uses noop delivery but still logs attempts.

## Legal Data Placeholder Rule

`LegalIdentity` values are placeholders in non-production.

Production startup blocks if `LegalIdentity` contains `TODO` placeholder values.
