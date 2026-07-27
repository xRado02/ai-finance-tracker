---
change_id: testing-critical-finance-invariants
title: Krytyczne inwarianty finansowe
status: planned
created: 2026-07-27
updated: 2026-07-27
---

# Krytyczne inwarianty finansowe - Implementation Plan

## Overview

Dodać minimalny pakiet testów integracyjnych o wysokiej wartości dla
istniejącego MVP. Pakiet ma wzmocnić zachowania przekrojowe między saldem
początkowym, miesięcznym podsumowaniem, prognozą celu i recurring, bez
budowania nowej infrastruktury testowej ani zmiany funkcjonalności produktu.

## Current State Analysis

Backend ma 39 zielonych testów xUnit opartych o `WebApplicationFactory`, EF Core
i osobną bazę SQLite in-memory. Podstawowe przypadki CRUD, izolacja profilu,
saldo, miesięczne podsumowania, recurring i statusy prognozy są już pokryte.

Research ujawnił jedną konkretną lukę funkcjonalną: miesięczne podsumowanie bez
parametrów okresu przechodzi wspólną walidację, a następnie dereferencjonuje
brakującą datę. Powinno zwrócić kontrolowane `400`, nie `500`.

## Desired End State

- Miesięczne podsumowanie bez pełnego okresu zwraca `400 ValidationProblem`.
- Test przekrojowy dowodzi, że `InitialBalance` wpływa na saldo całkowite i
  bieżącą kwotę celu, ale nie na saldo miesiąca ani średnią nadwyżkę.
- Miesięczne kwoty kategorii są asertowane, nie tylko ich nazwy.
- Recurring jest idempotentne osobno dla każdego wskazanego miesiąca.
- Nieznany `TransactionType` jest odrzucany dla zwykłej i stałej transakcji.
- Pełny backendowy zestaw testów oraz build pozostają zielone.

### Key Discoveries

- `TryGetMonthRange` celowo dopuszcza brak okresu dla historii transakcji, ale
  miesięczne podsumowanie wymaga obu wartości.
- SQLite in-memory bezpiecznie izoluje testy od lokalnej bazy użytkownika.
- Migracje SQL Server nie są wykonywane przez ten host i pozostają osobnym
  change'em `testing-database-migrations`.
- Większość potrzebnego pokrycia można uzyskać przez wzmocnienie jednego
  istniejącego pliku testowego.

## What We're NOT Doing

- Nowe funkcje produktu, przebudowa API, UI lub frontendowe testy.
- Refaktor endpointów, kontraktów, domeny lub persystencji.
- Testy pisane wyłącznie dla coverage.
- Współbieżny test generatora recurring.
- Zmiana semantyki średniej prognozy dla pustych miesięcy.
- Testowanie migracji lub używanie lokalnej bazy użytkownika.

## Implementation Approach

Jedna faza łączy prawdziwy cykl RED→GREEN dla wykrytego błędu walidacji z
testami charakteryzującymi istniejące inwarianty. Najpierw zostanie uruchomiony
pojedynczy failing test dla brakującego okresu, następnie dodany najmniejszy
guard w handlerze miesięcznego podsumowania. Pozostałe testy nie wymagają zmian
produkcyjnych.

## Phase 1: Minimalne testy krytycznych inwariantów

### Overview

Naprawić jedną ujawnioną lukę walidacji i dodać mały zestaw przekrojowych
asercji w istniejącym stylu testów API.

### Changes Required

#### 1. Walidacja miesięcznego podsumowania

**Files**:
- `tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs`
- `Endpoints/FinanceEndpoints.cs`

**Intent**: Żądanie miesięcznego podsumowania bez pełnych parametrów `year` i
`month` ma kończyć się kontrolowanym `400`.

**Contract**: `GET /api/dashboard/monthly-summary`, żądanie tylko z `year` oraz
żądanie tylko z `month` zwracają `ValidationProblem` z odpowiednim kluczem.
Historia transakcji zachowuje dotychczasowe opcjonalne filtrowanie.

#### 2. Inwariant salda i prognozy

**File**: `tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs`

**Intent**: Udowodnić jednym scenariuszem, że stan początkowy jest częścią salda
całkowitego i bieżącej kwoty celu, ale nie jest przychodem ani częścią salda
miesiąca lub średniej nadwyżki.

#### 3. Agregacje, recurring i walidacja enum

**File**: `tests/AiFinanceTracker.Tests/Endpoints/FinanceEndpointsTests.cs`

**Intent**: Wzmocnić istniejące scenariusze bez duplikowania testów.

**Contract**:
- miesięczne kategorie mają poprawne zagregowane kwoty,
- powtórne generowanie za maj jest pomijane, a czerwiec może wygenerować osobny
  wpis,
- nieznany `TransactionType` zwraca błąd walidacji dla obu typów żądań.

### Success Criteria

#### Automated Verification

- Nowy test brakującego okresu najpierw zawodzi z odpowiedzią `500`, a po
  minimalnym guardzie przechodzi z `400`.
- Wszystkie nowe i rozszerzone asercje przechodzą.
- `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false`
  przechodzi w całości.
- `dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false`
  przechodzi bez ostrzeżeń.

#### Manual Verification

Brak osobnego smoke testu. Zakres jest wyłącznie backendowy, deterministyczny i
pokryty publicznymi żądaniami HTTP w izolowanym hoście testowym.

## Testing Strategy

### Unit Tests

Nie dodajemy testów jednostkowych. Inwarianty przecinają kontrakt HTTP, EF Core
i agregacje, więc test integracyjny daje lepszy sygnał.

### Integration Tests

Użyć istniejącego `FinanceApiFactory` oraz SQLite in-memory. Testować publiczne
odpowiedzi i wynikowe podsumowania, bez asercji prywatnych metod handlera.

### Manual Testing Steps

Nie są wymagane dla tej fazy.

## Performance Considerations

Pakiet dodaje najwyżej dwa nowe scenariusze oraz kilka asercji do istniejących
testów. Nie uruchamia zewnętrznej bazy ani przeglądarki.

## Migration Notes

Brak zmian schematu i migracji. Testy nie używają connection stringa aplikacji.

## References

- `context/foundation/test-plan.md`
- `context/changes/testing-critical-finance-invariants/research.md`
- `context/changes/initial-balance-settings/research.md`
- `context/changes/monthly-period-navigation/research.md`
- `context/changes/goal-forecast/research.md`

## Progress

> Convention: `- [ ]` pending, `- [x]` done. Append ` - <commit sha>` when a step lands. Do not rename step titles.

### Phase 1: Minimalne testy krytycznych inwariantów

#### Automated

- [x] 1.1 RED: brak pełnego okresu dla miesięcznego podsumowania ma test reprodukujący odpowiedź inną niż `400`.
- [x] 1.2 GREEN: miesięczne podsumowanie odrzuca brakujący rok lub miesiąc przez `ValidationProblem`.
- [x] 1.3 Test przekrojowy rozdziela saldo początkowe, saldo miesiąca i średnią nadwyżkę prognozy.
- [x] 1.4 Testy miesięcznych kategorii i recurring potwierdzają kwoty oraz idempotencję per okres.
- [x] 1.5 Testy walidacji odrzucają nieznany `TransactionType`.
- [x] 1.6 Pełny `dotnet test` i `dotnet build` przechodzą.
