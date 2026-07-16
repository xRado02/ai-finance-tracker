---
change_id: dashboard-layout
title: Osobne sekcje dashboardu i nawigacja aplikacji
status: implemented
created: 2026-07-16
updated: 2026-07-16
---

# Plan: Osobne sekcje dashboardu i nawigacja aplikacji

## Cel

Zamienić obecny długi ekran w prostą aplikację z pięcioma sekcjami przełączanymi bez routera, zachowując wspólny wybór miesiąca i istniejące komponenty.

## Zakres

### W zakresie

- Nawigacja sekcji `dashboard`, `transactions`, `recurring`, `goals`, `settings` w `App.tsx`.
- Dashboard jako osobny główny widok z saldem całkowitym i miesięcznym, kategoriami oraz istniejącymi celami/prognozą.
- Transakcje jako osobny widok z formularzem, historią i wybranym okresem.
- Stałe transakcje, cele i ustawienia jako osobne widoki korzystające z istniejących komponentów.
- Responsywny sidebar/topbar bez nowych bibliotek.

### Poza zakresem

- Nowe endpointy i migracje.
- Duplikowanie logiki recurring, goals albo goal-forecast.
- React Router, auth, AI, cloud, import bankowy, realtime, background jobs i zaawansowane wykresy.

## Fazy

### Phase 1: Section navigation and composition

Dodać stan aktywnej sekcji, przyciski nawigacji i warunkowe renderowanie istniejących komponentów w pięciu widokach. Zachować wspólny `selectedPeriod` i odświeżanie danych.

### Phase 2: Dashboard and responsive layout polish

Ułożyć dashboard jako pierwszy widok oraz dopasować sidebar, topbar, spacing i mobile menu do krótszych sekcji bez zmiany kontraktów danych.

### Phase 3: Verification and closeout

Uruchomić testy backendu, typecheck/build frontendu, sprawdzić zakres i przygotować instrukcję ręcznego smoke testu bez automatycznego uruchamiania serwerów.

## Success Criteria

### Automated Verification

- `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false` przechodzi.
- `dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false` przechodzi bez ostrzeżeń.
- `npm run typecheck` w `frontend/` przechodzi.
- `npm run build` w `frontend/` przechodzi.

### Manual Verification

- Kliknięcie sekcji pokazuje tylko właściwy widok, a menu mobilne można zamknąć.
- Zmiana miesiąca w topbarze nadal odświeża transakcje, podsumowania i generowanie recurring.
- Dashboard rozróżnia saldo miesiąca od salda całkowitego i pokazuje istniejący postęp celów.
- Formularze transakcji, recurring, celów i ustawień zachowują dotychczasowe działanie.

## Progress

### Phase 1: Section navigation and composition

- [x] 1.1 App ma stan aktywnej sekcji i nawigacja przełącza pięć widoków
- [x] 1.2 Istniejące komponenty są renderowane tylko w odpowiednich sekcjach bez duplikowania logiki
- [x] 1.3 Wspólny wybór okresu i odświeżanie danych działają we wszystkich zależnych widokach

### Phase 2: Dashboard and responsive layout polish

- [x] 2.1 Dashboard jest domyślnym, osobnym widokiem z saldem miesięcznym i całkowitym
- [x] 2.2 Sidebar/topbar i mobile menu są spójne z przełączaniem sekcji

### Phase 3: Verification and closeout

- [x] 3.1 Backend test/build oraz frontend typecheck/build przechodzą — testy 39/39, build API 0 ostrzeżeń, frontend typecheck/build — commit baac6d8
- [x] 3.2 Zakres nie zawiera nowych endpointów ani duplikacji goals/forecast/recurring — commit baac6d8
- [x] 3.3 Manual smoke test jest gotowy do wykonania ręcznie; nie uruchamiano go automatycznie — commit baac6d8
