---
change_id: recurring-transactions
title: Stałe przychody i wydatki
status: planned
created: 2026-07-15
updated: 2026-07-15
---

# Plan: Stałe przychody i wydatki

## Cel

Umożliwić zapisanie stałego przychodu lub wydatku i ręczne wygenerowanie go raz w bieżącym miesiącu.

## Zakres

### W zakresie

- Encja `RecurringTransaction` dla default local profile.
- Kwota, typ, kategoria, opis, aktywność oraz nullable FK z wygenerowanej `Transaction`.
- Migracja EF Core.
- `GET /api/recurring-transactions`.
- `POST /api/recurring-transactions`.
- `PATCH /api/recurring-transactions/{id}/status`.
- `POST /api/recurring-transactions/generate-current-month`.
- Idempotentne generowanie: jedna transakcja z danej definicji w danym miesiącu.
- Polski frontend: formularz, lista, status, przełączanie aktywności i przycisk generowania.
- Odświeżenie historii, dashboardu i listy stałych transakcji po mutacji.
- Testy API oraz frontend typecheck/build.

### Poza zakresem

- Background job, harmonogramy dzienne, wybór dowolnego miesiąca, edycja definicji, auth, cloud, import bankowy, AI i zaawansowane raporty.

## Kontrakt API

`GET /api/recurring-transactions` zwraca listę definicji z `id`, `amount`, `type`, `categoryId`, `categoryName`, `description`, `isActive`.

`POST /api/recurring-transactions` przyjmuje `amount`, `type`, `categoryId`, `description`, `isActive` i zwraca utworzoną definicję.

`PATCH /api/recurring-transactions/{id}/status` przyjmuje `isActive` i zwraca zaktualizowaną definicję.

`POST /api/recurring-transactions/generate-current-month` zwraca `month`, `generatedCount`, `skippedCount` oraz wygenerowane transakcje.

## Decyzje techniczne

- Bieżący miesiąc jest liczony po stronie backendu z lokalnej daty systemowej, a data transakcji to pierwszy dzień miesiąca.
- Duplikat rozpoznaje się po `RecurringTransactionId` i zakresie dat miesiąca.
- Nieaktywne definicje są pomijane przy generowaniu.
- Kategoria jest walidowana tak samo jak przy zwykłej transakcji, włącznie z fallbackiem `Other`.

## Fazy

### Phase 1: Persistence and contracts

Dodać encję, relacje, konfigurację, migrację i kontrakty API.

### Phase 2: API and tests

Dodać endpointy list/create/status/generate, walidację, idempotencję i testy izolacji profilu.

### Phase 3: Frontend

Dodać API client, stan App oraz polskie komponenty formularza/listy/generowania i odświeżanie danych.

### Phase 4: Verification and closeout

Uruchomić build/test/typecheck, sprawdzić migrację, zakres i przygotować manual smoke test.

## Success Criteria

### Automated Verification

- `dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false` przechodzi.
- `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false` przechodzi.
- `npm run typecheck` w `frontend/` przechodzi.
- `npm run build` w `frontend/` przechodzi.

### Manual Verification

- Można utworzyć przychód i wydatek stały po polsku.
- Nieaktywna definicja jest widoczna i nie jest generowana.
- Przycisk generuje wpisy za bieżący miesiąc i odświeża historię/dashboard.
- Drugie kliknięcie w tym samym miesiącu nie tworzy duplikatu.

## Progress

> Convention: `- [ ]` pending, `- [x]` done. Append `- <commit sha>` when a step lands.

### Phase 1: Persistence and contracts

- [x] 1.1 Encja, relacje i konfiguracja EF są dodane
- [x] 1.2 Migracja obejmuje recurring definitions i nullable link transakcji
- [x] 1.3 Kontrakty request/response są dodane

### Phase 2: API and tests

- [ ] 2.1 Lista, create i status działają tylko dla default profile
- [ ] 2.2 Generowanie bieżącego miesiąca jest idempotentne i pomija inactive
- [ ] 2.3 Testy API pokrywają walidację, duplikat i izolację profilu

### Phase 3: Frontend

- [ ] 3.1 API client i stan App są zintegrowane
- [ ] 3.2 Formularz/lista/status/generowanie są po polsku
- [ ] 3.3 Refresh historii i dashboardu działa po generowaniu

### Phase 4: Verification and closeout

- [ ] 4.1 Backend build/test oraz frontend typecheck/build przechodzą
- [ ] 4.2 Migracja i zakres są sprawdzone
- [ ] 4.3 Manual smoke test jest gotowy do potwierdzenia
