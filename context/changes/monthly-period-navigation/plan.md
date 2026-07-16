---
change_id: monthly-period-navigation
title: Nawigacja po miesiącach i miesięczne podsumowania
status: planned
created: 2026-07-16
updated: 2026-07-16
---

# Plan: Nawigacja po miesiącach i miesięczne podsumowania

## Cel

Umożliwić pracę na wybranym miesiącu: przeglądanie i dodawanie transakcji, generowanie stałych wpisów oraz analizę miesięcznych sum i kategorii.

## Zakres

### W zakresie

- Wspólny wybór `year` i `month` w frontendzie.
- Filtrowanie istniejącej historii transakcji po wybranym miesiącu.
- Dodawanie transakcji z datą ograniczoną do wybranego miesiąca.
- `GET /api/dashboard/monthly-summary?year=&month=` z miesięcznymi przychodami, wydatkami, saldem oraz kategoriami przychodów i wydatków.
- Rozszerzenie istniejącego generowania recurring o opcjonalny wybrany rok i miesiąc.
- Odświeżenie historii i podsumowań po dodaniu, usunięciu lub wygenerowaniu transakcji.
- Polski interfejs i testy API/typecheck/build.

### Poza zakresem

- `InitialBalance` i saldo całkowite; to osobny change `initial-balance-settings`.
- Nowa encja transakcji, nowy CRUD recurring, nowy CRUD goals, nowa logika forecastu.
- Router, auth, AI, cloud, import bankowy, background jobs, realtime i zaawansowane wykresy.

## Kontrakty API

### `GET /api/transactions?year=2026&month=7&limit=50`

Zwraca istniejące `TransactionResponse`, ograniczone do zakresu od pierwszego dnia wybranego miesiąca do pierwszego dnia kolejnego miesiąca. `limit` pozostaje opcjonalny.

### `GET /api/dashboard/monthly-summary?year=2026&month=7`

```json
{
  "year": 2026,
  "month": 7,
  "totalIncome": 5000.00,
  "totalExpenses": 1200.00,
  "balance": 3800.00,
  "expenseCategories": [{ "categoryName": "Food", "amount": 450.00 }],
  "incomeCategories": [{ "categoryName": "Salary", "amount": 5000.00 }]
}
```

### Existing recurring generation

`POST /api/recurring-transactions/generate-current-month?year=2026&month=7` uses the selected period when both query values are supplied and retains current-month behavior when omitted.

## Decyzje techniczne

- Miesiąc jest liczony jako `[monthStart, nextMonthStart)`, więc filtr nie zależy od liczby dni.
- Walidacja okresu wymaga roku 2000-2100 i miesiąca 1-12.
- Miesięczne saldo to wyłącznie `income - expenses` wybranego miesiąca.
- Kategorie są grupowane po nazwie i sortowane malejąco po kwocie.
- Goals i goal forecast nadal liczą się z istniejących danych całkowitych; ich semantyka nie jest zmieniana w tym change.

## Fazy

### Phase 1: Monthly API and recurring period

Dodać kontrakt miesięcznego summary, filtrowanie transakcji, testy zakresu dat i rozszerzyć istniejące generowanie recurring o wybrany okres.

### Phase 2: Period-aware frontend

Dodać wspólny picker miesiąca/roku, podpiąć filtrowaną historię i miesięczne summary, ograniczyć datę formularza oraz przekazać wybrany okres do istniejącego panelu recurring.

### Phase 3: Verification and closeout

Uruchomić build/test/typecheck, sprawdzić odświeżanie mutacji i przygotować manual smoke test.

## Success Criteria

### Automated Verification

- `dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false` przechodzi.
- `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false` przechodzi.
- `npm run typecheck` w `frontend/` przechodzi.
- `npm run build` w `frontend/` przechodzi.

### Manual Verification

- Domyślnie wybrany jest bieżący miesiąc i rok.
- Zmiana miesiąca odświeża historię, sumy i kategorie.
- Dodanie transakcji zapisuje ją w wybranym miesiącu.
- Usunięcie transakcji odświeża wybrany miesiąc.
- Generowanie recurring dla wybranego miesiąca tworzy wpisy w tym okresie i aktualizuje summary.

## Progress

> Convention: `- [ ]` pending, `- [x]` done. Append `- <commit sha>` when a step lands.

### Phase 1: Monthly API and recurring period

- [x] 1.1 Filtrowanie transakcji i walidacja okresu są dodane - 0b0309e
- [x] 1.2 Monthly summary zwraca sumy i kategorie dla wybranego miesiąca - 0b0309e
- [x] 1.3 Istniejące recurring generation obsługuje wybrany okres - 0b0309e
- [x] 1.4 Testy API pokrywają zakres dat, kategorie, recurring i izolację profilu - 0b0309e

### Phase 2: Period-aware frontend

- [x] 2.1 Picker miesiąca/roku i wspólny stan okresu są dodane
- [x] 2.2 Historia, formularz i miesięczne podsumowania używają wybranego okresu
- [x] 2.3 Istniejący panel recurring generuje i odświeża wskazany miesiąc

### Phase 3: Verification and closeout

- [x] 3.1 Backend build/test oraz frontend typecheck/build przechodzą
- [x] 3.2 Zakres nie duplikuje recurring ani goal-forecast
- [x] 3.3 Manual smoke test jest gotowy do potwierdzenia
