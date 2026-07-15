---
change_id: transaction-delete-and-polish
title: Usuwanie transakcji i spolszczenie obecnego przeplywu
status: impl_reviewed
created: 2026-07-15
updated: 2026-07-15
archived_at: null
---

## Notes

- Dodac `DELETE /api/transactions/{id}` tylko dla default local profile.
- Zwracac 404 dla nieistniejacej transakcji i nie usuwac transakcji z innych profili.
- Dodac przycisk usuwania w historii, potwierdzenie i odswiezenie historii.
- Spolszczyc caly obecny frontend bez dodawania dashboardu, goals, auth, AI, chmury, custom kategorii ani edycji.
- Zachowac obecny stack i lokalny-first model danych.
