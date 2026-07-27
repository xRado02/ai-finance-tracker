# Ponawianie połączenia z lokalnym API - Plan Brief

> Full plan: `context/changes/frontend-error-recovery/plan.md`
> Research: `context/changes/frontend-error-recovery/research.md`

## What & Why

Po chwilowym błędzie lokalnego backendu aplikacja wymaga dziś odświeżenia całej strony. Slice dodaje prosty polski przycisk retry, aby użytkownik mógł odzyskać działający widok bez utraty kontekstu miesiąca.

## Starting Point

`App.tsx` ma centralny `ApiStatus` i funkcję `loadFinanceData`, która pobiera wszystkie dane. Pasek błędu pokazuje komunikat, ale nie ma akcji odzyskiwania.

## Desired End State

W stanie błędu widoczny jest przycisk „Spróbuj ponownie”. Jedno kliknięcie przełącza aplikację w loading, ponawia dane dla aktualnego okresu i po sukcesie wraca do ready bez reloadu.

## Key Decisions Made

| Decision | Choice | Why | Source |
|----------|--------|-----|--------|
| Miejsce retry | `App.tsx` | Tam już żyją status i wspólny loader | Research |
| Zakres pobierania | Cały `loadFinanceData(selectedPeriod)` | Zapobiega częściowo niespójnemu stanowi | Research |
| Ochrona przed kliknięciami | Natychmiastowy loading | Prosty sposób blokady równoległych prób | Plan |
| UX błędów | Inline, po polsku | Pasuje do istniejącego paska bez nowej biblioteki | Research |

## Scope

**In scope:** handler retry, przycisk w statusie error, stan loading i dopasowany CSS.

**Out of scope:** backend, API, migracje, automatyczne retry, toasty, telemetryka i nowe zależności.

## Architecture / Approach

Przycisk wywołuje mały handler w `App`, który ustawia loading i ponownie używa istniejącego loadera dla `selectedPeriod`. Nie powstaje nowa warstwa ani kontrakt.

## Phases at a Glance

| Phase | What it delivers | Key risk |
|-------|------------------|----------|
| 1. Ręczne odzyskiwanie po błędzie API | Retry bez reloadu i pełny smoke test awaria-odzyskanie | Wielokrotne kliknięcia przed zmianą statusu |

**Prerequisites:** branch `codex/slice-b`, zainstalowane zależności w `frontend/`.
**Estimated effort:** jedna krótka sesja, dwa pliki implementacyjne.

## Open Risks & Assumptions

- Retry celowo powtarza wszystkie żądania, ponieważ obecny stan ready jest atomowym zestawem danych.
- Manualny test wymaga ręcznego sterowania backendem.

## Success Criteria (Summary)

- Błąd pokazuje czytelną polską akcję retry.
- Retry odzyskuje dane bieżącego miesiąca bez przeładowania i bez równoległych prób.
- Frontend przechodzi typecheck i build bez zmian backendu/API.
