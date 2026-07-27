---
date: 2026-07-27T19:23:15+02:00
researcher: Codex
git_commit: 8280ffc22df9a6c20839108d0bc76af77d49d7ba
branch: main
repository: ai-finance-tracker
topic: "Minimalne testy krytycznych inwariantów finansowych"
tags: [research, tests, finance, api, persistence]
status: complete
last_updated: 2026-07-27
last_updated_by: Codex
---

# Research: Minimalne testy krytycznych inwariantów finansowych

**Date**: 2026-07-27T19:23:15+02:00
**Researcher**: Codex
**Git Commit**: 8280ffc22df9a6c20839108d0bc76af77d49d7ba
**Branch**: main
**Repository**: ai-finance-tracker

## Research Question

Które istniejące zachowania finansowe wymagają kilku dodatkowych testów o
wysokiej wartości, bez duplikowania obecnego pokrycia i bez zmiany funkcjonalności
aplikacji?

## Summary

Obecne 39 testów backendu pokrywa podstawowy CRUD, filtrowanie historii,
izolację profilu, saldo globalne, miesięczne podsumowanie, cztery statusy
prognozy oraz sekwencyjną deduplikację recurring. Największą wartość dają testy
przekrojowe między później dodanymi funkcjami, a nie kolejne osobne przypadki
endpointów.

Rekomendowany pakiet to jeden nowy scenariusz integracyjny oraz trzy wzmocnienia
istniejących testów. Nie wymaga zmian produkcyjnych. Test migracji SQL Server
pozostaje osobnym change'em, ponieważ obecny host testowy celowo używa SQLite
in-memory i `EnsureCreated()`.

## Detailed Findings

### Saldo i podsumowania

- Globalne saldo uwzględnia `InitialBalance`, natomiast miesięczne podsumowanie
  liczy wyłącznie transakcje wybranego okresu
  ([FinanceEndpoints.cs:294](https://github.com/xRado02/ai-finance-tracker/blob/8280ffc22df9a6c20839108d0bc76af77d49d7ba/Endpoints/FinanceEndpoints.cs#L294),
  [FinanceEndpoints.cs:344](https://github.com/xRado02/ai-finance-tracker/blob/8280ffc22df9a6c20839108d0bc76af77d49d7ba/Endpoints/FinanceEndpoints.cs#L344)).
- Istniejące testy sprawdzają te zachowania osobno, ale nie dowodzą w jednym
  scenariuszu, że stan początkowy wpływa na saldo globalne i postęp celu, a nie
  na saldo miesiąca ani średnią miesięczną prognozy
  ([FinanceEndpointsTests.cs:382](https://github.com/xRado02/ai-finance-tracker/blob/8280ffc22df9a6c20839108d0bc76af77d49d7ba/tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs#L382)).
- Test miesięcznych kategorii sprawdza nazwy, ale nie asertuje zagregowanych
  kwot kategorii
  ([FinanceEndpointsTests.cs:422](https://github.com/xRado02/ai-finance-tracker/blob/8280ffc22df9a6c20839108d0bc76af77d49d7ba/tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs#L422)).

### Stałe transakcje

- Endpoint generuje wpisy pierwszego dnia wskazanego miesiąca i wyszukuje
  wcześniejsze wpisy dla tego okresu
  ([FinanceEndpoints.cs:499](https://github.com/xRado02/ai-finance-tracker/blob/8280ffc22df9a6c20839108d0bc76af77d49d7ba/Endpoints/FinanceEndpoints.cs#L499)).
- Sekwencyjna idempotencja jest sprawdzona dla miesiąca bieżącego, ale test
  wybranego miesiąca nie potwierdza, że maj można wygenerować tylko raz, a
  czerwiec niezależnie drugi raz
  ([FinanceEndpointsTests.cs:639](https://github.com/xRado02/ai-finance-tracker/blob/8280ffc22df9a6c20839108d0bc76af77d49d7ba/tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs#L639),
  [FinanceEndpointsTests.cs:679](https://github.com/xRado02/ai-finance-tracker/blob/8280ffc22df9a6c20839108d0bc76af77d49d7ba/tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs#L679)).
- Model zawiera dodatkową ochronę w postaci unikalnego indeksu
  `(RecurringTransactionId, TransactionDate)`
  ([FinanceDbContext.cs:79](https://github.com/xRado02/ai-finance-tracker/blob/8280ffc22df9a6c20839108d0bc76af77d49d7ba/Persistence/FinanceDbContext.cs#L79)).

### Walidacja API

- Kwota, data, długość opisu, istnienie kategorii, zgodność typu kategorii oraz
  `Other` są już pokryte
  ([FinanceEndpointsTests.cs:739](https://github.com/xRado02/ai-finance-tracker/blob/8280ffc22df9a6c20839108d0bc76af77d49d7ba/tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs#L739)).
- Brakuje jawnego dowodu, że nieznana wartość `TransactionType` jest odrzucana
  zarówno dla zwykłej, jak i stałej transakcji, mimo że endpointy mają taką
  walidację
  ([FinanceEndpoints.cs:574](https://github.com/xRado02/ai-finance-tracker/blob/8280ffc22df9a6c20839108d0bc76af77d49d7ba/Endpoints/FinanceEndpoints.cs#L574),
  [FinanceEndpoints.cs:622](https://github.com/xRado02/ai-finance-tracker/blob/8280ffc22df9a6c20839108d0bc76af77d49d7ba/Endpoints/FinanceEndpoints.cs#L622)).

### Izolacja bazy i migracje

- Każdy test API tworzy własną bazę SQLite in-memory, zastępując produkcyjny
  provider SQL Server. To chroni lokalne dane użytkownika
  ([FinanceEndpointsTests.cs:912](https://github.com/xRado02/ai-finance-tracker/blob/8280ffc22df9a6c20839108d0bc76af77d49d7ba/tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs#L912)).
- `EnsureCreated()` omija łańcuch migracji, więc obecny zestaw nie wykryje
  rozjazdów powodujących błędy brakujących tabel lub kolumn. Ten obszar wymaga
  oddzielnego smoke testu na jednorazowej bazie o kontrolowanej nazwie.

## Code References

- `Endpoints/FinanceEndpoints.cs:294` - saldo całkowite ze stanem początkowym.
- `Endpoints/FinanceEndpoints.cs:344` - miesięczne podsumowanie.
- `Endpoints/FinanceEndpoints.cs:499` - generowanie recurring dla okresu.
- `Persistence/FinanceDbContext.cs:79` - unikalny indeks recurring.
- `tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs:382` -
  istniejący test globalnego salda.
- `tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs:422` -
  istniejący test miesięcznych agregacji.
- `tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs:679` -
  istniejący test generowania dla wskazanego miesiąca.

## Architecture Insights

Publiczne endpointy i baza SQLite in-memory są właściwym poziomem testowania
inwariantów finansowych. Pozwala to sprawdzić kontrakt HTTP, mapowanie DTO,
zapisy EF Core i wynik agregacji bez mockowania logiki pod testem. Nie ma
uzasadnienia dla nowego runnera, nowych helperów ani zmian w kodzie aplikacji.

## Historical Context

- `context/changes/initial-balance-settings/research.md` ustala, że
  `InitialBalance` wpływa na saldo całkowite i cele, lecz nie jest przychodem.
- `context/changes/monthly-period-navigation/research.md` ustala wspólny okres i
  generowanie recurring dla wybranego miesiąca.
- `context/changes/goal-forecast/research.md` ustala cztery statusy prognozy i
  miesięczną nadwyżkę opartą wyłącznie na transakcjach.
- `context/changes/recurring-transactions/research.md` ustala idempotencję
  generowania w obrębie miesiąca.

## Related Research

- `context/foundation/test-plan.md`
- `context/changes/initial-balance-settings/research.md`
- `context/changes/monthly-period-navigation/research.md`
- `context/changes/goal-forecast/research.md`
- `context/changes/recurring-transactions/research.md`

## Open Questions

- Współbieżne żądania generowania recurring mogą oprzeć się dopiero o unikalny
  indeks i zwrócić błąd zapisu dla jednego requestu. To ryzyko nie uzasadnia
  rozszerzania obecnego, minimalnego pakietu testów.
- Semantyka średniej prognozy pomija miesiące bez transakcji. Jest to decyzja
  produktowa poza zakresem jakościowym tego change'a.
