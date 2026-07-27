# Test Plan

## 1. Strategy and principles

The goal is not maximum coverage. The goal is confidence in the financial
behaviours whose failure could lose data, misstate the user's finances, or make
the local application unusable.

Testing follows a risk-first pyramid:

- Prefer backend integration tests for persisted financial behaviour and API
  contracts.
- Use focused unit tests only where a calculation has meaningful branches and
  can be exercised without mirroring its implementation.
- Keep browser coverage to one critical user flow and one recovery behaviour.
- Treat migrations and local database compatibility as release risks even
  though generated migrations are excluded from churn analysis.
- Do not change product behaviour to make a test easier to write.

## 2. Risk inventory

| # | Risk | Impact | Likelihood | Evidence source | Preferred response |
| --- | --- | --- | --- | --- | --- |
| 1 | A transaction is not persisted correctly, is lost, or add/delete behaviour regresses. | High | High | PRD: local persistence and transaction flow; user concerns Q1/Q4; churn: `Endpoints/` 9, `Persistence/` 7 | API integration tests covering successful add, persistence, delete, and not-found behaviour. |
| 2 | Monthly totals, monthly balance, or total balance are wrong, especially when the initial balance is present. | High | High | PRD: dashboard totals and financial summaries; user concerns Q1/Q4; churn: `Endpoints/` 9, `Contracts/` 7 | Integration tests with deliberately asymmetric income/expense data across months; assert initial balance affects only total balance. |
| 3 | Recurring transactions are generated for the wrong month, omitted, or duplicated within one month. | High | Medium | Existing MVP scope and user concerns Q1/Q4; churn: `Endpoints/` 9, `Persistence/` 7 | Integration tests that generate twice for one period and once for another period. |
| 4 | The application starts against an outdated schema or a migration leaves the local database incompatible. | High | Medium | User-reported database incidents Q2; SQL Server stack decision; explicit migration-risk requirement | Schema/migration smoke verification from an empty database plus documentation of update and recovery commands. |
| 5 | Goal forecast reports an incorrect month count/date or mishandles achieved goals, missing data, and non-positive surplus. | Medium | Medium | PRD: goal progress; user concerns Q1/Q4; churn: `Endpoints/` 9 | Focused integration or unit tests for the four forecast outcomes, with deterministic dates and amounts. |
| 6 | After the API is unavailable, the frontend cannot recover without reload or loses the selected month. | Medium | High | User concerns Q1/Q4 and confirmed recovery flow; churn: `frontend/src/` 32, `frontend/src/api/` 15 | One stable browser smoke/E2E scenario; retain manual verification if network control is brittle. |
| 7 | Invalid user input reaches persistence or creates an inconsistent API result. | High | Medium | PRD: user-entered transactions and correctable categorisation; user concerns Q4; churn: `Contracts/` 7, `Endpoints/` 9 | API integration tests for amount, date, required text, type, category, and profile ownership boundaries. |

Response guidance:

- A failing High-impact invariant blocks commit and push.
- Migration failures are investigated against a disposable database; tests must
  never modify the user's local finance database.
- Browser tests should verify user-observable outcomes, not CSS structure or
  internal React state.
- A newly discovered product bug may receive the smallest necessary fix, but
  feature expansion remains out of scope.

## 3. Phased rollout

| Phase | Change ID | Scope | Risks covered | Test level | Status |
| --- | --- | --- | --- | --- | --- |
| 1 | `testing-critical-finance-invariants` | Add a small set of high-signal tests for transaction persistence, monthly and total balances, initial balance, recurring deduplication, forecast branches, and validation. | 1, 2, 3, 5, 7 | Integration + focused unit | complete |
| 2 | `testing-database-migrations` | Verify the schema can be created or migrated safely in isolation and document failure recovery. | 4 | Integration + schema smoke | not started |
| 3 | `testing-critical-browser-flow` | Add one minimal browser flow for selecting a month, adding a transaction, seeing history/summary updates, and assess stable recovery coverage. | 1, 2, 6 | E2E + manual smoke | planned; infrastructure prerequisite |
| 4 | `testing-quality-gates` | Align local and CI commands, add only lightweight gates/hooks, and document the debugging checklist. | Cross-cutting | Build gates + runbook | complete |

Progress for each phase belongs in
`context/changes/<change-id>/plan.md`. The test plan records rollout state only.

## 4. Current test infrastructure

| Area | Current state | Tooling | Gap |
| --- | --- | --- | --- |
| Backend unit/integration | Present; 39 passing tests at baseline | xUnit 2.9.2, ASP.NET Core MVC Testing 9.0.14, EF Core SQLite 9.0.14 | Coverage is concentrated in two files and should be extended only for listed risks. |
| API contract | Exercised through the backend test host | `WebApplicationFactory` and HTTP requests | Confirm monthly aggregates, validation, recurring deduplication, and forecast branches. |
| Database | Test isolation uses SQLite; the application uses SQL Server/LocalDB | EF Core providers | No explicit disposable SQL Server migration smoke test. |
| Frontend static checks | Present | TypeScript typecheck and Vite production build | No frontend unit test runner; do not add one unless it pays for a listed risk. |
| Browser/E2E | Not automated | Manual smoke tests; Playwright is available through the browser tooling | One critical flow may justify minimal Playwright setup. |
| CI | Present on GitHub Actions | Backend build/test and frontend typecheck/build | Reuse these commands as mandatory gates; avoid duplicate pipelines. |

## 5. Quality gates

Before any Module 3 commit:

1. `dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false`
2. `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false`
3. `npm run typecheck` from `frontend/`
4. `npm run build` from `frontend/`

Phase 2 additionally requires an isolated schema/migration smoke check. Phase 3
requires either a stable automated browser pass or a recorded manual smoke
result. Failing tests are reported and fixed; they are never removed or hidden
to obtain a green result.

## 6. Test cookbook

### 6.1 Focused calculation test

To be added in Phase 1 only if forecast or balance calculation is exposed as a
meaningful deterministic unit. Otherwise cover it through the API.

### 6.2 API integration test

Use the existing backend test host and isolated database pattern. Arrange
explicit profiles, categories, dates, and values; call the public endpoint; then
assert both the HTTP contract and persisted result.

Run:

`dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false`

### 6.3 Browser test

Deferred to Phase 3. Keep one scenario with unique transaction data, a selected
month, and assertions on history plus summary. Do not rely on fragile visual
selectors.

### 6.4 Migration/schema smoke

Deferred to Phase 2. Use a disposable database, apply all migrations from zero,
and verify the application can query the core tables. Never point this check at
the user's normal LocalDB database.

### 6.5 Frontend recovery

Prefer a manual smoke test unless API outage and restoration can be controlled
deterministically. Required observations: Polish error state, retry control,
data recovery without reload, and preserved month selection.

## 7. Explicit exclusions

- Tests written only to increase a coverage percentage.
- Broad UI snapshot tests or detailed CSS assertions.
- Large E2E suites, advanced visual testing, or heavy browser infrastructure.
- Sentry, cloud monitoring, external telemetry, or production deployment work.
- Exhaustive low-value permutations that duplicate framework behaviour.
- Product features, UI redesigns, refactors, or contract changes unrelated to a
  test-proven bug.
- Tests that mirror private implementation instead of observable behaviour.

## 8. Maintenance

- Created: 2026-07-27.
- Review after each rollout phase and after any production-like data or migration
  incident.
- Update the risk inventory when behaviour changes; do not add tests merely
  because code churned.
- Keep completed phase evidence in the corresponding change folder and preserve
  this document as the cross-project quality strategy.
