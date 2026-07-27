---
topic: frontend-error-recovery
researcher: Codex
date: 2026-07-17
---

# Research: frontend-error-recovery

## Stan obecny

- `frontend/src/App.tsx:37-52` definiuje wspólny `ApiStatus` dla loading, ready i error.
- `frontend/src/App.tsx:73-99` ładuje wszystkie dane finansowe przez `loadFinanceData` i mapuje błąd na polski komunikat.
- `frontend/src/App.tsx:201-206` pokazuje komunikat błędu, ale nie oferuje ponowienia; odzyskanie wymaga odświeżenia strony.
- `frontend/src/App.css:774-795` zawiera finalne style paska statusu i wariantu błędu.
- Frontend nie ma frameworka testów jednostkowych; ma typecheck i build.
- Backend i kontrakty API nie wymagają zmian, ponieważ retry powtarza istniejący zestaw żądań dla wybranego okresu.

## Decyzje

- Retry pozostaje lokalnie w `App.tsx` i ponownie używa `loadFinanceData(selectedPeriod)`.
- Po kliknięciu stan natychmiast przechodzi na loading, co usuwa lub wyłącza akcję i zapobiega równoległym kliknięciom.
- Komunikat i przycisk są po polsku; nie dodajemy toastów, automatycznych retry, pollingu ani globalnego systemu błędów.
- Styl akcji jest dopasowany w `App.css` bez biblioteki komponentów.

## Dowody

- `frontend/src/App.tsx:63-103` - stan, funkcja ładowania i efekt zależny od okresu.
- `frontend/src/App.tsx:201-206` - obecny status bez akcji odzyskiwania.
- `frontend/package.json:6-10` - dostępne komendy typecheck/build.
