# Local Persistence Boundary - Plan Brief

> Full plan: `context/changes/local-persistence-boundary/plan.md`

## What & Why

This change creates the minimal local persistence foundation for AI Finance Tracker. It must exist before the first transaction slice can safely store income, expenses, categories, and the default local profile without drifting away from the local-first MVP contract.

## Starting Point

The codebase is still the generated ASP.NET Core Web API scaffold with a weather sample endpoint. There is no EF Core setup, SQL Server provider, `DbContext`, migration, seed data, or test project.

## Desired End State

The API project has EF Core + SQL Server configured for local Development, a minimal model for default local profile, categories, and transactions, an initial migration, deterministic seed data, and persistence smoke tests. The app still has no finance endpoints or UI in this change.

## Key Decisions Made

| Decision | Choice | Why |
|---|---|---|
| Foundation scope | Profile, categories, transactions, EF Core/SQL Server, migration, seed | Unlocks transaction entry without prebuilding goals or dashboard. |
| Local profile | One deterministic seeded default profile | Matches single-user MVP and avoids account management. |
| Categories | Small seed with mandatory `Other` | Keeps transaction entry unblocked while avoiding large domain design now. |
| Transaction model | Amount, type, date, optional description, category, profile | Supports the first transaction flow without currency or recurrence scope. |
| Configuration | Development connection string in `appsettings.Development.json` | Keeps MVP local-first and simple to run with local SQL Server. |
| Tests | Dedicated test project with persistence smoke tests | Protects the data foundation before user-facing slices depend on it. |
| API/UI scope | No endpoints, no UI; weather sample stays | Keeps F-01 as a small foundation, not a user-facing feature. |

## Scope

**In scope:**

- EF Core + SQL Server dependencies.
- Local persistence entities for profile, categories, and transactions.
- Application `DbContext` and local Development connection string.
- Initial migration.
- Seed for one default profile and categories: `Other`, `Food`, `Transport`, `Housing`, `Bills`, `Entertainment`, `Health`, `Salary`, `Other Income`.
- Dedicated test project and persistence smoke tests.
- Documentation update for the test command if implementation adds one.

**Out of scope:**

- Transaction endpoints.
- React frontend or UI.
- Goals, dashboard, statistics, charts, currency, recurring transactions.
- Cloud deployment or Azure resources.
- AI, auth, bank integrations, import, notifications, reminders.
- Removing the weather sample.

## Architecture / Approach

Add a small persistence boundary inside the ASP.NET Core backend: domain entities plus EF Core model configuration, local SQL Server connection in Development, deterministic seed data, and tests that verify the model and basic save/read behavior. Later slices consume this boundary instead of inventing their own storage shape.

## Phases at a Glance

| Phase | What it delivers | Key risk |
|---|---|---|
| 1. Persistence Model And Configuration | EF Core dependencies, model, context, local config | Accidentally adding too much domain scope. |
| 2. Initial Migration And Seed Contract | First migration and deterministic seed data | Seed IDs or categories drift across runs. |
| 3. Persistence Smoke Tests | Separate test project and persistence tests | Tests become too broad and start testing endpoints. |
| 4. Verification And Documentation Touch-Up | Restore/build/test/audit and docs update | Documentation implies cloud or out-of-scope setup. |

**Prerequisites:** Existing ASP.NET Core scaffold, roadmap item `F-01`, local SQL Server/LocalDB or SQL Server Express for manual migration verification.

**Estimated effort:** One focused foundation change across four implementation phases.

## Open Risks & Assumptions

- Assumes local SQL Server or LocalDB/SQL Server Express is available for manual migration verification.
- EF Core package versions should match the .NET 9 project line during implementation.
- Test provider choice must still prove relational persistence behavior enough for this foundation.

## Success Criteria (Summary)

- Local profile, category, and transaction persistence model exists and builds.
- Initial migration and deterministic seed data exist, including mandatory `Other`.
- Persistence smoke tests pass in a dedicated test project.
- No finance endpoints, UI, cloud setup, AI, goals, dashboard, or statistics were added.
