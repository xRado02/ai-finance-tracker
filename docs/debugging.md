# Lokalne debugowanie

Ten runbook dotyczy lokalnego MVP. Nie wymaga Sentry, zewnętrznego monitoringu
ani wysyłania danych finansowych poza komputer użytkownika.

## 1. Uruchomienie w dwóch terminalach

Backend, z katalogu głównego:

```powershell
dotnet run --project .\ai-finance-tracker.csproj --launch-profile http -p:UseAppHost=false
```

Frontend:

```powershell
cd frontend
npm run dev
```

Backend powinien nasłuchiwać pod `http://localhost:5218`. Adres frontendu podaje
Vite; domyślnie jest to `http://localhost:5173`.

## 2. Logi backendu

1. Zacznij od pierwszego wyjątku, nie od kolejnych błędów będących jego skutkiem.
2. Zapisz endpoint, status HTTP oraz nazwę wyjątku.
3. Dla błędów EF/SQL przeczytaj pierwszą linię `SqlException` i nazwę
   tabeli/kolumny/constraintu.
4. Nie ukrywaj wyjątku ogólnym `try/catch` tylko po to, by aplikacja wystartowała.

Typowe rozpoznanie:

- `Invalid object name '...'` lub `Invalid column name '...'` zwykle oznacza,
  że kod i lokalny schemat są na różnych migracjach.
- `MSB3021` lub `Access denied` dla pliku `.exe` oznacza problem apphosta lub
  działający proces. Użyj zalecanej komendy z `-p:UseAppHost=false`.
- Błąd constraintu podczas migracji wymaga sprawdzenia kolejności migracji i
  istniejących danych. Nie usuwaj bazy w ciemno.

## 3. Frontend: Console i Network

W DevTools:

1. Otwórz `Console` i znajdź pierwszy czerwony błąd.
2. W `Network` przefiltruj żądania po `/api`.
3. Sprawdź metodę, pełny URL, status oraz response body.
4. Dla `400` odczytaj klucze `ValidationProblem`.
5. Dla `404` potwierdź identyfikator i kontekst domyślnego profilu.
6. Dla `500` wróć do terminala backendu; frontend zwykle pokazuje tylko skutek.
7. Dla `(failed)` lub `ERR_CONNECTION_REFUSED` sprawdź proces backendu, port i
   proxy Vite.

Nie wklejaj do issue pełnych danych finansowych ani connection stringów.

## 4. Migracje i EF Core

Najpierw sprawdź listę migracji:

```powershell
dotnet ef migrations list --project .\ai-finance-tracker.csproj
```

Następnie zastosuj brakujące migracje:

```powershell
dotnet ef database update --project .\ai-finance-tracker.csproj
```

Po aktualizacji uruchom backend ponownie. Jeśli błąd pozostaje:

1. potwierdź używany connection string w `appsettings.Development.json`,
2. sprawdź, czy polecenie wykonano w tym samym repo/branchu,
3. sprawdź ostatnią zastosowaną migrację w tabeli `__EFMigrationsHistory`,
4. porównaj ją z `dotnet ef migrations list`.

Nie uruchamiaj `database drop`, nie usuwaj plików LocalDB i nie cofaj migracji na
normalnej bazie użytkownika bez kopii oraz osobnej decyzji.

## 5. LocalDB

Stan instancji:

```powershell
sqllocaldb info
sqllocaldb info MSSQLLocalDB
```

Uruchomienie instancji:

```powershell
sqllocaldb start MSSQLLocalDB
```

Jeśli instancja nie istnieje lub nie startuje, najpierw sprawdź instalację SQL
Server Express LocalDB. Nie twórz drugiej produkcyjnej nazwy bazy jako
obejścia problemu z migracją.

## 6. Porty i procesy

Sprawdź porty aplikacji:

```powershell
Get-NetTCPConnection -LocalPort 5218,5173 -ErrorAction SilentlyContinue |
    Select-Object LocalAddress, LocalPort, State, OwningProcess
```

Identyfikacja procesu:

```powershell
Get-Process -Id <PID>
```

Najpierw zamknij właściwy terminal procesu. Nie zabijaj wszystkich procesów
`dotnet` lub `node`, bo mogą należeć do innych projektów.

## 7. Gdy frontend nie łączy się z backendem

1. Wejdź bezpośrednio na `http://localhost:5218/api/categories`.
2. Jeśli adres nie odpowiada, napraw backend przed frontendem.
3. Jeśli API odpowiada, sprawdź proxy `/api` w `frontend/vite.config.ts`.
4. W Network potwierdź, że frontend wysyła żądanie do własnego hosta z prefiksem
   `/api`, a Vite przekazuje je na port `5218`.
5. Po powrocie backendu kliknij `Spróbuj ponownie`.
6. Potwierdź, że dane wróciły bez odświeżenia i wybrany miesiąc się nie zmienił.

## 8. Smoke test przed commitem

Najpierw automatyczne bramki:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify.ps1
```

Następnie krótki manualny smoke:

1. Otwórz Dashboard i zmień miesiąc.
2. Dodaj przychód lub wydatek do wybranego miesiąca.
3. Potwierdź wpis w historii oraz zmianę miesięcznego podsumowania.
4. Usuń wpis i potwierdź ponowne przeliczenie.
5. Wygeneruj recurring dwa razy dla jednego miesiąca i sprawdź brak duplikatu.
6. Otwórz cele oraz ustawienia i sprawdź, czy dane się ładują.
7. Wyłącz backend, sprawdź polski stan błędu, uruchom backend i użyj retry.

Jeśli którykolwiek krok nie przechodzi, nie pushuj zmiany i nie usuwaj testu,
który ujawnił regresję.
