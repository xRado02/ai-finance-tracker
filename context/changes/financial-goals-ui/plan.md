---
change_id: financial-goals-ui
title: Polski interfejs celow finansowych
status: draft
created: 2026-07-15
updated: 2026-07-15
---

# Plan: Polski interfejs celow finansowych

## Cel

Dodac do obecnego roboczego ekranu prosty obszar celow finansowych: tworzenie celu, lista celow oraz wizualny progress liczony przez backend.

## Stan obecny

- `frontend/src/api/financeApi.ts` obsluguje transakcje i ma wspolny helper JSON.
- `App.tsx` laduje kategorie i historie jako jeden stan gotowosci.
- Backend `financial-goals` udostepnia `GET /api/goals` i `POST /api/goals` z `currentAmount` i `progressPercentage`.
- Obecny UI korzysta z wlasnego CSS i jest juz po polsku.

## Zakres

### W zakresie

- Typed `GoalResponse` i `CreateGoalRequest`.
- `getGoals` i `createGoal` w `financeApi.ts`.
- Formularz celu z nazwa i kwota docelowa.
- Lista celow z kwota biezaca, kwota docelowa, procentem i progress barem.
- Odswiezanie celow po dodaniu i usunieciu transakcji oraz po utworzeniu celu.
- Stany ladowania, pustej listy i bledow po polsku.
- Responsywny layout bez nowej biblioteki UI.

### Poza zakresem

- Dashboard summary, kategorie wydatkow, edycja/usuwanie celow, auth, AI, cloud, import bankowy, custom kategorie, wykresy, notyfikacje i realtime.

## Decyzje techniczne

- App pobiera goals razem z kategoriami i transakcjami, a jeden refresh utrzymuje spojnosc ekranu.
- Formularz wysyla tylko `name` i `targetAmount`; `currentAmount` oraz progress pochodza z API.
- Progress bar ma stabilny tor i wartosc procentowa jest ograniczona do `0-100` po stronie UI jako ochrona prezentacji.
- Bledy przechodza przez istniejacy normalizer API i lokalne tlumaczenie komunikatow.
- Komponenty `GoalForm` i `GoalList` pozostaja osobne od `TransactionForm` i `TransactionHistory`.

## Fazy

### Phase 1: Goals API client and app state

Dodac typy, metody API i wpiac goals do ladowania oraz odswiezania stanu App.

### Phase 2: Goal form and progress list

Dodac polskie komponenty formularza/listy i CSS progress baru.

### Phase 3: Verification and change closeout

Uruchomic typecheck/build, sprawdzic integracje z transakcjami i zapisac Progress, SHA oraz status change'a.

## Success Criteria

### Automated Verification

- `npm run typecheck` w `frontend/` przechodzi.
- `npm run build` w `frontend/` przechodzi.

### Manual Verification

- Uzytkownik widzi polski formularz celu i pusta liste, gdy nie ma celow.
- Utworzenie celu pokazuje go na liscie z kwotami, procentem i paskiem postepu.
- Dodanie lub usuniecie transakcji odswieza progress celu.
- Niepoprawna nazwa lub kwota pokazuje czytelny blad po polsku.
- UI nie zawiera dashboardu summary ani edycji/usuwania celow.

## Testing Strategy

- Frontend typecheck/build zgodnie z obecnym MVP.
- Ręczny smoke test obejmuje utworzenie celu i zmianę progressu przez transakcję.
- Bez e2e i bez nowej biblioteki testowej.

## References

- `context/foundation/prd.md` FR-009, FR-010
- `context/foundation/roadmap.md` S-03 Financial Goal Progress
- `context/changes/financial-goals/plan.md`
- `frontend/src/App.tsx`
- `frontend/src/api/financeApi.ts`
- `frontend/src/labels.ts`
- `frontend/src/App.css`

## Progress

> Convention: `- [ ]` pending, `- [x]` done. Append `- <commit sha>` when a step lands. Do not rename step titles.

### Phase 1: Goals API client and app state

#### Automated

- [x] 1.1 Typy i metody goals API sa obecne
- [x] 1.2 App laduje goals i odswieza je po zmianie transakcji
- [x] 1.3 `npm run typecheck` przechodzi

#### Manual

- [x] 1.4 Frontend korzysta z istniejacego kontraktu `/api/goals` bez rozszerzania backendu

### Phase 2: Goal form and progress list

#### Automated

- [ ] 2.1 `npm run typecheck` przechodzi po dodaniu komponentow celow
- [ ] 2.2 `npm run build` przechodzi po dodaniu komponentow celow

#### Manual

- [ ] 2.3 Formularz celu, lista, progress bar i komunikaty sa po polsku
- [ ] 2.4 Utworzenie celu pokazuje dane zwrocone przez API
- [ ] 2.5 Zmiana transakcji odswieza progress celu

### Phase 3: Verification and change closeout

#### Automated

- [ ] 3.1 `npm run typecheck` przechodzi
- [ ] 3.2 `npm run build` przechodzi

#### Manual

- [ ] 3.3 Reczny smoke test goals UI przechodzi
- [ ] 3.4 Dokumentacja zmiany i Progress sa kompletne
