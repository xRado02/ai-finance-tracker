---
change_id: initial-balance-settings
title: Stan początkowy konta
status: planned
created: 2026-07-16
updated: 2026-07-16
---

# Plan: Stan początkowy konta

## Cel

Pozwolić ustawić stan konta przed pierwszą zapisaną transakcją i jasno liczyć saldo całkowite jako stan początkowy plus pełna historia.

## Zakres

### W zakresie

- `LocalProfile.InitialBalance` i migracja EF Core.
- `GET/PATCH /api/profile/settings` dla default local profile.
- Globalne saldo dashboardu uwzględniające InitialBalance.
- Istniejące goals/goal-forecast korzystają z poprawionej wspólnej wartości salda.
- Polski panel ustawień z edycją kwoty.
- Testy API oraz frontend typecheck/build.

### Poza zakresem

- Nowy profil, auth, historia zmian ustawienia, import bankowy, recurring, nowy forecast, cloud i router.

## Kontrakty API

`GET /api/profile/settings`:

```json
{ "displayName": "Default Local Profile", "initialBalance": 1000.00 }
```

`PATCH /api/profile/settings` przyjmuje `{ "initialBalance": 1000.00 }` i zwraca zaktualizowane ustawienia.

`DashboardSummaryResponse` zawiera jawne `initialBalance`, a `balance` oznacza saldo całkowite.

## Decyzje techniczne

- Wartość domyślna migracji to `0.00`, więc obecne dane zachowują dotychczasowe wyniki.
- Saldo całkowite to `InitialBalance + suma Income - suma Expense`.
- Saldo miesięczne z `monthly-period-navigation` pozostaje bez InitialBalance.
- Ujemna wartość InitialBalance jest dozwolona.

## Fazy

### Phase 1: Profile persistence and API

Dodać pole, migrację, settings endpoint oraz podpiąć InitialBalance do globalnego salda, goals i forecastu.

### Phase 2: Settings UI

Dodać polski formularz ustawienia i odświeżanie danych po zapisie.

### Phase 3: Verification and closeout

Uruchomić backend/frontend checks, sprawdzić migrację i zakres.

## Success Criteria

### Automated Verification

- `dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false` przechodzi.
- `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false` przechodzi.
- `npm run typecheck` w `frontend/` przechodzi.
- `npm run build` w `frontend/` przechodzi.

### Manual Verification

- Można ustawić i zmienić InitialBalance po polsku.
- InitialBalance nie pojawia się jako przychód.
- Saldo całkowite zmienia się zgodnie z formułą, a saldo miesięczne pozostaje miesięczne.
- Istniejące goals/forecast uwzględniają saldo całkowite.

## Progress

> Convention: `- [ ]` pending, `- [x]` done. Append `- <commit sha>` when a step lands.

### Phase 1: Profile persistence and API

- [x] 1.1 LocalProfile, konfiguracja i migracja zawierają InitialBalance
- [x] 1.2 Settings GET/PATCH działają dla default profile
- [x] 1.3 Globalne saldo, goals i forecast uwzględniają InitialBalance
- [x] 1.4 Testy API pokrywają default 0, zmianę ustawienia i formułę salda

### Phase 2: Settings UI

- [ ] 2.1 API client, App state i polski panel ustawień są dodane
- [ ] 2.2 Zapis ustawienia odświeża dashboard i istniejące dane

### Phase 3: Verification and closeout

- [ ] 3.1 Backend build/test oraz frontend typecheck/build przechodzą
- [ ] 3.2 Miesięczne i całkowite saldo są rozdzielone
- [ ] 3.3 Manual smoke test jest gotowy do potwierdzenia
