---
change_id: github-ci
title: Automatyczna weryfikacja projektu na GitHub Actions
status: planned
created: 2026-07-17
updated: 2026-07-17
---

# Automatyczna weryfikacja projektu na GitHub Actions - Implementation Plan

## Overview

Dodać minimalny workflow CI, który przy zmianach kierowanych do `main` osobno weryfikuje backend .NET i frontend React. Slice zamyka brak infrastruktury jakości w publicznym repozytorium bez dodawania deploymentu ani usług zewnętrznych do działania aplikacji.

## Current State Analysis

Projekt ma kompletne lokalne komendy build/test/typecheck, ale GitHub nie uruchamia ich automatycznie. Nie istnieje `.github/workflows`, choć tech stack wskazuje GitHub Actions jako dostawcę CI.

## Desired End State

Push do `main` i pull request do `main` uruchamiają dwa niezależne joby. Job backendu wykonuje restore, build i test na .NET 9, a job frontendu instaluje zależności z lockfile i wykonuje typecheck oraz build na Node.js.

### Key Discoveries

- `AGENTS.md:27-39` zawiera sprawdzone komendy Windows i wymaga `UseAppHost=false` dla .NET.
- `frontend/package.json:6-10` udostępnia `typecheck` i `build`, a `frontend/package-lock.json` umożliwia `npm ci`.
- Testy backendu używają SQLite in-memory, więc workflow nie potrzebuje SQL Servera ani migracji.
- `.github/workflows/ci.yml` będzie nowym i jedynym plikiem implementacyjnym tego slice'a.

## What We're NOT Doing

- Deployment, publikowanie artefaktów, sekrety, cache, macierz systemów i ręczna promocja.
- Uruchamianie SQL Servera, LocalDB lub `dotnet ef database update` w CI.
- Zmiany kodu aplikacji, testów, API, DTO, migracji, README lub zależności.

## Implementation Approach

Utworzyć jeden czytelny workflow z triggerami ograniczonymi do `main` i dwoma jobami działającymi równolegle. Każdy job konfiguruje wyłącznie potrzebny runtime i wykonuje komendy już opisane w repozytorium.

## Phase 1: Workflow CI i jego weryfikacja

### Overview

Dodać kompletny workflow oraz zweryfikować lokalnie wszystkie komendy, które będzie wykonywać GitHub Actions.

### Changes Required

#### 1. GitHub Actions workflow

**File**: `.github/workflows/ci.yml` (new)

**Intent**: Automatycznie wykrywać regresje backendu i frontendu przed scaleniem zmian do `main`, zachowując oba obszary jako niezależne joby.

**Contract**: Workflow reaguje na `push` i `pull_request` do `main`. Job backendu używa .NET 9 i uruchamia restore, build z `--no-restore -p:UseAppHost=false` oraz test z `-p:UseAppHost=false`. Job frontendu używa stabilnego Node.js, ustawia `frontend/` jako katalog roboczy i uruchamia `npm ci --no-audit --no-fund`, `npm run typecheck` oraz `npm run build`.

### Success Criteria

#### Automated Verification

- Restore backendu przechodzi: `dotnet restore .\ai-finance-tracker.csproj`.
- Build backendu przechodzi: `dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false`.
- Testy backendu przechodzą: `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false`.
- Instalacja frontendu z lockfile przechodzi: `npm ci --no-audit --no-fund` w `frontend/`.
- Typecheck frontendu przechodzi: `npm run typecheck` w `frontend/`.
- Build frontendu przechodzi: `npm run build` w `frontend/`.

#### Manual Verification

- Po wypchnięciu gałęzi lub otwarciu PR GitHub pokazuje zakończone powodzeniem joby backend i frontend, bez żądania sekretów lub usługi bazodanowej.

**Implementation Note**: Po przejściu lokalnych komend wypchnąć gałąź/otworzyć PR i potwierdzić pierwszy rzeczywisty przebieg GitHub Actions przed zamknięciem change'a.

## Testing Strategy

### Unit Tests

- Nie dodajemy testów jednostkowych dla deklaratywnego workflow.

### Integration Tests

- Istniejący backendowy zestaw xUnit jest wykonywany przez job backendu.
- TypeScript i Vite są weryfikowane przez job frontendu.

### Manual Testing Steps

1. Wypchnąć `codex/slice-a` i otworzyć PR do `main`.
2. Sprawdzić, że pojawiły się dwa joby: backend i frontend.
3. Potwierdzić, że oba kończą się powodzeniem bez konfiguracji sekretów i bazy danych.

## Performance Considerations

Joby mają działać równolegle. Cache zależności pozostaje poza zakresem, dopóki czas wykonania nie okaże się realnym problemem.

## Migration Notes

Brak zmian bazy danych i migracji. Wycofanie polega na usunięciu nowego workflow.

## References

- Related research: `context/changes/github-ci/research.md`
- Commands: `AGENTS.md:27-39`
- Frontend scripts: `frontend/package.json:6-10`
- Stack decision: `context/foundation/tech-stack.md:17-24`

## Progress

> Convention: `- [ ]` pending, `- [x]` done. Append ` - <commit sha>` when a step lands. Do not rename step titles.

### Phase 1: Workflow CI i jego weryfikacja

#### Automated

- [x] 1.1 Restore backendu przechodzi: `dotnet restore .\ai-finance-tracker.csproj`.
- [x] 1.2 Build backendu przechodzi: `dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false`.
- [x] 1.3 Testy backendu przechodzą: `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false`.
- [x] 1.4 Instalacja frontendu z lockfile przechodzi: `npm ci --no-audit --no-fund` w `frontend/`.
- [x] 1.5 Typecheck frontendu przechodzi: `npm run typecheck` w `frontend/`.
- [x] 1.6 Build frontendu przechodzi: `npm run build` w `frontend/`.

#### Manual

- [ ] 1.7 Po wypchnięciu gałęzi lub otwarciu PR GitHub pokazuje zakończone powodzeniem joby backend i frontend, bez żądania sekretów lub usługi bazodanowej.
