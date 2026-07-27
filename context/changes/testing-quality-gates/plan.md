---
change_id: testing-quality-gates
title: Lokalne bramki jakości
status: planned
created: 2026-07-27
updated: 2026-07-27
---

# Lokalne bramki jakości - Implementation Plan

## Overview

Dodać jeden powtarzalny punkt wejścia dla lokalnej weryfikacji backendu i
frontendu, zgodny z istniejącym workflow GitHub Actions.

## Current State Analysis

`.github/workflows/ci.yml` uruchamia backend restore/build/test oraz frontend
install/typecheck/build. Lokalne komendy są opisane w `AGENTS.md` i `README.md`,
ale trzeba wywoływać je osobno. Repozytorium nie używa Husky, lint-staged ani
wspólnego skryptu jakości.

## Desired End State

- Jedna komenda PowerShell uruchamia backend restore/build/test i frontend
  typecheck/build.
- Każdy niezerowy kod wyjścia natychmiast zatrzymuje skrypt.
- Dokumentacja jasno rozróżnia obowiązkowe CI od opcjonalnego lokalnego hooka.
- Nie dochodzą zależności ani automatyczna zmiana lokalnej konfiguracji Git.

## What We're NOT Doing

- Nowy workflow CI lub duplikowanie obecnego.
- Instalowanie Husky, lint-staged, formattera lub lintera.
- Automatyczne ustawianie `core.hooksPath`.
- Zmiany kodu aplikacji, API, testów lub migracji.

## Implementation Approach

Utworzyć `scripts/verify.ps1` kompatybilny z typowym PowerShell na Windows.
Skrypt jawnie sprawdza `$LASTEXITCODE`, używa katalogu repozytorium niezależnie
od miejsca wywołania i ma opcję `-SkipRestore` dla szybkiego ponowienia.

## Phase 1: Wspólny lokalny quality gate

### Changes Required

#### 1. Skrypt weryfikacyjny

**File**: `scripts/verify.ps1`

**Contract**: Uruchamia restore projektu API i testów, build API, pełny backend
test oraz frontend typecheck/build. Kończy się błędem przy pierwszej awarii.

#### 2. Dokumentacja

**File**: `docs/quality-gates.md`

**Contract**: Opisuje użycie skryptu, zgodność z CI oraz świadomą decyzję, by
nie instalować hooka automatycznie.

### Success Criteria

#### Automated Verification

- `powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify.ps1`
  przechodzi.
- `git diff --check` przechodzi.

#### Manual Verification

Brak. Skrypt sam uruchamia wszystkie deklarowane bramki.

## Migration Notes

Brak zmian bazy danych.

## Progress

> Convention: `- [ ]` pending, `- [x]` done. Append ` - <commit sha>` when a step lands. Do not rename step titles.

### Phase 1: Wspólny lokalny quality gate

#### Automated

- [x] 1.1 `scripts/verify.ps1` uruchamia pełny zestaw lokalnych bramek. - 8e1cf86
- [x] 1.2 Dokumentacja opisuje CI i opcjonalny model hooka bez nowych zależności. - 8e1cf86
- [x] 1.3 Skrypt oraz `git diff --check` przechodzą. - 8e1cf86
