# Repository Guidelines

AI Finance Tracker is a local-first personal finance MVP. Treat @context/foundation/prd.md as the product contract and @context/foundation/tech-stack.md as the stack hand-off; do not add AI, cloud deployment, bank integrations, realtime, background jobs, or multi-user auth unless those files are updated first.

## Hard Rules

- Preserve `context/`; it contains the shaping chain, PRD, stack decision, and bootstrap verification log.
- Keep the MVP local-first: financial data must remain local and no external services should receive user finance data.
- Use the default local profile model from the PRD. Do not introduce email/password auth or account management in MVP work.
- Keep categorization user-correctable. Any automatic or default category assignment must allow the user to fix it.

## Project Structure

- `ai-finance-tracker.csproj` is the ASP.NET Core Web API project file targeting `net9.0`.
- `Program.cs` wires API services and maps finance endpoints; keep feature route handlers out of `Program.cs`.
- `Endpoints/FinanceEndpoints.cs` maps `GET /api/categories`, `POST /api/transactions`, and `GET /api/transactions`.
- `Contracts/FinanceContracts.cs` contains public finance API DTOs; do not expose EF entities directly from endpoints.
- `Properties/launchSettings.json` controls local launch profiles.
- `tests/AiFinanceTracker.Tests/AiFinanceTracker.Tests.csproj` is the dedicated xUnit test project for backend tests.
- `context/foundation/` holds living product docs: PRD, shape notes, and tech-stack hand-off.
- `context/changes/bootstrap-verification/verification.md` records scaffold, restore, build, and audit results.

## Build, Restore, And Audit

Run these from the repository root:

- `dotnet restore .\ai-finance-tracker.csproj` restores NuGet packages.
- `dotnet build .\ai-finance-tracker.csproj --no-restore` builds after restore.
- `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj` runs the backend test suite.
- `dotnet list .\ai-finance-tracker.csproj package --vulnerable --include-transitive` audits direct and transitive NuGet packages.

## Coding Conventions

- Keep C# nullable reference types and implicit usings enabled as configured in @ai-finance-tracker.csproj.
- Use `AiFinanceTracker` as the root namespace for new backend code.
- Prefer feature-oriented names tied to the PRD nouns: transactions, categories, dashboard summaries, and financial goals.
- Do not reintroduce generated weather sample names or endpoints.

## Testing Guidelines

Use the dedicated xUnit test project at `tests/AiFinanceTracker.Tests/AiFinanceTracker.Tests.csproj`; do not mix tests into the API project. Run tests with `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj`.

## Commit And PR Notes

There is no git history in this repo yet, so no commit convention has been established. Until one is chosen, keep commits small and mention the PRD requirement or FR number touched when possible.

<!-- BEGIN @przeprogramowani/10x-cli -->

## 10xDevs AI Toolkit - Module 2, Lesson 3

Review AI-generated code before merge with the **implementation review chain**:

```
/10x-implement -> /10x-impl-review -> triage -> (/10x-lesson | fix | skip | disagree)
```

`/10x-impl-review` is the lesson focus. Review is a quality gate, not an instruction to fix every finding.

### Task Router - Where to start

| Skill | Use it when |
| --- | --- |
| **Code review (lesson focus)** | |
| `/10x-impl-review <change-id>` | You have implemented code and want a structured review before merge. The skill checks plan adherence, scope discipline, safety and quality, architecture, pattern consistency, and success criteria, then presents findings for triage. |
| **Recurring lesson outcome** | |
| `/10x-lesson` | A finding reveals a recurring project rule or agent failure pattern. Record it in `context/foundation/lessons.md` instead of treating it as a one-off note. |

### Triage discipline

- Severity says how bad the finding is. Impact says how much the decision matters now.
- Valid outcomes: fix now, fix differently, skip, accept as risk, record as recurring rule (`/10x-lesson`), disagree.
- Fix critical findings. Do not burn hours on low-impact observations just because the agent found them.
- Conscious skipping of low-impact findings is a valid review outcome, not negligence.
- If you disagree with a finding, record why. Wrong agent reasoning is also signal.

### Review boundaries

- This lesson reviews implemented code. It does not create the plan, execute new phases, or teach CI review.
- Testing strategy and quality gates are introduced in Module 3.
- Do not use `/10x-contract` as a triage outcome in this lesson.

### Paths used by this lesson

- `context/changes/<change-id>/plan.md` - expected implementation contract
- `context/changes/<change-id>/reviews/` - review output
- `context/foundation/lessons.md` - recurring lessons

Skills must not write to `context/archive/`. Archived changes are immutable; if a resolved target path starts with `context/archive/`, abort with: "This change is archived. Open a new change with `/10x-new` instead."

<!-- END @przeprogramowani/10x-cli -->
