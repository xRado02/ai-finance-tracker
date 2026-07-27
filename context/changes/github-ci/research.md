---
topic: github-ci
researcher: Codex
date: 2026-07-17
---

# Research: github-ci

## Stan obecny

- Repozytorium jest publiczne na GitHubie, a główna gałąź to `main`.
- Backend to ASP.NET Core na .NET 9 w `ai-finance-tracker.csproj`; testy są w `tests/AiFinanceTracker.Tests/AiFinanceTracker.Tests.csproj`.
- Frontend ma `frontend/package.json`, śledzony `frontend/package-lock.json` oraz skrypty `typecheck` i `build`.
- `AGENTS.md` podaje sprawdzone komendy Windows z `-p:UseAppHost=false`.
- `context/foundation/tech-stack.md` wskazuje GitHub Actions jako dostawcę CI.
- W repozytorium nie ma katalogu `.github/workflows` ani istniejącego workflow CI.

## Decyzje

- Jeden workflow z niezależnymi jobami backend i frontend, uruchamiany na `push` i `pull_request` do `main`.
- Oficjalne akcje `checkout`, `setup-dotnet` i `setup-node`; wersje runtime zgodne z projektem.
- Backend wykonuje restore, build i test z `UseAppHost=false`; frontend wykonuje deterministyczne `npm ci`, typecheck i build.
- CI nie uruchamia SQL Servera ani migracji: testy backendu korzystają z izolowanej bazy SQLite in-memory.
- Slice nie zmienia kodu aplikacji, API, kontraktów, migracji ani README.

## Dowody

- `AGENTS.md:27-39` - obowiązujące komendy restore/build/test i frontend.
- `frontend/package.json:6-10` - dostępne skrypty npm.
- `context/foundation/tech-stack.md:17-24` - GitHub Actions jako wybrany kierunek CI.
