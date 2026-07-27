---
change_id: testing-critical-browser-flow
title: Krytyczny przepływ przeglądarkowy
status: planned
created: 2026-07-27
updated: 2026-07-27
---

# Krytyczny przepływ przeglądarkowy - Implementation Plan

## Overview

Dodać dokładnie jeden test Playwright chroniący krytyczny flow: wybór miesiąca,
dodanie transakcji, widoczność w historii i aktualizacja miesięcznego
podsumowania.

## Current State Analysis

Funkcja jest gotowa w backendzie i frontendzie, ale repozytorium nie ma
`@playwright/test`, `playwright.config.*` ani istniejących speców. `/10x-e2e`
wymaga tej infrastruktury przed generowaniem testu i nie instaluje jej
automatycznie.

Drugim ograniczeniem jest baza. Uruchomienie testu przeciw zwykłemu connection
stringowi LocalDB mogłoby zapisać dane w bazie użytkownika. Konfiguracja wspiera
override przez `ConnectionStrings__FinanceDatabase`, więc przyszły setup musi
tworzyć bazę o unikalnej nazwie, migrować ją przed testem i usuwać w `finally`.

## Desired End State

- Playwright jest zależnością developerską frontendu.
- Konfiguracja uruchamia backend i Vite na stałych portach.
- Backend używa wyłącznie bazy `AiFinanceTracker_E2E_<unikalny-sufiks>`.
- Jeden niezależny spec wybiera miesiąc, dodaje unikalną transakcję i sprawdza
  historię oraz miesięczne sumy.
- Test używa locatorów po roli/etykiecie, czeka na stan i nie używa timeoutów.
- Cleanup usuwa tylko bazę z kontrolowanym prefiksem `AiFinanceTracker_E2E_`.

## What We're NOT Doing

- Duży zestaw E2E, testy każdej sekcji, screenshoty lub visual regression.
- Automatyzacja error recovery, dopóki kontrola awarii backendu nie będzie
  deterministyczna.
- Używanie normalnej bazy `AiFinanceTracker`.
- Zmiany funkcjonalności, UI, endpointów lub kontraktów.

## Implementation Approach

Po osobnej akceptacji instalacji Playwright:

1. Dodać `@playwright/test` i Chromium.
2. Dodać konfigurację z dwoma `webServer`: backendem na `5218` i Vite na
   `5173`.
3. Przekazać backendowi jednorazowy connection string przez zmienną środowiskową.
4. Przed startem zastosować migracje do tej bazy; po teście usunąć ją po
   sprawdzeniu prefiksu.
5. Utworzyć seed i reguły E2E wymagane przez `/10x-e2e`.
6. Wygenerować jeden spec na podstawie rzeczywistego accessibility tree.
7. Potwierdzić green oraz deliberate break, po czym natychmiast cofnąć break.

## Phase 1: Minimalna infrastruktura Playwright

### Changes Required

- zależność i skrypt pojedynczego speca w `frontend/package.json`,
- `frontend/playwright.config.ts`,
- bezpieczny setup/cleanup bazy testowej,
- seed spec i reguły E2E.

### Success Criteria

- Setup nigdy nie odwołuje się do bazy `AiFinanceTracker`.
- Pusty smoke spec może uruchomić oba serwery i załadować aplikację.

## Phase 2: Jeden krytyczny flow

### Changes Required

**Scenario**:

1. Otwórz aplikację.
2. Wybierz z góry ustalony miesiąc i rok.
3. Dodaj wydatek z unikalnym opisem i znaną kategorią.
4. Potwierdź widoczność wpisu w historii wybranego miesiąca.
5. Potwierdź zmianę wydatków i salda miesiąca.

### Success Criteria

- Spec przechodzi samodzielnie i przy ponownym uruchomieniu.
- Nie używa CSS/XPath ani `waitForTimeout`.
- Deliberate break wyniku miesięcznego powoduje RED, a po cofnięciu test wraca
  do GREEN.

## Testing Strategy

Jedna ścieżka real browser → frontend → proxy → API → jednorazowa baza SQL
Server LocalDB. Brak mockowania wewnętrznych granic tego flow.

## Migration Notes

Migracje są stosowane wyłącznie do bazy E2E z losowym sufiksem. Cleanup musi
sprawdzić prefiks przed usunięciem. Brak zmian istniejących migracji.

## Progress

> Convention: `- [ ]` pending, `- [x]` done. Append ` - <commit sha>` when a step lands. Do not rename step titles.

### Phase 1: Minimalna infrastruktura Playwright

#### Automated

- [ ] 1.1 Playwright, config i komenda pojedynczego speca są dostępne.
- [ ] 1.2 Jednorazowa baza E2E jest tworzona, migrowana i bezpiecznie usuwana.
- [ ] 1.3 Seed spec i reguły E2E są gotowe.

### Phase 2: Jeden krytyczny flow

#### Automated

- [ ] 2.1 Spec dodaje transakcję do wybranego miesiąca i widzi ją w historii.
- [ ] 2.2 Spec potwierdza aktualizację miesięcznych wydatków i salda.
- [ ] 2.3 Green run i deliberate-break RED są potwierdzone.
