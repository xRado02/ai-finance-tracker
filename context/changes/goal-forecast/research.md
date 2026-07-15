# Internal Research: goal-forecast

## Existing patterns

- `Goal` is already persisted and scoped to `FinanceDbContext.DefaultLocalProfileId`.
- `GET /api/goals` and `GET /api/dashboard/summary` calculate current goal progress from `max(income - expenses, 0)`.
- `GoalList.tsx` is the existing single place rendering goal progress; `App.tsx` owns the refresh and can load a second read model.
- The frontend has no chart dependency. Existing progress bars are CSS-only and the current UI is already a dark full-screen workspace.
- API tests use deterministic `DateOnly` transaction dates and SQLite, so monthly forecast cases can be tested without external services.

## Decisions

- Add `GET /api/goals/forecast` as a read-only projection for existing goals; do not create a second Goal entity or mutate `/api/goals` CRUD.
- Calculate average monthly surplus from calendar months containing profile transactions; no transactions means no data.
- Return an explicit status (`Forecastable`, `Achieved`, `NoData`, `NoPositiveSurplus`) plus nullable months/date and amounts, so the UI does not infer edge cases from magic numbers.
- Estimate months with `ceil(remaining / averageMonthlySurplus)` and date from the current local date plus that number of months.
- Render a compact CSS/SVG trend visualization next to the existing progress bar.
