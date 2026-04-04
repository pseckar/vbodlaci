# V bodlacich - MVP (ASP.NET Razor Pages + PostgreSQL)

This is phase 1 of the project: a locally runnable .NET 10 web app with:
- public Czech pages (`/`, `/techniky`, `/kurzy`),
- course administration (`/admin/courses`) using ASP.NET Identity + `Admin` role,
- PostgreSQL via Docker Compose,
- email service scaffolding (`Noop`/`SMTP`) for future phases,
- GitHub Actions CI workflow (build + test).

## Tech Stack
- .NET 10 (Razor Pages)
- Entity Framework Core + Npgsql
- ASP.NET Core Identity
- PostgreSQL (Docker Compose)
- xUnit integration tests

## Local Quick Start
1. Start PostgreSQL:
   ```powershell
   docker compose up -d
   ```
   PostgreSQL is available on `localhost:5432`.
2. Restore dependencies:
   ```powershell
   dotnet restore .\Vbodlaci.sln
   ```
3. Apply database migrations:
   ```powershell
   dotnet ef database update --project .\Vbodlaci.Web\Vbodlaci.Web.csproj --startup-project .\Vbodlaci.Web\Vbodlaci.Web.csproj
   ```
4. Run the app:
   ```powershell
   dotnet run --project .\Vbodlaci.Web\Vbodlaci.Web.csproj
   ```
5. Open `https://localhost:5001` or `http://localhost:5000`.

## Admin Login (Development)
Default development seed in `appsettings.Development.json`:
- email: `admin@vbodlaci.local`
- password: `Admin1234`

Recommended: override these values using User Secrets:
```powershell
dotnet user-secrets set "Admin:Email" "your-admin@example.com" --project .\Vbodlaci.Web\Vbodlaci.Web.csproj
dotnet user-secrets set "Admin:Password" "StrongPassword123" --project .\Vbodlaci.Web\Vbodlaci.Web.csproj
```

## SMTP (Scaffolded for Future Phases)
Default implementation is `NoopEmailService` (does not send emails).

To enable SMTP, set:
- `Email:Smtp:Enabled = true`
- `Email:Smtp:Host`, `Port`, `UserName`, `Password`, `EnableSsl`, `From`

## Tests
```powershell
dotnet test .\Vbodlaci.sln
```

Includes:
- home page smoke test,
- unauthorized redirect test for admin area,
- course create/read flow test (repository + admin list model).

## CI (GitHub Actions)
Workflow: `.github/workflows/ci.yml`
- `dotnet restore`
- `dotnet build`
- `dotnet test`

## EF CLI Note
If you see a `dotnet-ef` version warning, update the tool:
```powershell
dotnet tool update --global dotnet-ef
```
