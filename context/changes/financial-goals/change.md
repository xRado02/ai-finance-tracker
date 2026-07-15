---
change_id: financial-goals
title: Cele finansowe w lokalnym API
status: implemented
created: 2026-07-15
updated: 2026-07-15
archived_at: null
---

## Notes

- Dodac persystencje celow finansowych dla default local profile.
- Dodac `GET /api/goals` i `POST /api/goals`.
- Progress celu liczyc z aktualnego salda transakcji; bez osobnego endpointu aktualizacji.
- Nie dodawac UI celow w tym change'u; frontend bedzie osobnym `financial-goals-ui`.
- Zachowac local-first, brak auth, chmury, AI i custom kategorii.
