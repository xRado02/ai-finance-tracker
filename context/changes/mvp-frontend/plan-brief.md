# Frontend dla transakcji MVP - Plan Brief

> Full plan: `context/changes/mvp-frontend/plan.md`

## What & Why

Budujemy pierwszy frontend React dla AI Finance Tracker, ale tylko nad API, ktore juz istnieje. Uzytkownik ma lokalnie dodac przychod albo wydatek z kategoria i zobaczyc historie transakcji bez goals, dashboardu i innych funkcji, ktorych backend jeszcze nie wspiera.

## Starting Point

Backend ma juz `GET /api/categories`, `POST /api/transactions` i `GET /api/transactions`, a frontend nie istnieje. PRD opisuje szerszy MVP, ale ten change celowo nie dorabia brakujacych API ani atrap UI.

## Desired End State

W repo istnieje `frontend/` z Vite + React + TypeScript. Aplikacja ma jeden roboczy ekran: formularz dodawania transakcji, kategorie pobierane z backendu i historie transakcji. Komunikacja idzie przez relatywne `/api/...` i Vite proxy do lokalnego API.

## Key Decisions Made

| Decision | Choice | Why |
| --- | --- | --- |
| Zakres | Tylko obecne endpointy transakcji i kategorii | Backend nie ma jeszcze goals ani dashboard summaries, wiec frontend nie bedzie udawal pelnego MVP. |
| UI | Jeden ekran roboczy | Najszybciej domyka realny user flow bez routingu i nawigacji do nieistniejacych funkcji. |
| Setup | Vite + React + TypeScript w `frontend/` | Pasuje do ustalonego React kierunku i izoluje frontend od projektu API. |
| Styling | Wlasny CSS bez biblioteki UI | Minimalizuje zaleznosci i research, a wystarcza dla narzedziowego MVP. |
| Weryfikacja | Typecheck/build + manualny smoke test | To pierwszy frontend w repo, wiec szybka weryfikacja jest lepsza niz duzy setup e2e. |
| API URL | Relatywne `/api/...` + Vite proxy | Unika CORS i twardych lokalnych adresow w kodzie aplikacji. |

## Scope

**In scope:**

- `frontend/` z Vite + React + TypeScript
- jeden ekran: formularz transakcji + historia
- pobieranie kategorii z `GET /api/categories`
- tworzenie transakcji przez `POST /api/transactions`
- pobieranie historii z `GET /api/transactions`
- filtrowanie kategorii po typie z `Other` jako fallback
- podstawowe stany loading/error/empty
- proste, narzedziowe CSS

**Out of scope:**

- goals i goal progress
- dashboard, podsumowania, struktura wydatkow, top kategorie
- auth, profile, AI, chmura, import bankowy
- edycja/usuwanie transakcji
- custom kategorie
- biblioteka UI i testy e2e

## Architecture / Approach

Frontend bedzie osobnym projektem w `frontend/`. Komponenty UI beda korzystac z malego typed API clienta, ktory wywoluje relatywne `/api/...`; Vite w development proxy'uje te requesty do `http://localhost:5218`. Backend pozostaje zrodlem walidacji i persystencji.

## Phases at a Glance

| Phase | What it delivers | Key risk |
| --- | --- | --- |
| 1. Frontend scaffold and local API wiring | Projekt Vite/React/TS, skrypty i proxy | Zaleznosci npm moga wymagac sieci podczas implementacji. |
| 2. API client and typed contracts | Typy i fetch wrapper dla trzech endpointow | Niepoprawna obsluga ProblemDetails moglaby ukryc bledy backendu. |
| 3. Single transaction workspace UI | Formularz, historia, kategorie i stany UI | UI moze przypadkiem zasugerowac funkcje poza backendiem. |
| 4. Verification and repository guidance | Pelna weryfikacja i aktualizacja AGENTS.md | Dokumentacja moze nie opisac nowego frontend workflow. |

**Prerequisites:** Backend API z `transaction-entry-history` jest dostepne lokalnie.
**Estimated effort:** ok. 2-3 sesje przez 4 male fazy.

## Open Risks & Assumptions

- Zakladamy Node/npm dostepne lokalnie; jesli nie, implementacja musi najpierw doprecyzowac runtime.
- Pierwszy `npm install` moze wymagac sieci i zgody na pobranie paczek.
- Backend musi miec zastosowane migracje w lokalnym srodowisku, zeby manualny smoke test mogl zapisac dane.

## Success Criteria (Summary)

- Uzytkownik lokalnie dodaje `Income` i `Expense` z kategoria, a nowe transakcje widzi w historii.
- Frontend przechodzi `npm run typecheck` i `npm run build`, a backendowe testy nadal przechodza.
- UI nie pokazuje ani nie implementuje funkcji spoza obecnego backendu.
