# Transaction Entry And History Implementation Plan

## Overview

This change adds the first finance API slice: a backend-only contract for listing categories, creating income or expense transactions, and reading transaction history. It builds on the completed local persistence boundary and keeps the MVP local-first.

This is an API-first change. React frontend work is explicitly deferred to a separate change after the backend contract is stable.

## Current State Analysis

The project is an ASP.NET Core Web API targeting .NET 9.0. `Program.cs` still contains the generated weather sample and currently has no finance endpoints.

The local persistence foundation is already implemented and reviewed. `FinanceDbContext` exposes local profiles, categories, and transactions. The database seeds one deterministic default local profile and the agreed startup categories, including `Other` as the required fallback.

The existing test project verifies EF Core model configuration, deterministic seed data, and transaction save/read behavior. It does not yet host or test the HTTP API.

## Desired End State

After this change, the backend exposes a minimal finance API:

- `GET /api/categories`
- `POST /api/transactions`
- `GET /api/transactions`

The user-facing flow is not available in React yet, but the stable backend contract exists for the upcoming frontend slice. The API stores transactions for the seeded default local profile, validates transaction input, rejects category/type mismatches except for `Other`, and returns newest transactions first with a simple limit.

## Key Decisions

| Decision | Choice | Rationale |
| --- | --- | --- |
| UI scope | Backend-only; React in a separate change | Keeps this change focused on the first stable API contract. |
| API surface | Categories, create transaction, list transactions | Covers category selection, transaction entry, and history without edit/delete scope. |
| Category/type validation | Reject mismatched category type except `Other` | Uses existing `Category.AppliesTo` and keeps stored data consistent. |
| History shape | Newest first, default limit 50, valid range 1-100 | Provides useful history while keeping list size bounded and explicit. |
| Field validation | `Amount > 0`, required date, optional description max 500 | Matches the MVP model and avoids zero/negative/manual correction semantics for now. |
| Response DTO | Flat transaction DTO with `categoryName` | Gives the future frontend table/list everything it needs without nested contracts. |
| Error style | ASP.NET Core `ProblemDetails` / `ValidationProblem` | Idiomatic and future frontend-friendly. |
| Test scope | Endpoint integration tests plus persistence tests where useful | Protects the first public API contract. |

## What We're NOT Doing

- No React frontend or UI.
- No dashboard, summaries, statistics, charts, or category analysis.
- No financial goals.
- No transaction edit/delete.
- No category management UI or custom category creation.
- No currency, recurring transactions, imports, bank integrations, notifications, reminders, or AI features.
- No auth, account management, or multi-profile support.
- No cloud deployment or external finance-data services.
- No new persistence tables or migration unless implementation discovers a truly blocking schema issue.

## Implementation Approach

Create a small backend API area for transactions and categories. Put endpoint mapping in `Endpoints/FinanceEndpoints.cs` and public request/response DTOs in `Contracts/FinanceContracts.cs`. Use the existing `FinanceDbContext` directly or through small focused helpers only if the endpoint code would otherwise become hard to read.

The API should use the seeded `FinanceDbContext.DefaultLocalProfileId` rather than accepting a profile id from clients. That preserves the single local profile contract and avoids introducing account management.

## Critical Implementation Details

`Other` is the only category allowed to bypass `AppliesTo` matching because it is the required fallback. Any category with `AppliesTo = Income` must only be accepted for income transactions, and any category with `AppliesTo = Expense` must only be accepted for expense transactions.

`GET /api/transactions` should return newest transactions first. If two transactions share the same `TransactionDate`, use a stable secondary ordering such as id or creation order available in the model. Since the current model has no created timestamp, do not add one in this change unless explicitly required by tests; keep ordering deterministic enough for current requirements.

## Phase 1: API Structure And Contracts

### Overview

Prepare the API structure, request/response DTOs, and test host support without yet completing all endpoint behavior.

### Changes Required:

#### 1. API feature structure

**File**: `Endpoints/FinanceEndpoints.cs`

**Intent**: Keep finance endpoint contracts out of `Program.cs` while replacing the generated sample with real API routing.

**Contract**: Add a finance endpoint mapping surface in the `AiFinanceTracker` namespace, for example a static endpoint extension that maps the category and transaction routes.

#### 2. Transaction DTOs

**File**: `Contracts/FinanceContracts.cs`

**Intent**: Define the first public backend contract without exposing EF entities directly.

**Contract**: Add:

- create transaction request with `Amount`, `Type`, `TransactionDate`, `Description`, and `CategoryId`
- transaction response with `Id`, `Amount`, `Type`, `TransactionDate`, `Description`, `CategoryId`, and `CategoryName`
- category response with `Id`, `Name`, and `AppliesTo`

#### 3. Program entrypoint testability

**File**: `Program.cs`

**Intent**: Allow endpoint integration tests to host the ASP.NET Core app.

**Contract**: Make the top-level `Program` type accessible to `WebApplicationFactory` using the standard partial `Program` pattern if needed.

#### 4. Weather sample removal

**File**: `Program.cs`

**Intent**: Replace generated sample functionality once real finance API work starts, matching repository guidance.

**Contract**: Remove `/weatherforecast`, `WeatherForecast`, and sample summary data when finance endpoint mappings are added.

### Success Criteria:

#### Automated Verification:

- API project builds after route and DTO structure changes.
- Test project builds after any test-host package or `Program` visibility changes.

#### Manual Verification:

- Confirm no React frontend or UI files were added.
- Confirm weather sample code is removed only as part of adding real finance API endpoints.

---

## Phase 2: Categories And Transaction Endpoints

### Overview

Implement the three minimal endpoints: list categories, create transaction, and list transaction history.

### Changes Required:

#### 1. `GET /api/categories`

**File**: `Endpoints/FinanceEndpoints.cs`

**Intent**: Let clients fetch the seeded category list from the local database instead of hardcoding categories.

**Contract**: Return all categories ordered predictably, including `Other`, with `id`, `name`, and `appliesTo`.

#### 2. `POST /api/transactions`

**File**: `Endpoints/FinanceEndpoints.cs`

**Intent**: Let clients create one income or expense transaction for the default local profile.

**Contract**: Accept the create transaction request. Validate:

- `Amount > 0`
- `TransactionDate` is present and parseable
- `Description` is optional and no longer than 500 characters
- `Type` is `Income` or `Expense`
- `CategoryId` exists
- category applies to the transaction type, unless the category is `Other`

Persist the transaction with a generated id and `FinanceDbContext.DefaultLocalProfileId`. Return the created transaction response with the category name. Use ASP.NET Core validation/problem responses for bad input and `404` for a missing category.

#### 3. `GET /api/transactions`

**File**: `Endpoints/FinanceEndpoints.cs`

**Intent**: Let clients review local transaction history.

**Contract**: Return transactions for the default local profile, newest first, with category name. Support an optional `limit` query parameter. If `limit` is omitted, use `50`. Accept values from `1` through `100`. Return `400 ValidationProblem` for values less than `1` or greater than `100`.

### Success Criteria:

#### Automated Verification:

- API project builds.
- Existing persistence tests still pass.

#### Manual Verification:

- Confirm endpoint behavior remains local-only and uses the default local profile.
- Confirm no goals, dashboard, statistics, edit/delete, auth, cloud, or AI scope was introduced.

---

## Phase 3: Endpoint Integration Tests

### Overview

Add integration tests for the first public API contract.

### Changes Required:

#### 1. Test host setup

**File**: `tests/AiFinanceTracker.Tests/AiFinanceTracker.Tests.csproj` and test support files under `tests/AiFinanceTracker.Tests/`

**Intent**: Host the API in tests and isolate persistence from the developer's local SQL Server database.

**Contract**: Add the required ASP.NET Core test package and test setup to run endpoint tests against a test database provider. The tests must not require Azure, cloud services, external finance data, or the developer's real finance database.

#### 2. Category endpoint tests

**File**: `tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs`

**Intent**: Prove clients can retrieve seeded categories.

**Contract**: Test that `GET /api/categories` returns the startup categories, including `Other`, with stable fields.

#### 3. Create transaction endpoint tests

**File**: `tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs`

**Intent**: Prove clients can save valid income and expense transactions through HTTP.

**Contract**: Test successful creation with seeded categories and default local profile. Assert response fields include `categoryName` and that persisted data can be read back.

#### 4. History endpoint tests

**File**: `tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs`

**Intent**: Prove transaction history is useful for the next frontend slice.

**Contract**: Test newest-first ordering and `limit` behavior.

#### 5. Validation endpoint tests

**File**: `tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs`

**Intent**: Protect the public contract against invalid finance data.

**Contract**: Test validation/problem responses for non-positive amount, too-long description, missing category, invalid category/type mismatch, and `Other` fallback acceptance.

### Success Criteria:

#### Automated Verification:

- Test project restores successfully.
- Test project builds successfully.
- Endpoint integration tests pass.
- API project still builds.

#### Manual Verification:

- Confirm tests exercise HTTP endpoints, not only EF persistence.
- Confirm tests do not require real user finance data or external services.

---

## Phase 4: Verification And Documentation Touch-Up

### Overview

Run the full verification loop and update repository guidance if commands or project structure changed.

### Changes Required:

#### 1. Verification commands

**File**: no code file; terminal verification

**Intent**: Prove the backend API slice is ready before the React frontend slice depends on it.

**Contract**: Run restore, build, test, and vulnerability audit from the repository root.

#### 2. Repository guidance update

**File**: `AGENTS.md`

**Intent**: Keep repository guidance accurate after the first finance API endpoints replace the weather sample.

**Contract**: Update only guidance that became inaccurate, such as references to weather sample still being present. Preserve hard rules about local-first MVP scope.

#### 3. Change metadata

**File**: `context/changes/transaction-entry-history/change.md`

**Intent**: Keep the change state aligned with the planning and implementation flow.

**Contract**: Implementation should record phase commits in `## Progress` and move status through implementation-owned states. Planning sets the change to `planned`.

### Success Criteria:

#### Automated Verification:

- Restore succeeds: `dotnet restore .\ai-finance-tracker.csproj`
- API build succeeds: `dotnet build .\ai-finance-tracker.csproj --no-restore`
- Test suite succeeds: `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj`
- Vulnerability audit succeeds: `dotnet list .\ai-finance-tracker.csproj package --vulnerable --include-transitive`

#### Manual Verification:

- Confirm MVP remains local-first and backend-only for this change.
- Confirm API contract covers category list, transaction creation, and transaction history.
- Confirm no out-of-scope features slipped in.

---

## Testing Strategy

### Unit Tests:

- Request validation helpers if implementation extracts validation outside endpoint handlers.
- Category/type matching behavior if implemented as a separate function or service.

### Integration Tests:

- `GET /api/categories` returns seeded categories including `Other`.
- `POST /api/transactions` creates valid income and expense transactions.
- `POST /api/transactions` rejects invalid amount, too-long description, missing category, and category/type mismatch.
- `POST /api/transactions` accepts `Other` for both transaction types.
- `GET /api/transactions` returns newest first, defaults `limit` to 50, accepts `limit` values from 1 through 100, and returns `400 ValidationProblem` outside that range.

### Manual Testing Steps:

1. Apply migrations to the local Development database if needed.
2. Run the API locally.
3. Call `GET /api/categories` and confirm seeded categories are returned.
4. Call `POST /api/transactions` with one valid income and one valid expense.
5. Call `GET /api/transactions` and confirm the newest transactions appear first with category names.
6. Try one invalid category/type mismatch and confirm a problem response.

## Performance Considerations

The MVP data volume is small. The only performance boundary in this slice is avoiding unbounded transaction history by supporting a default limit of 50 and a maximum limit of 100. More advanced filtering, pagination, indexing, and dashboard query optimization are deferred until real read patterns require them.

## Migration Notes

No schema migration is expected. This change should use the existing `LocalProfiles`, `Categories`, and `Transactions` tables from the local persistence boundary. If implementation discovers a required schema change, stop and update the plan before adding a migration.

## References

- Roadmap item: `context/foundation/roadmap.md` (`S-01 transaction-entry-history`)
- Product contract: `context/foundation/prd.md` (FR-001 through FR-005)
- Persistence foundation: `context/changes/local-persistence-boundary/plan.md`
- Implementation review of foundation: `context/changes/local-persistence-boundary/reviews/impl-review.md`
- Repository guidelines: `AGENTS.md`
- Current API entrypoint: `Program.cs`
- Existing persistence context: `Persistence/FinanceDbContext.cs`

## Progress

> Convention: `- [ ]` pending, `- [x]` done. Append `- <commit sha>` when a step lands. Do not rename step titles.

### Phase 1: API Structure And Contracts

#### Automated

- [x] 1.1 API project builds after route and DTO structure changes - f681759
- [x] 1.2 Test project builds after test-host changes - f681759

#### Manual

- [x] 1.3 No React frontend or UI files were added - f681759
- [x] 1.4 Weather sample removed only as part of adding real finance API endpoints - f681759

### Phase 2: Categories And Transaction Endpoints

#### Automated

- [x] 2.1 API project builds - dbbee92
- [x] 2.2 Existing persistence tests still pass - dbbee92

#### Manual

- [x] 2.3 Endpoint behavior remains local-only and uses the default local profile - dbbee92
- [x] 2.4 No goals, dashboard, statistics, edit/delete, auth, cloud, or AI scope was introduced - dbbee92

### Phase 3: Endpoint Integration Tests

#### Automated

- [x] 3.1 Test project restores - f47bd44
- [x] 3.2 Test project builds - f47bd44
- [x] 3.3 Endpoint integration tests pass - f47bd44
- [x] 3.4 API project still builds - f47bd44

#### Manual

- [x] 3.5 Tests exercise HTTP endpoints, not only EF persistence - f47bd44
- [x] 3.6 Tests do not require real user finance data or external services - f47bd44

### Phase 4: Verification And Documentation Touch-Up

#### Automated

- [x] 4.1 Restore succeeds - 98cee34
- [x] 4.2 API build succeeds - 98cee34
- [x] 4.3 Test suite succeeds - 98cee34
- [x] 4.4 Vulnerability audit succeeds - 98cee34

#### Manual

- [x] 4.5 MVP remains local-first and backend-only for this change - 98cee34
- [x] 4.6 API contract covers category list, transaction creation, and transaction history - 98cee34
- [x] 4.7 No out-of-scope features slipped in - 98cee34
