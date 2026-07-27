---
change_id: frontend-error-recovery
title: Ponawianie połączenia z lokalnym API
status: planned
created: 2026-07-17
updated: 2026-07-17
---

# Ponawianie połączenia z lokalnym API - Implementation Plan

## Overview

Dodać użytkownikowi prostą drogę odzyskania aplikacji po chwilowym błędzie lokalnego backendu. Ponowienie ma działać bez przeładowania strony i korzystać z istniejącego ładowania danych dla aktualnie wybranego miesiąca.

## Current State Analysis

`App.tsx` ma centralny stan loading/ready/error i jedną funkcję pobierającą wszystkie dane. Po błędzie interfejs pokazuje polski komunikat, ale nie udostępnia akcji naprawczej; wszystkie sekcje pozostają zablokowane do ręcznego odświeżenia strony.

## Desired End State

W stanie error pasek statusu pokazuje komunikat oraz przycisk „Spróbuj ponownie”. Kliknięcie natychmiast przełącza aplikację w loading, ponawia istniejący komplet żądań dla `selectedPeriod`, a po sukcesie przywraca normalny stan ready bez przeładowania strony.

### Key Discoveries

- `frontend/src/App.tsx:73-99` ma gotowy punkt ponownego użycia: `loadFinanceData`.
- `frontend/src/App.tsx:201-206` jest jedynym miejscem wymagającym rozszerzenia interakcji błędu.
- `frontend/src/App.css:774-795` zawiera finalny wariant stylu statusu, więc nowa akcja powinna być stylowana obok niego.
- Brak test runnera frontendu; właściwa automatyczna weryfikacja to typecheck i build.

## What We're NOT Doing

- Automatyczne retry, exponential backoff, polling, timeouty i background jobs.
- Toasty, globalny error boundary, telemetryka i nowa biblioteka UI.
- Zmiany backendu, endpointów, DTO, persystencji, migracji lub zależności npm.
- Zachowywanie częściowo pobranych danych po błędzie jednego z równoległych żądań.

## Implementation Approach

Dodać w `App` mały handler retry, który przełącza stan na loading i wywołuje istniejące `loadFinanceData` z bieżącym okresem. W statusie error wyrenderować polski przycisk, a w CSS zapewnić czytelny układ i focus bez zmiany pozostałych sekcji.

## Critical Implementation Details

Stan loading musi zostać ustawiony synchronicznie przed rozpoczęciem ponownego pobierania. Dzięki temu przycisk znika lub jest nieaktywny po pierwszym kliknięciu i użytkownik nie uruchomi kilku równoległych prób.

## Phase 1: Ręczne odzyskiwanie po błędzie API

### Overview

Rozszerzyć istniejący pasek błędu o bezpieczną akcję retry i zweryfikować pełny scenariusz awaria-odzyskanie.

### Changes Required

#### 1. Stan i akcja retry

**File**: `frontend/src/App.tsx`

**Intent**: Umożliwić ponowne pobranie całego stanu finansowego dla bieżącego okresu bez odświeżania strony i bez duplikowania integracji API.

**Contract**: Handler retry ustawia `ApiStatus` na loading, następnie wywołuje istniejące `loadFinanceData(selectedPeriod)`. Wariant error paska statusu pokazuje komunikat oraz przycisk typu `button` z etykietą „Spróbuj ponownie”; podczas próby interfejs pokazuje istniejący polski stan ładowania i nie pozwala uruchomić równoległego retry.

#### 2. Styl akcji odzyskiwania

**File**: `frontend/src/App.css`

**Intent**: Zachować czytelność przycisku retry w obecnym ciemnym dashboardzie na desktopie i urządzeniu mobilnym.

**Contract**: Nowa klasa akcji mieści się w `.api-status`, ma widoczny hover/focus/disabled, nie rozszerza paska poza viewport i korzysta z obecnych zmiennych kolorów.

### Success Criteria

#### Automated Verification

- Typecheck frontendu przechodzi: `npm run typecheck` w `frontend/`.
- Build frontendu przechodzi: `npm run build` w `frontend/`.
- Diff implementacyjny nie zawiera zmian poza `frontend/src/App.tsx` i `frontend/src/App.css`.

#### Manual Verification

- Gdy backend jest niedostępny, aplikacja pokazuje polski błąd i przycisk „Spróbuj ponownie”; po uruchomieniu backendu kliknięcie odzyskuje dane wybranego miesiąca bez przeładowania strony i bez wielokrotnych równoległych prób.

**Implementation Note**: Manualny scenariusz wymaga kontrolowanego zatrzymania i ponownego uruchomienia lokalnego backendu; nie automatyzować uruchamiania serwerów przez `Start-Job`, `Start-Process` ani `cmd /c start`.

## Testing Strategy

### Unit Tests

- Nie dodajemy frameworka testowego dla tej małej zmiany; typy i renderowanie produkcyjne weryfikują istniejące komendy Vite/TypeScript.

### Integration Tests

- `npm run build` weryfikuje integrację komponentu i stylów w produkcyjnym bundlu.

### Manual Testing Steps

1. Uruchomić frontend przy wyłączonym backendzie i poczekać na stan błędu.
2. Potwierdzić polski komunikat i przycisk „Spróbuj ponownie”.
3. Kliknąć raz i sprawdzić przejście do stanu ładowania bez możliwości kolejnych kliknięć.
4. Uruchomić backend poleceniem z `AGENTS.md`, kliknąć retry i potwierdzić dane dla wybranego miesiąca bez reloadu.
5. Sprawdzić układ oraz focus przycisku na szerokim i mobilnym viewportcie.

## Performance Considerations

Retry wykonuje ten sam mały zestaw lokalnych żądań co pierwsze ładowanie. Brak automatycznego ponawiania ogranicza niekontrolowany ruch i złożoność.

## Migration Notes

Brak zmian bazy danych, migracji i kontraktów API. Wycofanie polega na usunięciu handlera, przycisku i jego stylu.

## References

- Related research: `context/changes/frontend-error-recovery/research.md`
- Existing loader: `frontend/src/App.tsx:73-99`
- Existing error UI: `frontend/src/App.tsx:201-206`
- Status styles: `frontend/src/App.css:774-795`
- Frontend scripts: `frontend/package.json:6-10`

## Progress

> Convention: `- [ ]` pending, `- [x]` done. Append ` - <commit sha>` when a step lands. Do not rename step titles.

### Phase 1: Ręczne odzyskiwanie po błędzie API

#### Automated

- [x] 1.1 Typecheck frontendu przechodzi: `npm run typecheck` w `frontend/`.
- [x] 1.2 Build frontendu przechodzi: `npm run build` w `frontend/`.
- [x] 1.3 Diff implementacyjny nie zawiera zmian poza `frontend/src/App.tsx` i `frontend/src/App.css`.

#### Manual

- [x] 1.4 Gdy backend jest niedostępny, aplikacja pokazuje polski błąd i przycisk „Spróbuj ponownie”; po uruchomieniu backendu kliknięcie odzyskuje dane wybranego miesiąca bez przeładowania strony i bez wielokrotnych równoległych prób.
