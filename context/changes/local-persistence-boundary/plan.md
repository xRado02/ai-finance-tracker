# Local Persistence Boundary Implementation Plan

## Overview

This change adds the minimal local persistence foundation required before the first transaction flow can be built. It introduces EF Core with SQL Server, a default local profile, categories, transactions, an initial migration, deterministic seed data, and persistence smoke tests.

This is a foundation change, not a user-facing finance feature. It deliberately does not add endpoints, frontend UI, dashboard summaries, goals, statistics, currency handling, recurring transactions, cloud deployment, or AI features.

## Current State Analysis

The project is a generated ASP.NET Core Web API scaffold targeting .NET 9.0. The only runtime endpoint is the generated weather sample in `Program.cs`, and the project currently references only `Microsoft.AspNetCore.OpenApi`.

There is no persistence layer yet: no EF Core packages, no SQL Server provider, no `DbContext`, no domain entities, no migrations, no seed data, and no test project. The PRD and roadmap require local-first storage and a default local profile before transaction entry can be planned safely.

## Desired End State

After this plan is implemented, the application has a minimal local persistence boundary that can store one seeded default local profile, seeded categories, and transactions tied to that profile and category. The app is still local-first and has no cloud dependency.

The change is verified by restore/build/test/audit commands and persistence smoke tests that prove model configuration, default seed data, and transaction save/read behavior. The weather sample may remain until the next user-facing transaction slice replaces it with finance endpoints.

### Key Discoveries:

- `Program.cs` currently contains only the generated OpenAPI setup and `/weatherforecast` sample endpoint.
- `ai-finance-tracker.csproj` targets `net9.0`, has nullable reference types enabled, implicit usings enabled, root namespace `AiFinanceTracker`, and only `Microsoft.AspNetCore.OpenApi`.
- `appsettings.Development.json` exists and is the right local place for a Development connection string.
- `context/foundation/roadmap.md` marks `local-persistence-boundary` as `F-01`, ready, and required before `transaction-entry-history`.
- `context/foundation/prd.md` requires a default local profile, mandatory categories with a fallback category, local-only finance data, and no email/password account system.

## What We're NOT Doing

- No transaction API endpoints.
- No React frontend or UI.
- No dashboard, summaries, statistics, or charts.
- No financial goals.
- No currency, recurring transaction, import, bank integration, notification, reminder, or AI fields.
- No cloud deployment, Azure resource creation, production connection string, or environment-variable-only configuration.
- No removal of the generated weather sample in this change.
- No business-logic tests or endpoint tests yet; this change only verifies persistence.

## Implementation Approach

Add the smallest backend persistence foundation that lets later vertical slices work against real local data. Keep domain types focused on the first transaction flow: one deterministic local profile, seeded categories including `Other`, and transactions with amount, type, date, optional description, category, and local profile.

Use EF Core with the SQL Server provider because the stack hand-off names SQL Server as the intended database. Configure the app for local Development via `appsettings.Development.json`, and add a separate test project for persistence smoke tests.

## Critical Implementation Details

The seed data must be deterministic. The default local profile and seeded categories should use stable identifiers so migrations, tests, and later application code can reliably refer to the same local records without creating duplicates.

The `Other` category is mandatory. Later transaction entry can use it as the fallback category when the user does not choose a more specific one.

## Phase 1: Persistence Model And Configuration

### Overview

Add EF Core/SQL Server dependencies, the minimal domain model, the application `DbContext`, Development connection string, and service registration.

### Changes Required:

#### 1. Project dependencies

**File**: `ai-finance-tracker.csproj`

**Intent**: Add the EF Core packages needed for SQL Server persistence and migrations. This gives the API project the runtime and tooling contracts needed for the local database foundation.

**Contract**: Add package references for EF Core, SQL Server provider, and EF Core design-time tooling compatible with the current .NET target and project package style.

#### 2. Domain model

**File**: new files under an `AiFinanceTracker` feature-oriented backend structure

**Intent**: Define the minimal persistence entities needed by the first transaction slice while keeping goals and dashboard concepts out of this foundation.

**Contract**: Add:

- `LocalProfile` with deterministic identity and display/name metadata sufficient for one default local user.
- `Category` with identity, name, transaction-kind applicability if needed by implementation, and relationship to transactions.
- `Transaction` with `Amount`, `Type`, `TransactionDate`, optional `Description`, `CategoryId`, and `LocalProfileId`.
- `TransactionType` enum with `Income` and `Expense`.

#### 3. EF Core context and model configuration

**File**: new application persistence context file

**Intent**: Centralize EF Core sets, relationships, required fields, decimal precision, and seed data hooks in one persistence boundary.

**Contract**: Add a `DbContext` in the `AiFinanceTracker` namespace with `DbSet` properties for local profiles, categories, and transactions. Configure required relationships from transaction to category and local profile. Configure amount as a decimal with explicit precision.

#### 4. Development connection string

**File**: `appsettings.Development.json`

**Intent**: Keep the MVP local-first and easy to run by providing a Development-only SQL Server connection string.

**Contract**: Add a `ConnectionStrings` entry for the local finance database. It should target a local SQL Server instance such as LocalDB or SQL Server Express, not Azure SQL or any cloud resource.

#### 5. Service registration

**File**: `Program.cs`

**Intent**: Register the application `DbContext` with SQL Server so future slices can use persistence through dependency injection.

**Contract**: Add the EF Core service registration without adding finance endpoints and without removing the weather sample in this change.

### Success Criteria:

#### Automated Verification:

- Package restore succeeds: `dotnet restore .\ai-finance-tracker.csproj`
- API project builds: `dotnet build .\ai-finance-tracker.csproj --no-restore`

#### Manual Verification:

- Confirm `appsettings.Development.json` points only to a local SQL Server instance.
- Confirm no finance endpoints or UI were added in this phase.

---

## Phase 2: Initial Migration And Seed Contract

### Overview

Create the initial EF Core migration and deterministic seed data for the default local profile and startup categories.

### Changes Required:

#### 1. Initial migration

**File**: new EF Core migration files

**Intent**: Capture the local persistence schema in a migration so the database can be created and evolved consistently.

**Contract**: Add an initial migration that creates tables for local profiles, categories, and transactions with the relationships and constraints from Phase 1.

#### 2. Default profile seed

**File**: persistence model configuration / seed configuration

**Intent**: Ensure the MVP always has exactly one default local profile available without requiring account management or UI.

**Contract**: Seed one default local profile with a deterministic identifier. The seed must not create multiple profiles across repeated migration/application cycles.

#### 3. Category seed

**File**: persistence model configuration / seed configuration

**Intent**: Provide enough categories for transaction entry and category analysis while keeping category management out of scope for this foundation.

**Contract**: Seed these initial categories with deterministic identifiers: `Other`, `Food`, `Transport`, `Housing`, `Bills`, `Entertainment`, `Health`, `Salary`, and `Other Income`. `Other` must always exist as the fallback category.

### Success Criteria:

#### Automated Verification:

- Initial migration exists and is included in the API project.
- Migration can be applied to a local Development database using the documented EF Core command.
- API project builds after migration files are added: `dotnet build .\ai-finance-tracker.csproj --no-restore`

#### Manual Verification:

- Inspect the migration and confirm it contains only local profile, category, and transaction persistence.
- Confirm no goals, dashboard summary tables, statistics tables, currency fields, or recurring transaction fields were introduced.

---

## Phase 3: Persistence Smoke Tests

### Overview

Add a dedicated test project and persistence smoke tests for the model, seed data, and transaction save/read behavior.

### Changes Required:

#### 1. Test project

**File**: new test project under a dedicated test directory

**Intent**: Keep tests separate from the API project, matching the repository guideline that tests should not be mixed into the application project.

**Contract**: Create a dedicated test project referencing the API project and the test packages needed to run .NET tests. Document the exact test command in the plan and, if implementation updates repository guidance, in the appropriate docs.

#### 2. Model configuration tests

**File**: new persistence test file

**Intent**: Verify the EF Core model has the expected entities, required fields, relationships, and decimal amount configuration before user-facing slices depend on it.

**Contract**: Tests should assert the configured model contains local profiles, categories, and transactions, with transaction relationships to profile and category.

#### 3. Seed tests

**File**: new persistence test file

**Intent**: Prove the default local profile and seeded categories exist and are deterministic.

**Contract**: Tests should verify one default local profile is available, `Other` exists, and the agreed category names are present.

#### 4. Transaction persistence smoke test

**File**: new persistence test file

**Intent**: Prove the foundation can save and read a transaction tied to the default profile and a category.

**Contract**: Test should create or use the seeded profile/category, save an income or expense transaction with amount, type, transaction date, optional description, category id, and local profile id, then read it back with the expected values.

### Success Criteria:

#### Automated Verification:

- Test project restores successfully.
- Test project builds successfully.
- Persistence smoke tests pass with `dotnet test`.
- API project still builds after test project addition.

#### Manual Verification:

- Confirm tests do not call finance API endpoints because none exist yet.
- Confirm tests do not require Azure, cloud services, AI services, or external finance data.

---

## Phase 4: Verification And Documentation Touch-Up

### Overview

Run the full verification loop and update project-facing documentation where this change introduces new commands.

### Changes Required:

#### 1. Verification commands

**File**: no code file; terminal verification

**Intent**: Prove the planned persistence foundation is healthy before downstream transaction work starts.

**Contract**: Run restore, build, test, and NuGet vulnerability audit commands from the repository root.

#### 2. Repository guidance update

**File**: `AGENTS.md`

**Intent**: Keep the repo guidance accurate now that a test project and test command exist.

**Contract**: Update the testing guideline to name the dedicated test project and exact test command. Do not alter product-scope hard rules.

#### 3. Change status remains implementation-owned

**File**: `context/changes/local-persistence-boundary/change.md`

**Intent**: Leave execution state for `/10x-implement` and later review tools.

**Contract**: This plan sets the change to `planned`; implementation phases will move progress through the `## Progress` checklist.

### Success Criteria:

#### Automated Verification:

- Restore succeeds: `dotnet restore .\ai-finance-tracker.csproj`
- Build succeeds: `dotnet build .\ai-finance-tracker.csproj --no-restore`
- Tests pass with the exact test command added by this change.
- Vulnerability audit succeeds: `dotnet list .\ai-finance-tracker.csproj package --vulnerable --include-transitive`

#### Manual Verification:

- Confirm documentation still states the MVP is local-first.
- Confirm no cloud deployment instructions became mandatory.
- Confirm no AI, auth, goals, dashboard, or endpoint scope slipped into the implementation.

---

## Testing Strategy

### Unit Tests:

- EF Core model configuration for the three persistence entities.
- Deterministic default local profile seed.
- Deterministic startup category seed, including mandatory `Other`.

### Integration Tests:

- Persistence smoke test that writes and reads one transaction through EF Core using the configured model.
- Migration application against a local/test database provider selected during implementation.

### Manual Testing Steps:

1. Review the local Development connection string and confirm it targets only local SQL Server.
2. Apply the initial migration to a local database.
3. Inspect seeded data and confirm one default profile and the agreed categories exist.
4. Confirm the weather sample still exists and no finance endpoints were added.

## Performance Considerations

The expected MVP scale is small. The important performance choice in this foundation is correctness of relational constraints and decimal amount precision rather than caching or query optimization. Later dashboard work can introduce query-level optimization when it has real read paths.

## Migration Notes

This change creates the first migration for a greenfield project, so there is no existing production data to migrate. If a developer has a local scratch database from experimentation, they may need to drop/recreate it manually before applying the first official migration.

## References

- Roadmap item: `context/foundation/roadmap.md` (`F-01 local-persistence-boundary`)
- Product contract: `context/foundation/prd.md`
- Stack hand-off: `context/foundation/tech-stack.md`
- Repository guidelines: `AGENTS.md`
- API scaffold: `Program.cs`
- Project file: `ai-finance-tracker.csproj`

## Progress

> Convention: `- [ ]` pending, `- [x]` done. Append `— <commit sha>` when a step lands. Do not rename step titles. See `references/progress-format.md`.

### Phase 1: Persistence Model And Configuration

#### Automated

- [x] 1.1 Package restore succeeds
- [x] 1.2 API project builds

#### Manual

- [x] 1.3 Development connection string is local-only
- [x] 1.4 No finance endpoints or UI were added

### Phase 2: Initial Migration And Seed Contract

#### Automated

- [ ] 2.1 Initial migration exists
- [ ] 2.2 Migration applies to a local Development database
- [ ] 2.3 API project builds after migration

#### Manual

- [ ] 2.4 Migration scope excludes goals, dashboard, statistics, currency, and recurring fields

### Phase 3: Persistence Smoke Tests

#### Automated

- [ ] 3.1 Test project restores
- [ ] 3.2 Test project builds
- [ ] 3.3 Persistence smoke tests pass
- [ ] 3.4 API project still builds

#### Manual

- [ ] 3.5 Tests do not depend on endpoints, cloud services, AI services, or external finance data

### Phase 4: Verification And Documentation Touch-Up

#### Automated

- [ ] 4.1 Restore succeeds
- [ ] 4.2 Build succeeds
- [ ] 4.3 Test command succeeds
- [ ] 4.4 Vulnerability audit succeeds

#### Manual

- [ ] 4.5 Documentation preserves local-first MVP scope
- [ ] 4.6 No out-of-scope features slipped into implementation
