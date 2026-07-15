# AI Finance Tracker

Lokalne MVP do ręcznego zapisywania finansów, przeglądania historii, śledzenia celów i szybkiego podsumowania sytuacji finansowej.

## Zakres MVP

- domyślny profil lokalny bez logowania,
- dodawanie przychodów i wydatków,
- kategorie z fallbackiem `Inne`,
- historia i usuwanie transakcji,
- dashboard: przychody, wydatki, saldo i największe kategorie wydatków,
- tworzenie celów finansowych i śledzenie ich progressu,
- dane przechowywane lokalnie w SQL Server LocalDB.

Poza MVP pozostają AI, logowanie, chmura, import bankowy, wielu użytkowników, custom kategorie, edycja transakcji, zaawansowane wykresy, notyfikacje i realtime.

## Wymagania

- .NET SDK 9,
- SQL Server LocalDB,
- Node.js i npm.

## Uruchomienie backendu

W katalogu głównym repozytorium:

```powershell
dotnet restore .\ai-finance-tracker.csproj
dotnet ef database update --project .\ai-finance-tracker.csproj
dotnet run --project .\ai-finance-tracker.csproj --launch-profile http -p:UseAppHost=false
```

`UseAppHost=false` omija problem blokowania pliku `.exe` na Windows. Backend działa pod `http://localhost:5218`.

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

Backend:

```powershell
dotnet build .\ai-finance-tracker.csproj --no-restore -p:UseAppHost=false
dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj -p:UseAppHost=false
```

Frontend, uruchamiane z `frontend/`:

```powershell
npm run typecheck
npm run build
```

## API

- `GET /api/categories`
- `GET /api/transactions`
- `POST /api/transactions`
- `DELETE /api/transactions/{id}`
- `GET /api/goals`
- `POST /api/goals`
- `GET /api/dashboard/summary`

Wszystkie dane finansowe należą do domyślnego profilu lokalnego. API nie wysyła ich do usług zewnętrznych.
