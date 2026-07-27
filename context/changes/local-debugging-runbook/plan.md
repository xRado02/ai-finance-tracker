---
change_id: local-debugging-runbook
title: Lokalny runbook debugowania
status: implemented
created: 2026-07-27
updated: 2026-07-27
---

# Lokalny runbook debugowania - Implementation Plan

## Overview

Udokumentować powtarzalną diagnostykę backendu, frontendu, EF Core, LocalDB,
migracji, portów i lokalnego smoke testu.

## Scope

- `docs/debugging.md`
- brak zmian aplikacji, zależności, bazy, CI i zewnętrznego monitoringu

## Success Criteria

- Checklista zawiera wszystkie obszary wymagane przez M3L5.
- Komendy są zgodne z Windows i aktualnym repozytorium.
- Dokument ostrzega przed usuwaniem normalnej bazy użytkownika.

## Progress

> Convention: `- [ ]` pending, `- [x]` done. Append ` - <commit sha>` when a step lands. Do not rename step titles.

### Phase 1: Runbook

#### Automated

- [x] 1.1 Dokument obejmuje logi backendu, konsolę i Network frontendu.
- [x] 1.2 Dokument obejmuje EF Core, migracje, LocalDB i porty.
- [x] 1.3 Dokument obejmuje smoke test i odzyskiwanie po braku backendu.
