# Internal Research: monthly-period-navigation

## Existing patterns

- `FinanceEndpoints` maps all finance handlers and already scopes transactions to `FinanceDbContext.DefaultLocalProfileId`.
- `GetTransactions` currently accepts only an optional `limit` and orders one global list by date.
- `GetDashboardSummary` aggregates the full default-profile transaction set and exposes expense categories plus existing goal progress.
- `GenerateCurrentMonthRecurringTransactions` already contains the recurring generation and duplicate guard. It can accept a requested period through the same handler without duplicating recurring logic.
- `CreateTransactionRequest` already carries `TransactionDate`; the frontend can constrain the existing form to the selected month instead of changing the transaction entity.
- Frontend `App.tsx` owns the single refresh flow and currently loads transactions, dashboard, goals, goal forecast and recurring definitions together.
- The frontend has no router. Existing sidebar anchors can be replaced later by section state in `dashboard-layout`.
- Tests use SQLite with deterministic `DateOnly` values and the existing `FinanceApiFactory`.

## Decisions

- Add `GET /api/transactions?year=&month=` while preserving the existing limit validation and default behavior when no period is supplied.
- Add `GET /api/dashboard/monthly-summary?year=&month=` returning selected year/month, monthly income, monthly expenses, monthly balance, expense categories and income categories.
- Extend the existing recurring generation endpoint with optional `year` and `month` query parameters. Missing parameters mean the current month; supplied parameters target the selected month. No second recurring endpoint or scheduler.
- Keep goals and goal forecast reads unchanged and load them alongside the selected-month data.
- The existing transaction date input is constrained to the selected month and defaults to its first day. The backend remains a general transaction API and does not introduce a second transaction model.
