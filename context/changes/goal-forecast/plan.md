---
change_id: goal-forecast
title: Prognoza osiągnięcia celu
status: planned
created: 2026-07-15
updated: 2026-07-15
---

# Plan: Prognoza osiągnięcia celu

## Cel

Pokazać dla każdego istniejącego celu brakującą kwotę, historyczną średnią miesięczną nadwyżkę i prosty szacunek terminu osiągnięcia.

## Zakres

### W zakresie

- Read-only `GET /api/goals/forecast` dla istniejących goals default profile.
- Remaining amount, average monthly surplus, estimated months, estimated date and forecast status.
- Obsługa celu osiągniętego, braku transakcji i nadwyżki miesięcznej `<= 0`.
- Integracja z istniejącą listą goals, bez duplikowania CRUD.
- Polski komunikat dla każdego statusu.
- Lekka wizualizacja CSS/SVG trendu, bez ciężkiej biblioteki.
- Testy API oraz frontend typecheck/build.

### Poza zakresem

- AI forecasting, predykcje sezonowe, budżety, background jobs, auth, cloud, import bankowy, wielu użytkowników i zaawansowane wykresy.

## Kontrakt API

`GET /api/goals/forecast` zwraca listę:

```json
{
  "goalId": "...",
  "name": "Poduszka finansowa",
  "targetAmount": 10000.00,
  "currentAmount": 2500.00,
  "remainingAmount": 7500.00,
  "averageMonthlySurplus": 1250.00,
  "estimatedMonths": 6,
  "estimatedDate": "2027-01-15",
  "status": "Forecastable"
}
```

Statusy: `Forecastable`, `Achieved`, `NoData`, `NoPositiveSurplus`.

## Decyzje techniczne

- Średnia miesięczna nadwyżka to średnia z `income - expenses` dla kalendarzowych miesięcy zawierających co najmniej jedną transakcję.
- Brak transakcji daje `NoData`; nadwyżka `<= 0` daje `NoPositiveSurplus`.
- Dla celu osiągniętego `remainingAmount = 0`, a termin i miesiące są puste.
- `estimatedMonths = ceil(remainingAmount / averageMonthlySurplus)`; data to dzisiaj plus ta liczba miesięcy.
- UI używa istniejącego progressu celu oraz dodatkowej małej wizualizacji trendu.

## Fazy

### Phase 1: Forecast API

Dodać kontrakt, statusy, agregację miesięcy, endpoint i testy przypadków brzegowych.

### Phase 2: Forecast UI

Dodać API client, App state, polskie komunikaty, brakującą kwotę i prosty CSS/SVG trend przy istniejącym celu.

### Phase 3: Verification and closeout

Uruchomić build/test/typecheck, sprawdzić brak duplikacji Goals i przygotować manual smoke test.

## Success Criteria

### Automated Verification

- `dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false` przechodzi.
- `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false` przechodzi.
- `npm run typecheck` w `frontend/` przechodzi.
- `npm run build` w `frontend/` przechodzi.

### Manual Verification

- Cel forecastable pokazuje komunikat z liczbą miesięcy i datą.
- Cel osiągnięty pokazuje `Cel osiągnięty`.
- Brak danych i brak dodatniej nadwyżki pokazują właściwe komunikaty.
- Wizualizacja trendu jest lekka, czytelna i nie wymaga dodatkowej biblioteki.

## Progress

> Convention: `- [ ]` pending, `- [x]` done. Append `- <commit sha>` when a step lands.

### Phase 1: Forecast API

- [x] 1.1 Kontrakt i statusy forecastu są dodane
- [x] 1.2 Endpoint liczy miesięczną nadwyżkę i termin dla existing goals
- [x] 1.3 Testy API pokrywają forecast, achieved, no data i non-positive surplus

### Phase 2: Forecast UI

- [x] 2.1 API client i App state są zintegrowane z istniejącymi goals - 9e3ec3b
- [x] 2.2 Komunikaty forecastu i remaining amount są po polsku - 9e3ec3b
- [x] 2.3 CSS/SVG trend jest widoczny bez biblioteki wykresów - 9e3ec3b

### Phase 3: Verification and closeout

- [x] 3.1 Backend build/test oraz frontend typecheck/build przechodzą
- [x] 3.2 Brak duplikacji Goals i scope creep jest sprawdzony
- [x] 3.3 Manual smoke test jest gotowy do potwierdzenia
