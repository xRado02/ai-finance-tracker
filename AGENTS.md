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
- `frontend/` is the Vite + React + TypeScript frontend for the current transaction/category API.
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

## 10xDevs AI Toolkit - Module 2, Lesson 4

Prepare for a harder implementation stream with the **research-backed planning chain**:

```
internal research (/10x-research) + external research (exa.ai, Context7) -> /10x-plan -> /10x-implement -> success
```

The lesson focus is distinguishing internal from external research and using evidence to back planning decisions.

### Task Router - Where to start

| Skill | Use it when |
| --- | --- |
| **Internal research (lesson focus)** | |
| `/10x-research <change-id>` | You need evidence from the existing codebase — patterns, conventions, integration points, or existing implementations. Runs parallel sub-agents over the repo and writes structured findings to `research.md`. |
| **External research (lesson focus)** | |
| exa.ai | You need AI-native web search for library comparisons, best practices, or ecosystem context that the codebase cannot answer. |
| Context7 (`resolve-library-id` → `get-library-docs`) | You need live, current documentation for a specific library or framework. Resolves a library ID first, then fetches relevant doc pages. |
| **Framing spare wheel** | |
| `/10x-frame <change-id>` | The plan won't converge, the plan doesn't deliver expected results, or persistent drift keeps breaking the implementation. Use as an escape hatch on a separate problem (demonstrated on Space Explorers example), not as pre-research ritual. |
| **Planning and execution** | |
| `/10x-plan <change-id>` / `/10x-implement <change-id> phase <n>` | Use the same planning and execution chain from Lesson 2, now with upstream research evidence feeding the plan. |

### Research discipline

- Internal research (`/10x-research`) answers "what does our codebase already do?" — patterns, schemas, conventions, integration points.
- External research (exa.ai, Context7) answers "what should we do?" — library capabilities, API docs, ecosystem best practices.
- Combine both as evidence-backed input to `/10x-plan`. A plan without research evidence on a non-trivial stream is a guess.
- Agent-friendly docs (`llms.txt`, markdown-for-agents, `/md` endpoints) are a quality signal for library selection — libraries that publish agent-readable docs integrate faster.

### `/10x-frame` as spare wheel

Three triggers for reaching for `/10x-frame`:
1. The plan won't converge — research keeps opening more questions instead of narrowing to a contract.
2. The plan doesn't deliver — implementation repeatedly fails to meet success criteria.
3. Persistent drift — the implementation keeps diverging from the plan in ways that suggest the problem was mis-framed.

Demonstrated on a Space Explorers example, not the SRS path. It is an escape hatch, not a mandatory step.

### Paths used by this lesson

- `context/changes/<change-id>/research.md` - internal research output
- `context/changes/<change-id>/frame.md` - framing output when needed
- `context/changes/<change-id>/plan.md` - evidence-backed implementation contract
- `context/foundation/lessons.md` - recurring rules and pitfalls

Skills must not write to `context/archive/`. Archived changes are immutable; if a resolved target path starts with `context/archive/`, abort with: "This change is archived. Open a new change with `/10x-new` instead."

<!-- END @przeprogramowani/10x-cli -->
