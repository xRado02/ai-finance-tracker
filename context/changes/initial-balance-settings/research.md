# Internal Research: initial-balance-settings

## Existing patterns

- `LocalProfile` is the existing owner for all local finance data and is seeded with `DefaultLocalProfileId`.
- `FinanceDbContext` configures profile-owned relations and decimal precision for finance amounts.
- `FinanceEndpoints` already computes global dashboard balance and goal current amount from default-profile transactions.
- Goals and goal forecast are existing read models; their balance helper can be adjusted to include profile initial balance without duplicating their logic.
- Frontend `App.tsx` owns all refreshes and can load a settings read model alongside existing data.

## Decisions

- Add `InitialBalance` as a required decimal column with default `0.00` to `LocalProfile`.
- Add `GET /api/profile/settings` and `PATCH /api/profile/settings` for the default local profile.
- Allow a negative initial balance because debt/overdraft is a valid account state; keep decimal precision 18,2.
- `DashboardSummaryResponse.Balance` becomes the total balance including InitialBalance, and an explicit `InitialBalance` field is added for clarity.
- Existing goals and forecast use the same total balance semantics. No second goal or forecast implementation is introduced.
