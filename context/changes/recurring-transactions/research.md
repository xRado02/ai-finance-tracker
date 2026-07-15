# Internal Research: recurring-transactions

## Existing patterns

- `FinanceDbContext.DefaultLocalProfileId` scopes all current MVP data to the default local profile.
- `Transaction` owns amount, type, date, optional description, category and profile relations.
- `FinanceEndpoints` contains the existing typed minimal API handlers and validates category/type compatibility.
- `FinanceContracts` exposes records rather than EF entities.
- Tests use an in-memory SQLite connection through `FinanceApiFactory` and verify endpoint behavior plus profile isolation.
- Frontend refresh is centralized in `App.tsx`; feature components call API helpers and report Polish messages locally.

## Decisions

- Add `RecurringTransaction` with amount, type, category, description, `IsActive` and default-profile relation.
- Add nullable `Transaction.RecurringTransactionId` so generated rows can be detected without relying on matching mutable text or amount.
- Use `POST /api/recurring-transactions/generate-current-month`; it creates one transaction on the first day of the current month for every active definition not already generated in that month.
- Add a small status endpoint for changing active/inactive state because a list-only status cannot be managed after creation.
- Keep generation synchronous and user-triggered; no scheduler, queue or hosted service.
