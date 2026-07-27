# Automatyczna weryfikacja projektu na GitHub Actions - Plan Brief

> Full plan: `context/changes/github-ci/plan.md`
> Research: `context/changes/github-ci/research.md`

## What & Why

Publiczne repozytorium nie ma automatycznej kontroli jakości. Slice dodaje minimalne CI dla istniejącego backendu i frontendu, aby regresje były widoczne przed scaleniem do `main`.

## Starting Point

Lokalne komendy restore/build/test/typecheck są sprawdzone i opisane w `AGENTS.md`, a tech stack wskazuje GitHub Actions. Brakuje tylko workflow.

## Desired End State

Push i pull request do `main` uruchamiają dwa niezależne joby: backend .NET 9 oraz frontend Node.js. Oba korzystają z obecnych komend i nie wymagają sekretów ani SQL Servera.

## Key Decisions Made

| Decision | Choice | Why | Source |
|----------|--------|-----|--------|
| Struktura | Jeden workflow, dwa joby | Czytelny wynik i równoległa praca backend/frontend | Research |
| Triggery | Push i PR do `main` | Minimalny zakres ochrony głównej gałęzi | Plan |
| Baza danych | Bez SQL Servera i migracji | Testy korzystają z SQLite in-memory | Research |
| Instalacja npm | `npm ci` | Repo ma lockfile, więc instalacja jest deterministyczna | Research |

## Scope

**In scope:** `.github/workflows/ci.yml`, backend restore/build/test, frontend install/typecheck/build.

**Out of scope:** deploy, sekrety, cache, artefakty, zmiany aplikacji, API, migracje i README.

## Architecture / Approach

Nowy workflow uruchamia niezależne joby na hostowanych runnerach GitHub. Każdy job konfiguruje swój runtime i wykonuje wyłącznie istniejące polecenia repozytorium.

## Phases at a Glance

| Phase | What it delivers | Key risk |
|-------|------------------|----------|
| 1. Workflow CI i jego weryfikacja | Działające joby backend i frontend | Różnica między lokalnym środowiskiem Windows a runnerem GitHub |

**Prerequisites:** branch `codex/slice-a`, dostęp do push/PR na GitHubie.
**Estimated effort:** jedna krótka sesja, jeden plik implementacyjny.

## Open Risks & Assumptions

- Zakładamy, że oficjalne akcje obsługują .NET 9 i wybraną stabilną wersję Node.js.
- Pierwszy rzeczywisty przebieg może być potwierdzony dopiero po wypchnięciu gałęzi.

## Success Criteria (Summary)

- Backend restore/build/test przechodzi w GitHub Actions.
- Frontend install/typecheck/build przechodzi w GitHub Actions.
- Workflow nie wymaga sekretów ani zewnętrznej bazy danych.
