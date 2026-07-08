---
bootstrapped_at: 2026-06-14T20:33:35.1485330+02:00
starter_id: dotnet
starter_name: ".NET (ASP.NET Core webapi)"
project_name: ai-finance-tracker
language_family: dotnet
package_manager: dotnet
cwd_strategy: subdir-then-move
bootstrapper_confidence: verified
phase_3_status: ok
audit_command: "dotnet list package --vulnerable --include-transitive"
---

## Hand-off

```yaml
---
starter_id: dotnet
package_manager: dotnet
project_name: ai-finance-tracker
hints:
  language_family: dotnet
  team_size: solo
  deployment_target: self-host
  ci_provider: github-actions
  ci_default_flow: manual-promotion
  bootstrapper_confidence: verified
  path_taken: standard
  quality_override: false
  self_check_answers: null
  has_auth: false
  has_payments: false
  has_realtime: false
  has_ai: false
  has_background_jobs: false
---
```

AI Finance Tracker is a solo, local-first web-app MVP with a 3-week after-hours target, no server authentication, no payments, no realtime, no AI in scope, and no background jobs. The recommended starter for this product shape in the .NET family is ASP.NET Core Web API, which clears the agent-friendly gates through strong typing, conventions, mature documentation, and broad ecosystem familiarity. Bootstrapper confidence is verified, so scaffolding should be smooth. Deployment is set to self-host because the MVP runs locally rather than in the cloud; GitHub Actions with manual promotion keeps CI focused on build and test quality. React and Microsoft SQL Server remain the intended frontend and database context for the bootstrap/configuration step.

## Pre-scaffold verification

| Signal | Value | Severity | Notes |
| --- | --- | --- | --- |
| npm package | not run | n/a | not a JavaScript-family starter |
| GitHub repo | not run | n/a | card docs_url points to Microsoft Learn, not a GitHub repository |

## Scaffold log

**Resolved invocation**: `dotnet new webapi -n .bootstrap-scaffold --no-restore`
**Strategy**: subdir-then-move
**Exit code**: 0
**Files moved**: 6
**Conflicts (.scaffold siblings)**: none
**.gitignore handling**: absent in scaffold
**.bootstrap-scaffold cleanup**: deleted

Moved files:

- `Properties/launchSettings.json`
- `ai-finance-tracker.csproj` (renamed from `.bootstrap-scaffold.csproj`)
- `.bootstrap-scaffold.http`
- `appsettings.Development.json`
- `appsettings.json`
- `Program.cs`

Follow-up applied: the generated project file was renamed from `.bootstrap-scaffold.csproj` to `ai-finance-tracker.csproj`, and the root namespace was updated to `AiFinanceTracker`.

## Restore and build verification

**Restore command**: `dotnet restore .\ai-finance-tracker.csproj`
**Restore status**: passed

```text
Restored D:\10DEVS\ai-finance-tracker.csproj.
```

**Build command**: `dotnet build .\ai-finance-tracker.csproj --no-restore`
**Build status**: passed

```text
ai-finance-tracker -> D:\10DEVS\bin\Debug\net9.0\ai-finance-tracker.dll
Build succeeded.
Warnings: 0
Errors: 0
```

## Post-scaffold audit

**Tool**: `dotnet list package --vulnerable --include-transitive`
**Status**: passed
**Summary**: 0 vulnerable packages found, including transitive packages, using the current NuGet sources.

Output:

```text
Used the following sources:
   https://api.nuget.org/v3/index.json

The given project "ai-finance-tracker" has no vulnerable packages given the current sources.
```

## Hints recorded but not acted on

| Hint | Value |
| --- | --- |
| bootstrapper_confidence | verified |
| quality_override | false |
| path_taken | standard |
| self_check_answers | null |
| team_size | solo |
| deployment_target | self-host |
| ci_provider | github-actions |
| ci_default_flow | manual-promotion |
| has_auth | false |
| has_payments | false |
| has_realtime | false |
| has_ai | false |
| has_background_jobs | false |

## Next steps

Next: a future skill will set up agent context (CLAUDE.md, AGENTS.md). For now, your project is scaffolded and verified.

Useful manual steps in the meantime:

- Run `git init` if you have not already to start your own repo history.
- Continue with frontend/database setup and CI workflow definition in the next project setup pass.
