<!-- PLAN-REVIEW-REPORT -->
# Plan Review: Stan początkowy konta

- **Plan**: `context/changes/initial-balance-settings/plan.md`
- **Verdict**: SOUND
- **Findings**: 0 blocking, 0 warnings

## Review

Plan rozszerza istniejący LocalProfile i wspólną logikę salda. Nie tworzy transakcji dla ustawienia, więc InitialBalance nie zanieczyszcza miesięcznych przychodów ani wydatków. Domyślne zero zachowuje kompatybilność obecnych danych.

## Accepted decisions

- Ujemny InitialBalance jest dozwolony.
- `DashboardSummaryResponse.Balance` oznacza saldo całkowite, a monthly summary pozostaje miesięczne.
- Goals i forecast korzystają z istniejącej logiki po zmianie źródła salda.
