# Transaction Entry And History - Plan Brief

> Full plan: `context/changes/transaction-entry-history/plan.md`

## What & Why

This change adds the first real finance API slice: categories, transaction creation, and transaction history. It gives the future React frontend a stable backend contract without mixing UI work into this change.

## Starting Point

The local persistence foundation is already implemented and reviewed. The app has `FinanceDbContext`, seeded default local profile, seeded categories, transactions, migrations, and persistence tests, but `Program.cs` still exposes only the generated weather endpoint.

## Desired End State

The backend exposes `GET /api/categories`, `POST /api/transactions`, and `GET /api/transactions`. Transactions are saved for the default local profile, categories are validated against transaction type except for `Other`, and history returns newest items first with a default limit of 50.

## Key Decisions Made

| Decision | Choice | Why |
| --- | --- | --- |
| UI scope | Backend-only | React will be a separate change after the API contract is stable. |
| API surface | Categories, create, list | Covers S-01 without edit/delete overreach. |
| Category validation | Strict type match except `Other` | Keeps data consistent while preserving fallback category behavior. |
| History | Newest first, default limit 50, max 100 | Gives useful history and avoids unbounded reads. |
| Validation | `Amount > 0`, required date, description max 500 | Matches the current model and MVP rules. |
| Errors | ProblemDetails / ValidationProblem | Idiomatic ASP.NET Core contract for future frontend use. |
| Tests | Endpoint integration tests | Protects the first public API contract. |

## Scope

**In scope:**

- `GET /api/categories`
- `POST /api/transactions`
- `GET /api/transactions`
- DTOs for category, create transaction, and transaction history
- validation for amount, date, description, category existence, and category/type match
- endpoint integration tests
- removal of the generated weather sample once finance endpoints exist

**Out of scope:**

- React frontend or UI
- transaction edit/delete
- category management
- dashboard, statistics, charts, or goals
- currency, recurring transactions, imports, bank integrations, auth, cloud, or AI
- new database tables or migration unless a blocking schema issue is discovered

## Architecture / Approach

Keep the feature backend-only and local-first. Add `Endpoints/FinanceEndpoints.cs` for route mapping and `Contracts/FinanceContracts.cs` for public DTOs. Use `FinanceDbContext` and the seeded default local profile, return DTOs rather than EF entities, and test the HTTP API through an integration test host isolated from real local finance data.

## Phases at a Glance

| Phase | What it delivers | Key risk |
| --- | --- | --- |
| 1. API Structure And Contracts | Feature structure, DTOs, test-host readiness, weather removal | Over-shaping abstractions before endpoints exist |
| 2. Categories And Transaction Endpoints | Three minimal finance endpoints | Validation drift around category/type matching |
| 3. Endpoint Integration Tests | HTTP tests for success and failure paths | Test host accidentally using real SQL Server data |
| 4. Verification And Documentation | Full verification and guidance updates | Docs drifting from local-first/backend-only scope |

**Prerequisites:** `local-persistence-boundary` implemented and reviewed.
**Estimated effort:** ~2-3 focused implementation sessions across 4 phases.

## Open Risks & Assumptions

- The current schema is sufficient; no migration is expected.
- History ordering can use transaction date plus a deterministic secondary ordering because the model has no created timestamp.
- `GET /api/transactions` uses default limit 50, accepts 1-100, and rejects values outside that range with `400 ValidationProblem`.
- The test host should use an isolated test database provider and must not touch the developer's real local finance database.

## Success Criteria (Summary)

- A client can fetch categories, create a valid income or expense, and list newest transactions first.
- Invalid requests return idiomatic problem responses.
- The full test suite passes and includes endpoint-level coverage for the first finance API contract.
