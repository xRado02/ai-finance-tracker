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
- `Endpoints/FinanceEndpoints.cs` maps categories, transactions, goals and `GET /api/dashboard/summary`.
- `Contracts/FinanceContracts.cs` contains public finance API DTOs; do not expose EF entities directly from endpoints.
- `frontend/` is the Vite + React + TypeScript frontend for the transaction, goals and dashboard API.
- `Migrations/` contains the EF Core schema history; run `dotnet ef database update` after restoring packages.
- `Properties/launchSettings.json` controls local launch profiles.
- `tests/AiFinanceTracker.Tests/AiFinanceTracker.Tests.csproj` is the dedicated xUnit test project for backend tests.
- `context/foundation/` holds living product docs: PRD, shape notes, and tech-stack hand-off.
- `context/changes/bootstrap-verification/verification.md` records scaffold, restore, build, and audit results.

## Build, Restore, And Audit

Run these from the repository root:

- `dotnet restore .\ai-finance-tracker.csproj` restores NuGet packages.
- `dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false` builds after restore on Windows.
- `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false` runs the backend test suite on Windows.
- `dotnet ef database update --project .\ai-finance-tracker.csproj` applies local migrations.
- `dotnet list .\ai-finance-tracker.csproj package --vulnerable --include-transitive` audits direct and transitive NuGet packages.
- `npm install --no-audit --no-fund` from `frontend/` installs frontend dependencies.
- `npm run dev` from `frontend/` starts the frontend dev server.
- `npm run typecheck` from `frontend/` runs the frontend TypeScript check.
- `npm run build` from `frontend/` builds the frontend.

On Windows, run the backend with `dotnet run --project .\ai-finance-tracker.csproj --launch-profile http -p:UseAppHost=false` to avoid local apphost file access issues.

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

## 10xDevs AI Toolkit - Module 3, Lesson 4 (E2E Tests)

**For E2E tests, use the `/10x-e2e` skill.** It is the single source of truth
for the workflow — risk → seed test + rules → generate → review against the five
anti-patterns → re-prompt → verify. The skill's `references/` carry the full
rules, anti-patterns, seed pattern, and prompt-template.

A few hard rules that hold even before you invoke the skill:

- **Locators:** `getByRole` / `getByLabel` / `getByText` first; `getByTestId`
  only when accessibility attributes are ambiguous. Never CSS selectors, XPath,
  or DOM structure.
- **Never `page.waitForTimeout()`.** Wait for state: `toBeVisible()`,
  `waitForURL()`, `waitForResponse()`.
- **Test independence + cleanup.** Each test runs standalone — its own setup,
  action, assertion, and cleanup; unique ids (timestamp suffix) so parallel runs
  and re-runs don't collide.

Two boundaries to keep straight:

- **DOM (snapshot) is the default.** Vision (`--caps=vision`) is a supplement for
  visual-only risks (layout, z-index, animation); for pixel regression prefer
  deterministic tools (`toMatchSnapshot`, Argos, Lost Pixel). VLM model
  selection/cost is a debugging topic (Lesson 5), not testing.
- **Healer helps on selectors, harms on logic.** A changed selector → healer
  re-finds it (route through PR review). A changed business behavior → healer
  masks the bug; that failing-test-to-fix case is Lesson 5.

<!-- END @przeprogramowani/10x-cli -->
