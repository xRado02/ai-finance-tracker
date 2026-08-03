# Launcher Windows

Launcher pozwala uruchomić aplikację bez ręcznego otwierania dwóch terminali.
Nie zmienia danych, nie uruchamia automatycznie migracji i nie otwiera
przeglądarki.

## Uruchomienie

Kliknij dwukrotnie:

`Start AI Finance Tracker.cmd`

Launcher:

1. otwiera terminal backendu,
2. otwiera terminal frontendu,
3. uruchamia oba procesy odpowiednimi komendami.

Przeglądarkę otwórz ręcznie pod adresem pokazanym w terminalu Vite, zwykle:

`http://localhost:5173`

## Zatrzymanie

W obu otwartych terminalach naciśnij `Ctrl+C` albo zamknij ich okna.

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

## Porty

- backend: `5218`,
- frontend: zwykle `5173`; jeśli port jest zajęty, Vite pokaże inny adres.

Ewentualny błąd pozostaje widoczny w odpowiednim terminalu, dzięki czemu nie
trzeba szukać ukrytych logów.
