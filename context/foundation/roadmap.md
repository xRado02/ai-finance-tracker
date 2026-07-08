---
project: "AI Finance Tracker"
version: 1
status: draft
created: 2026-06-14
updated: 2026-06-14
prd_version: 1
main_goal: low-complexity
top_blocker: capacity
---

# Roadmap: AI Finance Tracker

> Derived from `context/foundation/prd.md` (v1) + auto-researched codebase baseline.
> Edit-in-place; archive when superseded.
> Slices below are listed in dependency order. The "At a glance" table is the index.

## Vision recap

AI Finance Tracker ma dać jednej osobie prosty, lokalny sposób kontroli finansów: dodawanie przychodów i wydatków, kategorie, historia, dashboard oraz postęp celu finansowego. Najważniejsza granica produktu to pełna lokalność danych: MVP działa lokalnie i nie wysyła danych finansowych do usług zewnętrznych ani chmury. Roadmapa jest ustawiona pod niską złożoność, bo projekt ma małą skalę, pracę po godzinach i brak AI, auth, realtime oraz obowiązkowego deployu.

## North star

W tym dokumencie north star oznacza pierwszy mały przepływ end-to-end, który pokazuje, że produkt zaczyna realnie działać, zanim dobudujemy resztę funkcji.

**S-01: Użytkownik może dodać przychód lub wydatek z kategorią i zobaczyć historię transakcji** - to pierwszy wybrany przepływ, bo daje lokalny zapis, kategorię i historię bez czekania na pełny dashboard oraz cele finansowe.

## At a glance

| ID | Change ID | Outcome (user can ...) | Prerequisites | PRD refs | Status |
|---|---|---|---|---|---|
| F-01 | local-persistence-boundary | (foundation) local persistence boundary and default local profile contract exist for the first vertical slice | - | FR-001, FR-004, PRD Guardrails: local data | ready |
| S-01 | transaction-entry-history | user can add income or expense with a category and review transaction history | F-01 | US-01, FR-001, FR-002, FR-003, FR-004, FR-005 | proposed |
| S-02 | dashboard-basic-summary | user can see income, expenses, balance, and spending structure from saved transactions | S-01 | US-01, FR-006, FR-007, FR-008 | proposed |
| S-03 | financial-goal-progress | user can create a financial goal and track progress from local finance data | S-01 | US-01, FR-009, FR-010 | proposed |
| S-04 | first-local-finance-overview | user can complete the first local finance overview in one place | S-02, S-03 | US-01, FR-001, FR-002, FR-003, FR-004, FR-005, FR-006, FR-007, FR-008, FR-009, FR-010 | proposed |

## Baseline

What's already in place in the codebase as of `2026-06-14` (auto-researched + user-confirmed).
Foundations below assume these are present and do NOT re-scaffold them.

- **Frontend:** absent - React frontend is planned for the first roadmap milestones, but no frontend project or build tooling exists yet.
- **Backend / API:** partial - ASP.NET Core Web API scaffold exists, but it still exposes only the generated weather sample in `Program.cs`.
- **Data:** absent - SQL Server and EF Core are not implemented yet; there are no migrations, data access layer, or seed data.
- **Auth:** absent by design - the PRD uses a default local profile and explicitly excludes email/password account management from the MVP.
- **Deploy / infra:** partial docs only - `context/foundation/infrastructure.md` and `context/deployment/deploy-plan.md` exist, but no cloud deployment is executed or required for MVP.
- **Observability:** absent - no dedicated observability setup beyond the framework defaults is present.

## Foundations

### F-01: Local Persistence Boundary

- **Outcome:** (foundation) The project has the minimal local data boundary needed for vertical slices to persist a default local profile, categories, transactions, and goals without sending finance data outside the machine.
- **Change ID:** local-persistence-boundary
- **PRD refs:** FR-001, FR-004, PRD Guardrails: local data
- **Unlocks:** S-01, S-02, S-03, S-04; verifies the local-first storage guardrail before user-facing finance features depend on it.
- **Prerequisites:** -
- **Parallel with:** -
- **Blockers:** -
- **Unknowns:** -
- **Risk:** If this is skipped, the first transaction slice may accidentally become an in-memory demo or drift away from the local-first product contract.
- **Status:** ready

## Slices

### S-01: Transaction Entry And History

- **Outcome:** user can use the default local profile, add an income or expense, assign a category including a fallback category, and review transaction history.
- **Change ID:** transaction-entry-history
- **PRD refs:** US-01, FR-001, FR-002, FR-003, FR-004, FR-005
- **Prerequisites:** F-01
- **Parallel with:** -
- **Blockers:** -
- **Unknowns:** -
- **Risk:** This is sequenced first because it is the smallest useful local finance loop; the risk is overbuilding category management before the add-and-review flow works.
- **Status:** proposed

### S-02: Dashboard Basic Summary

- **Outcome:** user can see total income, total expenses, balance, spending structure by category, and highest-spending categories based on saved transactions.
- **Change ID:** dashboard-basic-summary
- **PRD refs:** US-01, FR-006, FR-007, FR-008
- **Prerequisites:** S-01
- **Parallel with:** S-03
- **Blockers:** -
- **Unknowns:** -
- **Risk:** This waits until real transactions exist so the dashboard is not a disconnected demo; the risk is making presentation too fancy before the summary numbers are correct.
- **Status:** proposed

### S-03: Financial Goal Progress

- **Outcome:** user can create a financial goal with a target amount and track progress toward it using local finance data.
- **Change ID:** financial-goal-progress
- **PRD refs:** US-01, FR-009, FR-010
- **Prerequisites:** S-01
- **Parallel with:** S-02
- **Blockers:** -
- **Unknowns:** -
- **Risk:** This is parallel with the dashboard after transactions exist; the risk is unclear progress semantics if it is not tied back to the locally saved balance.
- **Status:** proposed

### S-04: First Local Finance Overview

- **Outcome:** user can complete the first useful local finance overview: add income and expenses, categorize transactions, create a goal, and open one dashboard that shows balance, category structure, largest spending categories, and goal progress.
- **Change ID:** first-local-finance-overview
- **PRD refs:** US-01, FR-001, FR-002, FR-003, FR-004, FR-005, FR-006, FR-007, FR-008, FR-009, FR-010
- **Prerequisites:** S-02, S-03
- **Parallel with:** -
- **Blockers:** -
- **Unknowns:** -
- **Risk:** This is the integration slice that proves the PRD's full first overview; the risk is discovering late that transaction, dashboard, and goal assumptions do not line up.
- **Status:** proposed

## Backlog Handoff

| Roadmap ID | Change ID | Suggested issue title | Ready for `/10x-plan` | Notes |
|---|---|---|---|---|
| F-01 | local-persistence-boundary | Define minimal local persistence boundary | yes | Run `/10x-plan local-persistence-boundary`; this unlocks the first transaction slice. |
| S-01 | transaction-entry-history | Add transaction entry and history | no | Plan after F-01 is done. |
| S-02 | dashboard-basic-summary | Add dashboard summary from local transactions | no | Plan after S-01 is done. |
| S-03 | financial-goal-progress | Add financial goal progress | no | Plan after S-01 is done; can proceed in parallel with S-02 later. |
| S-04 | first-local-finance-overview | Integrate first local finance overview | no | Plan after S-02 and S-03 are done. |

## Open Roadmap Questions

No open roadmap questions recorded. The main product boundary is already explicit: MVP remains local-first, AI is outside MVP, and cloud deployment is future-only.

## Parked

- **AI-based expense analysis** - Why parked: PRD Non-Goals reserve AI features for a future version.
- **AI chat over personal finances** - Why parked: PRD Non-Goals exclude AI chat from MVP.
- **Bank integrations** - Why parked: PRD Non-Goals exclude bank integrations.
- **Automatic transaction import** - Why parked: PRD Non-Goals require manual entry first.
- **Notifications and reminders** - Why parked: PRD Non-Goals exclude them from MVP.
- **Mobile application** - Why parked: PRD Non-Goals focus the MVP on desktop/laptop use.
- **Mandatory cloud deployment or public availability** - Why parked: MVP remains local-first; Azure is only a future educational preview path.
- **Full account system** - Why parked: PRD Access Control uses a default local profile without email/password auth.
- **Shared budgets, comparisons, and family roles** - Why parked: PRD Non-Goals keep the domain single-user for MVP.
- **Savings recommendations** - Why parked: MVP presents largest spending categories but does not advise what to cut.
- **Financial charts** - Why parked: shape notes explicitly removed charts from MVP; they can be added later as presentation improvements.
- **Recurring income or monthly salary handling** - Why parked: shape notes defer it until manual income entry works.

## Done

(Empty on first generation. `/10x-archive` appends an entry here - and flips that item's `Status` to `done` - when a change whose `Change ID` matches the item is archived.)
