# Launcher Windows

Launcher pozwala uruchomić aplikację bez ręcznego otwierania dwóch terminali.
Nie zmienia danych ani nie uruchamia automatycznie migracji.

## Uruchomienie

Kliknij dwukrotnie:

`Start AI Finance Tracker.cmd`

Launcher:

1. sprawdza dostępność .NET, npm i zależności frontendu,
2. uruchamia backend i frontend w ukrytych procesach,
3. czeka na gotowość obu części,
4. otwiera `http://127.0.0.1:5173` w domyślnej przeglądarce.

## Zatrzymanie

Kliknij dwukrotnie:

`Stop AI Finance Tracker.cmd`

Zatrzymywane są tylko procesy zapisane przez launcher. Ręcznie uruchomione
procesy `dotnet` i `node` nie są zamykane.

## Pierwsze uruchomienie

Jeśli frontend nie ma jeszcze zależności:

```powershell
cd frontend
npm install --no-audit --no-fund
```

Jeśli lokalna baza wymaga migracji:

```powershell
dotnet ef database update --project .\ai-finance-tracker.csproj
```

## Diagnostyka

W przypadku błędu launcher pokazuje komunikat. Logi znajdują się w:

`%LOCALAPPDATA%\AI Finance Tracker\launcher`

Launcher korzysta ze stałych portów:

- backend: `5218`,
- frontend: `5173`.
