---
change_id: mvp-polish
title: Dokumentacja i finalne spięcie MVP
status: draft
created: 2026-07-15
updated: 2026-07-15
---

# Plan: Dokumentacja i finalne spięcie MVP

## Cel

Domknąć lokalne MVP przez aktualną instrukcję uruchomienia, uporządkowanie onboarding docs i końcową weryfikację całego przepływu bez zmiany funkcji produktu.

## Stan obecny

- Backend ma transakcje, kategorie, goals i dashboard summary oraz migracje EF.
- Frontend działa jako Vite + React + TypeScript i ma pełny polski ekran roboczy.
- README nie istnieje.
- `AGENTS.md` opisuje tylko wcześniejsze endpointy transakcji.
- `.bootstrap-scaffold.http` wskazuje na usunięty weather endpoint, a `api-smoke*.log` są ignorowanymi artefaktami.

## Zakres

### W zakresie

- README po polsku: wymagania, migracja, backend, frontend, testy i zakres MVP.
- Aktualizacja `AGENTS.md` o goals, dashboard summary, migrację i komendy Windows.
- Usunięcie starego `.bootstrap-scaffold.http` i ignorowanych `api-smoke*.log`.
- Końcowy restore/build/test/typecheck/build oraz weryfikacja migracji.
- Finalny ręczny smoke test całej aplikacji.

### Poza zakresem

- Nowe endpointy, nowe funkcje produktu, auth, AI, cloud, bank import, custom kategorie, wykresy, realtime i notyfikacje.

## Fazy

### Phase 1: Documentation and cleanup

Napisać README, zaktualizować AGENTS i usunąć artefakty scaffold/smoke.

### Phase 2: Final verification

Uruchomić pełny zestaw komend oraz sprawdzić spójność dokumentacji i zakresu MVP.

### Phase 3: Final smoke and change closeout

Wykonać ręczny smoke całego MVP, zapisać Progress, SHA, status i review implementacji.

## Success Criteria

### Automated Verification

- `dotnet restore .\ai-finance-tracker.csproj` przechodzi.
- `dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false` przechodzi.
- `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false` przechodzi.
- `npm run typecheck` w `frontend/` przechodzi.
- `npm run build` w `frontend/` przechodzi.
- README i AGENTS zawierają aktualne komendy oraz endpointy.

### Manual Verification

- Po migracji backend i frontend uruchamiają się lokalnie.
- Dashboard pokazuje metryki, kategorie i cele.
- Użytkownik może dodać przychód, wydatek i cel.
- Usunięcie transakcji aktualizuje historię, saldo, kategorie i progress celu.
- Cały interfejs użytkowy jest po polsku.

## Testing Strategy

- Pełny backend test suite i frontend build gates.
- Ręczny smoke test na dwóch terminalach, bez automatycznego uruchamiania procesów przez job/start.

## References

- `context/foundation/prd.md`
- `context/foundation/roadmap.md`
- `context/foundation/tech-stack.md`
- `context/changes/transaction-delete-and-polish/plan.md`
- `context/changes/financial-goals/plan.md`
- `context/changes/financial-goals-ui/plan.md`
- `context/changes/dashboard-summary/plan.md`
- `AGENTS.md`
- `appsettings.Development.json`

## Progress

> Convention: `- [ ]` pending, `- [x]` done. Append `- <commit sha>` when a step lands. Do not rename step titles.

### Phase 1: Documentation and cleanup

#### Automated

- [x] 1.1 README opisuje uruchomienie, migrację, testy i zakres MVP - 335ae97
- [x] 1.2 AGENTS opisuje aktualną strukturę i komendy - 335ae97
- [x] 1.3 Stary scaffold HTTP i logi smoke są usunięte - 335ae97

#### Manual

- [x] 1.4 Dokumentacja nie obiecuje funkcji poza MVP - 335ae97

### Phase 2: Final verification

#### Automated

- [x] 2.1 Backend restore/build/test przechodzi - f386477
- [x] 2.2 Frontend typecheck/build przechodzi - f386477
- [x] 2.3 Migracje zawierają InitialCreate i AddFinancialGoals - f386477

#### Manual

- [x] 2.4 Komendy z README odpowiadają lokalnemu workflow - f386477

### Phase 3: Final smoke and change closeout

#### Automated

- [x] 3.1 Finalny zakres MVP jest spójny z PRD i roadmapą
- [x] 3.2 Progress, SHA i status change'a są kompletne

#### Manual

- [x] 3.3 Finalny smoke test całej aplikacji przechodzi
