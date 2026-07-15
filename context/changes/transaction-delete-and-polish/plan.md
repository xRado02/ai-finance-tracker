---
change_id: transaction-delete-and-polish
title: Usuwanie transakcji i spolszczenie obecnego przeplywu
status: draft
created: 2026-07-15
updated: 2026-07-15
---

# Plan: Usuwanie transakcji i spolszczenie obecnego przeplywu

## Cel

Rozszerzyc pierwszy dzialajacy przeplyw transakcji o bezpieczne usuwanie wpisu oraz spolszczyc obecny ekran roboczy. Zmiana pozostaje lokalna, jedno-profilowa i nie dodaje zadnego nowego obszaru MVP.

## Kontekst i stan obecny

- Backend mapuje `GET /api/categories`, `POST /api/transactions` i `GET /api/transactions` w `Endpoints/FinanceEndpoints.cs`.
- Transakcje maja `LocalProfileId`, a obecny odczyt filtruje `FinanceDbContext.DefaultLocalProfileId`.
- Frontend w `frontend/src/` ma formularz dodawania i historie, ale nie ma operacji usuwania.
- `frontend/src/api/financeApi.ts` jest jedynym miejscem wywolywania API z komponentow.
- Istniejace testy endpointow uzywaja `WebApplicationFactory` z SQLite in-memory i sa w `tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs`.

## Zakres

### W zakresie

- `DELETE /api/transactions/{id}`.
- Usuwanie wyłącznie transakcji default local profile.
- `204 No Content` po usunieciu i `404 Not Found` dla brakujacego albo nie nalezacego do profilu wpisu.
- Testy API dla sukcesu, braku wpisu i ochrony profilu.
- Typed `deleteTransaction` w frontendowym kliencie API.
- Przycisk `Usuń`, potwierdzenie przez `window.confirm` i odswiezenie historii.
- Spolszczenie wszystkich obecnych tekstow widocznych w UI, w tym statusow, formularza, historii, bledow i etykiet kategorii.
- `npm run typecheck`, `npm run build` oraz backend restore/build/test.

### Poza zakresem

- Edycja transakcji, goals, dashboard, auth, AI, chmura, import bankowy, custom kategorie, wykresy, notyfikacje i realtime.
- Zmiana modelu profilu lub migracja bazy.
- Usuwanie kategorii albo danych spoza default local profile.

## Decyzje techniczne

### Backend

- Dodac `api.MapDelete("/transactions/{id:guid}", DeleteTransaction)` obok pozostalych tras transakcji.
- Handler filtruje po `Id` oraz `LocalProfileId == FinanceDbContext.DefaultLocalProfileId` w jednym zapytaniu.
- Brak wyniku zwraca `ProblemDetails` z tytulem `Transaction not found` i statusem 404, zgodnie z obecnym stylem endpointow.
- Sukces usuwa encje i zwraca 204. Nie ma kaskadowego usuwania kategorii ani profilu.

### Frontend

- `deleteTransaction(id)` zostaje w `financeApi.ts` i nie bedzie wywolywane bezposrednio z warstwy UI przez `fetch`.
- Poniewaz sukces DELETE zwraca `204 No Content`, klient doda osobna sciezke requestu bez parsowania `response.json()` albo parametr oczekiwanej odpowiedzi; nie wolno przepuszczac 204 przez obecny helper JSON.
- `TransactionHistory` dostaje callback usuwania oraz stan operacji; po potwierdzeniu komponent wywoluje callback i pokazuje polski komunikat bledu.
- `App` po usunieciu ponownie pobiera historie i zachowuje dotychczasowe kategorie.
- Nazwy seeded categories pozostaja kontraktem backendu, a frontend tlumaczy ich wyswietlane etykiety, z `Other` jako `Inne`.
- Wszystkie teksty widoczne dla uzytkownika sa po polsku; typy API `Income` i `Expense` pozostaja angielskim kontraktem technicznym, ale sa prezentowane jako `Przychod` i `Wydatek`.

## Plan faz

### Phase 1: Backend delete endpoint

Dodac trase, handler i testy API obejmujace 204, 404 oraz izolacje default local profile.

### Phase 2: Frontend delete integration

Dodac typed client, przycisk `Usuń`, potwierdzenie, stan usuwania i odswiezenie historii po sukcesie.

### Phase 3: Polish current transaction UI

Przetlumaczyc teksty App, formularza, historii, statusow, bledow, typow i kategorii oraz dopasowac formatowanie kwot do polskiej lokalizacji.

### Phase 4: Verification and change closeout

Uruchomic automatyczna weryfikacje, wykonac reczny smoke test usuwania oraz zapisac Progress, SHA i status change'a.

## Success Criteria

### Automated Verification

- `dotnet restore .\ai-finance-tracker.csproj` przechodzi.
- `dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false` przechodzi.
- `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false` przechodzi.
- `npm run typecheck` w `frontend/` przechodzi.
- `npm run build` w `frontend/` przechodzi.

### Manual Verification

- Historia pokazuje polskie etykiety i przycisk `Usuń` przy transakcji.
- Potwierdzenie anulowane nie usuwa wpisu.
- Potwierdzenie zaakceptowane usuwa wpis z historii po odswiezeniu.
- Proba usuniecia brakujacego wpisu zwraca czytelny komunikat bledu po polsku.
- Dodawanie przychodu, wydatku i kategorii `Inne` nadal dziala.
- Interfejs tego ekranu nie pokazuje goals, dashboardu, auth, AI, chmury, edycji ani custom kategorii.

## Testing Strategy

- Backend: testy endpointow w istniejacym `FinanceEndpointsTests.cs`, bez osobnego test project.
- Frontend: typecheck/build oraz reczny smoke test, zgodnie z poprzednim change'em.
- Nie dodajemy e2e ani nowej biblioteki testowej.

## Migration Notes

Brak migracji. Usuwanie korzysta z istniejacej tabeli transakcji i nie zmienia schematu.

## References

- `context/foundation/prd.md`
- `context/foundation/roadmap.md`
- `context/foundation/tech-stack.md`
- `context/changes/mvp-frontend/plan.md`
- `Endpoints/FinanceEndpoints.cs`
- `Contracts/FinanceContracts.cs`
- `Persistence/FinanceDbContext.cs`
- `frontend/src/api/financeApi.ts`
- `frontend/src/components/TransactionHistory.tsx`
- `tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs`

## Progress

> Convention: `- [ ]` pending, `- [x]` done. Append `- <commit sha>` when a step lands. Do not rename step titles.

### Phase 1: Backend delete endpoint

#### Automated

- [x] 1.1 `DELETE /api/transactions/{id}` zwraca 204 i usuwa wpis default local profile - 475dd31
- [x] 1.2 Brakujacy lub obcy profilowo wpis zwraca 404 - 475dd31
- [x] 1.3 Testy backendowe dla delete przechodza - 475dd31

#### Manual

- [x] 1.4 Endpoint pozostaje ograniczony do default local profile - 475dd31

### Phase 2: Frontend delete integration

#### Automated

- [x] 2.1 `npm run typecheck` przechodzi po dodaniu operacji delete - 086dae2
- [x] 2.2 `npm run build` przechodzi po dodaniu operacji delete - 086dae2

#### Manual

- [x] 2.3 Historia pokazuje przycisk `Usuń`, confirm i odswiezenie po sukcesie - 086dae2
- [x] 2.4 Anulowanie confirm nie usuwa transakcji - 086dae2

### Phase 3: Polish current transaction UI

#### Automated

- [x] 3.1 `npm run typecheck` przechodzi po spolszczeniu UI - 4c45622
- [x] 3.2 `npm run build` przechodzi po spolszczeniu UI - 4c45622

#### Manual

- [x] 3.3 Obecny ekran transakcji jest po polsku, wlacznie z kategoriami, statusami i bledami - 4c45622
- [x] 3.4 Dodawanie przychodu, wydatku i `Inne` nadal dziala - 4c45622
- [x] 3.5 Zakres nadal nie zawiera nowych obszarow MVP - 4c45622

### Phase 4: Verification and change closeout

#### Automated

- [x] 4.1 `dotnet restore .\ai-finance-tracker.csproj` przechodzi - cb93918
- [x] 4.2 `dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false` przechodzi - cb93918
- [x] 4.3 `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false` przechodzi - cb93918
- [x] 4.4 `npm run typecheck` przechodzi - cb93918
- [x] 4.5 `npm run build` przechodzi - cb93918

#### Manual

- [x] 4.6 Reczny smoke test usuwania i polskiego UI przechodzi - cb93918
- [x] 4.7 Dokumentacja zmiany i Progress sa kompletne - cb93918
