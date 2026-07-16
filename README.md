# AI Finance Tracker

Lokalna aplikacja open source do ręcznego zarządzania finansami osobistymi. Umożliwia pracę miesiąc po miesiącu, zapisywanie przychodów i wydatków, śledzenie celów oraz przeglądanie prostych podsumowań.

## Funkcje MVP

- lokalny profil domyślny bez logowania,
- polski interfejs z osobnymi sekcjami: Dashboard, Transakcje, Stałe transakcje, Cele finansowe i Ustawienia,
- wybór miesiąca i roku oraz historia ograniczona do wybranego okresu,
- dodawanie i usuwanie przychodów oraz wydatków,
- kategorie z fallbackiem `Inne`,
- stałe przychody i wydatki generowane ręcznie za wybrany miesiąc bez duplikatów,
- saldo miesiąca i saldo całkowite z uwzględnieniem stanu początkowego konta,
- podsumowanie kategorii przychodów i wydatków,
- cele finansowe z postępem i prognozą terminu osiągnięcia,
- dane przechowywane lokalnie w SQL Server LocalDB.

Poza zakresem MVP pozostają AI, auth, chmura, import bankowy, wielu użytkowników, custom kategorie, edycja transakcji, zaawansowane wykresy, notyfikacje, realtime i background joby.

## Stack

- Backend: ASP.NET Core Web API, .NET 9, Entity Framework Core, SQL Server
- Frontend: React, TypeScript, Vite, własny CSS
- Testy backendu: xUnit, SQLite in-memory

## Wymagania

- .NET SDK 9
- SQL Server LocalDB
- Node.js i npm

## Uruchomienie backendu

W katalogu głównym repozytorium:

```powershell
dotnet restore .\ai-finance-tracker.csproj
dotnet ef database update --project .\ai-finance-tracker.csproj
dotnet run --project .\ai-finance-tracker.csproj --launch-profile http -p:UseAppHost=false
```

Opcja `UseAppHost=false` omija problem blokowania pliku `.exe` na Windows. Backend działa pod `http://localhost:5218`.

Po dodaniu nowej migracji uruchom ponownie `dotnet ef database update` przed startem aplikacji.

## Uruchomienie frontendu

W drugim terminalu:

```powershell
cd frontend
npm install --no-audit --no-fund
npm run dev
```

Otwórz adres pokazany przez Vite. Frontend korzysta z proxy `/api` do lokalnego backendu.

## Weryfikacja

Backend, z katalogu głównego:

```powershell
dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false
dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false
```

Frontend, z katalogu `frontend/`:

```powershell
npm run typecheck
npm run build
```

## API

- `GET /api/categories`
- `GET /api/transactions?year=2026&month=7`
- `POST /api/transactions`
- `DELETE /api/transactions/{id}`
- `GET /api/profile/settings`
- `PATCH /api/profile/settings`
- `GET /api/goals`
- `POST /api/goals`
- `GET /api/goals/forecast`
- `GET /api/dashboard/summary`
- `GET /api/dashboard/monthly-summary?year=2026&month=7`
- `GET /api/recurring-transactions`
- `POST /api/recurring-transactions`
- `PATCH /api/recurring-transactions/{id}/status`
- `POST /api/recurring-transactions/generate-current-month?year=2026&month=7`

Wszystkie dane finansowe należą do domyślnego profilu lokalnego. Aplikacja nie wysyła ich do usług zewnętrznych.

## Licencja

Projekt jest dostępny na licencji MIT. Szczegóły znajdują się w pliku [LICENSE](LICENSE).
