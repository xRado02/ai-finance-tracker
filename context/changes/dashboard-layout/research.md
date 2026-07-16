# Research: dashboard-layout

## Zakres zbadany

- `frontend/src/App.tsx` już ładuje kategorie, transakcje wybranego miesiąca, miesięczne podsumowanie, dashboard globalny, recurring, goals, forecast i ustawienia profilu.
- `frontend/src/components/PeriodPicker.tsx` jest wspólnym wyborem okresu; jego stan znajduje się w `App.tsx` i powinien pozostać wspólny dla Dashboard, Transakcji i recurring.
- `DashboardSummary.tsx`, `TransactionForm.tsx`, `TransactionHistory.tsx`, `RecurringTransactionPanel.tsx`, `GoalForm.tsx`, `GoalList.tsx` i `SettingsPanel.tsx` są już gotowymi granicami komponentów.
- Backend ma stabilne endpointy `GET /api/dashboard/summary`, `GET /api/dashboard/monthly-summary`, `GET/POST /api/transactions`, `GET/POST /api/goals`, `GET /api/goals/forecast`, recurring oraz `GET/PATCH /api/profile/settings`. Ten change nie wymaga nowej logiki backendowej.
- Aktualny sidebar używa kotwic `#overview`, `#transactions`, `#recurring`, `#goals`; można zastąpić je stanem sekcji bez wprowadzania React Routera.

## Decyzje

- Nawigacja będzie lokalnym stanem `activeSection` w `App.tsx`; bez zależności routera i bez zmiany kontraktów API.
- `PeriodPicker` pozostaje w topbarze jako wspólny kontekst. Zmiana okresu przeładowuje dane tak jak obecnie, niezależnie od aktywnej sekcji.
- Każda sekcja otrzyma jeden główny widok: dashboard, transakcje, recurring, goals albo ustawienia. Istniejące komponenty pozostają źródłem logiki formularzy i danych.
- Dashboard pokaże jednocześnie saldo całkowite z `dashboard.balance`, saldo miesiąca z `monthlySummary.balance`, kategorie i istniejący postęp/prognozę celów.

## Poza zakresem

- Nowe endpointy, przebudowa goals, goal-forecast lub recurring.
- React Router, auth, AI, cloud, import bankowy, zaawansowane wykresy i nowe funkcje domenowe.
