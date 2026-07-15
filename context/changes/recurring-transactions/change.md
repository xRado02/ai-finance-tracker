---
change_id: recurring-transactions
title: Stałe przychody i wydatki
status: implemented
created: 2026-07-15
updated: 2026-07-15
archived_at: null
---

## Notes

- Dodac definicje stalych transakcji dla default local profile.
- Generowanie odbywa sie recznie przyciskiem dla biezacego miesiaca; bez background joba.
- Wygenerowana transakcja ma jawne powiazanie z definicja, aby blokada duplikatu byla jednoznaczna.
- Nie zmieniac istniejacego CRUD transakcji ani goals poza potrzebnym nullable FK.
