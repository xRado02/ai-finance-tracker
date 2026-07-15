---
change_id: financial-goals
title: Cele finansowe w lokalnym API
status: draft
created: 2026-07-15
updated: 2026-07-15
---

# Plan: Cele finansowe w lokalnym API

## Cel

Dodac lokalna persystencje i API celow finansowych, aby frontend mogl pozniej wyswietlic liste celow i progress bez wprowadzania drugiego zrodla kwoty postepu.

## Kontekst

- `FinanceDbContext` ma seeded `DefaultLocalProfileId`, encje `Transaction` i konfiguracje SQL Server.
- Istnieje migracja `Migrations/20260708165919_InitialCreate.cs` oraz snapshot modelu.
- Endpointy finansowe sa mapowane przez `Endpoints/FinanceEndpoints.cs`, a kontrakty sa w `Contracts/FinanceContracts.cs`.
- Testy API korzystaja z SQLite in-memory i `EnsureCreated`, wiec po dodaniu encji testy moga pokryc relacje bez zewnetrznego SQL Server.

## Zakres

### W zakresie

- Encja `Goal` z `Id`, `Name`, `TargetAmount`, `LocalProfileId`.
- Relacja `Goal -> LocalProfile` z kasowaniem celow razem z profilem.
- Migracja EF Core dodajaca tabele celow i klucz obcy.
- `GET /api/goals` dla default local profile.
- `POST /api/goals` dla default local profile.
- Progress odpowiedzi liczony z bieżącego salda transakcji: `max(0, min(balance / target * 100, 100))`.
- Walidacja pustej/nadmiernie dlugiej nazwy i dodatniej kwoty docelowej.
- Testy modelu, endpointow, walidacji i wyliczenia progressu.

### Poza zakresem

- Frontend celow, edycja/usuwanie celow, auth, multi-user, AI, cloud, import bankowy, notyfikacje, realtime i wykresy.
- Osobne `CurrentAmount` zapisywane w bazie; kwota jest wyprowadzana z transakcji.
- Endpoint `PUT/PATCH` progressu. Progress zmienia sie po dodaniu/usunieciu transakcji.

## Kontrakt API

### `POST /api/goals`

Request:

```json
{ "name": "Poduszka finansowa", "targetAmount": 10000.00 }
```

Response `201 Created`:

```json
{
  "id": "...",
  "name": "Poduszka finansowa",
  "targetAmount": 10000.00,
  "currentAmount": 0.00,
  "progressPercentage": 0.0
}
```

### `GET /api/goals`

Zwraca cele default local profile posortowane alfabetycznie po nazwie oraz dla kazdego `currentAmount` i `progressPercentage` wyliczone z transakcji tego profilu. `currentAmount` jest `max(0, balance)`, aby ujemne saldo nie generowalo ujemnego postepu.

## Decyzje techniczne

- `Goal` korzysta z tego samego profilu lokalnego co transakcje.
- Obliczenie salda korzysta z `Income` jako plus i `Expense` jako minus.
- Ujemne saldo daje `currentAmount = 0`; saldo ponad target daje maksymalnie `100%` progressu.
- Nazwa ma maksymalnie 120 znakow, kwota docelowa ma precyzje `18,2` i musi byc wieksza od zera.
- Endpointy nie przyjmuja `LocalProfileId` od klienta.
- Migracja jest generowana standardowym `dotnet ef migrations add`, bez recznego resetowania bazy.

## Fazy

### Phase 1: Goal persistence and migration

Dodac encje, nawigacje, konfiguracje DbContext, test modelu oraz migracje `AddFinancialGoals`.

### Phase 2: Goals API and tests

Dodac kontrakty, endpointy GET/POST, walidacje, wyliczenie progressu i testy API.

### Phase 3: Verification and change closeout

Uruchomic restore/build/test, sprawdzic migracje i reczny smoke test API, a potem zapisac Progress, SHA i status change'a.

## Success Criteria

### Automated Verification

- `dotnet restore .\ai-finance-tracker.csproj` przechodzi.
- `dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false` przechodzi.
- `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false` przechodzi.
- Migracja `AddFinancialGoals` istnieje i snapshot zawiera `Goal`.

### Manual Verification

- `POST /api/goals` tworzy cel z `201` i poczatkowym progressem.
- `GET /api/goals` zwraca cele default profilu z aktualnym saldem i procentem.
- Dodanie transakcji zmienia `currentAmount` oraz `progressPercentage` przy kolejnym GET.
- Niepoprawna nazwa lub kwota zwraca czytelny blad walidacji.
- API celow nie przyjmuje profilu z requestu i nie wychodzi poza MVP.

## Testing Strategy

- Test modelu sprawdza encje, relacje i precision kwoty.
- Testy API sprawdzaja utworzenie, liste, progress, walidacje i izolacje profilu.
- Bez nowego projektu testowego i bez testow frontendowych.

## Migration Notes

Migracja tworzy tabele `Goals`, indeks po `LocalProfileId` i klucz obcy do `LocalProfiles`. Nie usuwa ani nie zmienia danych transakcji.

## References

- `context/foundation/prd.md` FR-009, FR-010
- `context/foundation/roadmap.md` S-03 Financial Goal Progress
- `context/foundation/tech-stack.md`
- `Persistence/FinanceDbContext.cs`
- `Domain/LocalProfile.cs`
- `Domain/Transaction.cs`
- `Endpoints/FinanceEndpoints.cs`
- `Contracts/FinanceContracts.cs`
- `Migrations/20260708165919_InitialCreate.cs`
- `tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs`
- `tests/AiFinanceTracker.Tests/Persistence/FinanceDbContextTests.cs`

## Progress

> Convention: `- [ ]` pending, `- [x]` done. Append `- <commit sha>` when a step lands. Do not rename step titles.

### Phase 1: Goal persistence and migration

#### Automated

- [x] 1.1 Encja `Goal`, relacja profilu i konfiguracja precision sa obecne - aa712f6
- [x] 1.2 Test modelu i relacji celu przechodzi - aa712f6
- [x] 1.3 Migracja `AddFinancialGoals` i snapshot sa obecne - aa712f6

#### Manual

- [x] 1.4 Migracja nie zmienia tabel transakcji ani kategorii - aa712f6

### Phase 2: Goals API and tests

#### Automated

- [x] 2.1 `POST /api/goals` tworzy cel i zwraca progress
- [x] 2.2 `GET /api/goals` zwraca cele default profilu z obliczonym saldem
- [x] 2.3 Walidacja i izolacja profilu sa pokryte testami
- [x] 2.4 Pelny backend test suite przechodzi

#### Manual

- [x] 2.5 Progress zmienia sie po dodaniu transakcji

### Phase 3: Verification and change closeout

#### Automated

- [ ] 3.1 `dotnet restore .\ai-finance-tracker.csproj` przechodzi
- [ ] 3.2 `dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false` przechodzi
- [ ] 3.3 `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false` przechodzi

#### Manual

- [ ] 3.4 Reczny smoke test API celow przechodzi
- [ ] 3.5 Dokumentacja zmiany i Progress sa kompletne
