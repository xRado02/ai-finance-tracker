---
change_id: dashboard-summary
title: Podsumowanie finansow i dashboard MVP
status: draft
created: 2026-07-15
updated: 2026-07-15
---

# Plan: Podsumowanie finansow i dashboard MVP

## Cel

Dodac stabilny kontrakt podsumowania finansowego i prosty dashboard, zeby uzytkownik od razu widzial przychody, wydatki, saldo, najwieksze kategorie wydatkow oraz postep celow.

## Stan obecny

- Transakcje i cele sa zapisane dla `FinanceDbContext.DefaultLocalProfileId`.
- `GET /api/goals` juz liczy progress celu z salda.
- Frontend ma jeden ekran roboczy z formularzami, historia i lista celow.
- CSS nie korzysta z biblioteki komponentow ani wykresow.

## Zakres

### W zakresie

- `GET /api/dashboard/summary`.
- Typed response dla metryk, kategorii wydatkow i celow.
- Agregacja tylko dla default local profile.
- Suma przychodow, suma wydatkow i saldo.
- Lista kategorii wydatkow posortowana malejaco po kwocie.
- Lista celow z progress, z tym samym znaczeniem co `/api/goals`.
- Polski dashboard na frontendzie z metrykami, lista kategorii i progress celow.
- Testy API oraz frontend typecheck/build.

### Poza zakresem

- Zaawansowane wykresy, filtrowanie po datach, budzety, rekomendacje, AI, auth, cloud, import bankowy, custom kategorie, realtime i notyfikacje.

## Kontrakt API

`GET /api/dashboard/summary`:

```json
{
  "totalIncome": 5000.00,
  "totalExpenses": 1200.00,
  "balance": 3800.00,
  "expenseCategories": [
    { "categoryName": "Food", "amount": 450.00 }
  ],
  "goals": [
    {
      "id": "...",
      "name": "Emergency fund",
      "targetAmount": 10000.00,
      "currentAmount": 3800.00,
      "progressPercentage": 38.0
    }
  ]
}
```

## Decyzje techniczne

- Endpoint agreguje transakcje default profilu bez limitu historii; małe lokalne wolumeny MVP nie wymagają paginacji.
- Kategorie wydatkow sa grupowane po `Category.Name` i sortowane malejaco.
- `balance = totalIncome - totalExpenses`; goal `currentAmount` korzysta z `max(0, balance)` i progressu 0-100.
- Frontend pobiera dashboard podczas głównego refreshu, więc dodanie/usunięcie transakcji odświeża również metryki.
- Dashboard używa prostych metryk i list, bez biblioteki chartów.

## Fazy

### Phase 1: Dashboard summary API

Dodac kontrakty, agregacje endpointu i testy dla metryk, kategorii, celów oraz izolacji profilu.

### Phase 2: Dashboard UI integration

Dodac client API, stan App, polski komponent metryk/kategorii/celow i CSS responsywny.

### Phase 3: Verification and change closeout

Uruchomic backend/frontend verification, sprawdzic zakres i zapisac Progress, SHA oraz status change'a.

## Success Criteria

### Automated Verification

- `dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false` przechodzi.
- `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false` przechodzi.
- `npm run typecheck` w `frontend/` przechodzi.
- `npm run build` w `frontend/` przechodzi.

### Manual Verification

- Dashboard pokazuje przychody, wydatki i saldo po polsku.
- Największe kategorie wydatków są widoczne malejąco.
- Postęp celów jest widoczny na dashboardzie.
- Dodanie/usunięcie transakcji odświeża dashboard.
- UI nie zawiera zaawansowanych wykresów ani funkcji spoza MVP.

## Testing Strategy

- Backend testy endpointu podsumowania w istniejącym xUnit project.
- Frontend typecheck/build i reczny smoke test.
- Bez nowego projektu i bez biblioteki wykresów.

## References

- `context/foundation/prd.md` FR-006, FR-007, FR-008
- `context/foundation/roadmap.md` S-02 Dashboard Basic Summary
- `context/changes/financial-goals/plan.md`
- `context/changes/financial-goals-ui/plan.md`
- `Endpoints/FinanceEndpoints.cs`
- `Contracts/FinanceContracts.cs`
- `frontend/src/App.tsx`
- `frontend/src/App.css`

## Progress

> Convention: `- [ ]` pending, `- [x]` done. Append `- <commit sha>` when a step lands. Do not rename step titles.

### Phase 1: Dashboard summary API

#### Automated

- [x] 1.1 Endpoint zwraca metryki przychodow, wydatkow i salda - 7b74e97
- [x] 1.2 Endpoint zwraca kategorie wydatkow i progress celow - 7b74e97
- [x] 1.3 Testy API pokrywaja agregacje i izolacje profilu - 7b74e97

#### Manual

- [x] 1.4 Dashboard contract korzysta tylko z default local profile - 7b74e97

### Phase 2: Dashboard UI integration

#### Automated

- [x] 2.1 `npm run typecheck` przechodzi
- [x] 2.2 `npm run build` przechodzi

#### Manual

- [x] 2.3 Metryki i listy dashboardu sa po polsku
- [x] 2.4 Refresh transakcji aktualizuje dashboard
- [x] 2.5 UI nie zawiera zaawansowanych wykresow ani scope creep

### Phase 3: Verification and change closeout

#### Automated

- [ ] 3.1 Backend build przechodzi
- [ ] 3.2 Backend test suite przechodzi
- [ ] 3.3 Frontend typecheck przechodzi
- [ ] 3.4 Frontend build przechodzi

#### Manual

- [ ] 3.5 Reczny smoke test dashboardu przechodzi
- [ ] 3.6 Dokumentacja zmiany i Progress sa kompletne
