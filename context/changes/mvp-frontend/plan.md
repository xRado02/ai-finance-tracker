# Frontend dla transakcji MVP Implementation Plan

## Overview

Ten change dodaje pierwszy frontend React dla AI Finance Tracker, dopasowany do API, ktore juz istnieje w backendzie. Celem jest szybki lokalny przeplyw: uzytkownik widzi kategorie, dodaje przychod albo wydatek i odswieza historie transakcji bez dodawania nowych funkcji domenowych po stronie backendu.

## Current State Analysis

Projekt ma ASP.NET Core Web API na .NET 9 oraz testy xUnit. Backend ma juz lokalna persystencje, domyslny lokalny profil, seed kategorii i trzy endpointy finansowe:

- `GET /api/categories`
- `POST /api/transactions`
- `GET /api/transactions`

Frontend jeszcze nie istnieje. PRD opisuje szerszy MVP z dashboardem i celami finansowymi, ale obecny backend nie wystawia API dla goals ani dashboard summaries. Ten plan celowo trzyma frontend przy aktualnym kontrakcie backendu, zeby nie tworzyc atrap i nie rozszerzac zakresu ukradkiem.

## Desired End State

Po zakonczeniu change'a repo ma katalog `frontend/` z aplikacja Vite + React + TypeScript. Aplikacja pokazuje jeden ekran roboczy: formularz dodania transakcji, liste kategorii uzywana w formularzu oraz historie ostatnich transakcji pobrana z backendu. UI komunikuje sie z backendem przez relatywne sciezki `/api/...`, a lokalny dev server Vite proxy'uje je do ASP.NET Core API.

### Key Discoveries:

- `Program.cs` konfiguruje JSON string enum converter, wiec frontend powinien wysylac i odbierac `type` jako `"Income"` albo `"Expense"`.
- `Contracts/FinanceContracts.cs` definiuje publiczne DTO dla kategorii i transakcji; frontend powinien odwzorowac te pola bez ekspozycji EF entities.
- `Endpoints/FinanceEndpoints.cs` waliduje kwote, date, opis, typ i zgodnosc kategorii z typem; UI powinien pomagac wybrac poprawna kategorie i pokazywac bledy z API.
- `Properties/launchSettings.json` wystawia lokalny backend na `http://localhost:5218`, co jest naturalnym celem dla Vite proxy w development.
- PRD i AGENTS.md zabraniaja AI, chmury, auth, bank integrations i multi-user auth w MVP bez aktualizacji dokumentow fundamentu.

## What We're NOT Doing

- Nie dodajemy goals ani API/UI do celow finansowych.
- Nie dodajemy dashboardu, podsumowan, struktury wydatkow ani top kategorii.
- Nie dodajemy auth, kont uzytkownika ani wyboru profilu.
- Nie dodajemy AI, bank integrations, importu transakcji, chmury, realtime ani background jobs.
- Nie dodajemy edycji/usuwania transakcji.
- Nie dodajemy custom kategorii ani category management UI.
- Nie dodajemy biblioteki komponentow UI.
- Nie dodajemy automatycznych testow e2e w tym change'u.

## Implementation Approach

Utworzyc samodzielny frontend w `frontend/` oparty o Vite, React i TypeScript. Frontend ma miec cienki klient API, typy zgodne z backendowymi DTO, prosty stan komponentowy i jeden ekran roboczy. Backend pozostaje zrodlem prawdy dla walidacji i persystencji; frontend tylko ulatwia poprawne wyslanie danych i czytelne pokazanie odpowiedzi.

## Critical Implementation Details

Frontend powinien uzywac relatywnych adresow `/api/categories` i `/api/transactions`, a nie twardo wpisanego pelnego URL API. W development Vite proxy przekieruje `/api` do `http://localhost:5218`, co pozwala uniknac CORS i utrzymuje lokalny charakter aplikacji.

Kategoria `Other` moze pasowac do obu typow transakcji, a pozostale kategorie sa zgodne z `appliesTo`. UI powinien filtrowac liste kategorii po wybranym typie, ale nie moze polegac tylko na filtrze frontendowym; bledy walidacji z backendu nadal musza byc widoczne dla uzytkownika.

## Phase 1: Frontend Scaffold And Local API Wiring

### Overview

Utworzyc katalog `frontend/`, podstawowy projekt Vite + React + TypeScript, skrypty weryfikacyjne i konfiguracje proxy do istniejacego backendu.

### Changes Required:

#### 1. Vite React project

**File**: `frontend/package.json`, `frontend/index.html`, `frontend/src/main.tsx`, `frontend/src/App.tsx`

**Intent**: Dodac minimalny frontendowy projekt bez mieszania go z projektem API i bez dodawania biblioteki komponentow.

**Contract**: Projekt uzywa React + TypeScript. `package.json` zawiera skrypty `dev`, `build`, `typecheck` i `preview`. Pierwszy render moze byc szkieletem aplikacji, ale musi budowac sie w TypeScript.

#### 2. Vite configuration

**File**: `frontend/vite.config.ts`

**Intent**: Umozliwic lokalny development bez CORS i bez twardego URL backendu w kodzie aplikacji.

**Contract**: Dev server proxy'uje `/api` do `http://localhost:5218`. Kod aplikacji uzywa relatywnych endpointow `/api/...`.

#### 3. Frontend TypeScript configuration

**File**: `frontend/tsconfig.json`, `frontend/tsconfig.app.json`, `frontend/tsconfig.node.json`

**Intent**: Utrzymac frontend typowany i agent-friendly.

**Contract**: Typecheck przechodzi przez `npm run typecheck`; konfiguracja nie wymaga globalnych narzedzi poza standardowym Node/npm.

#### 4. Ignore generated frontend artifacts

**File**: `.gitignore`

**Intent**: Nie commitowac zaleznosci ani build outputu frontendowego.

**Contract**: Ignorowane sa co najmniej `frontend/node_modules/` i `frontend/dist/`.

### Success Criteria:

#### Automated Verification:

- `dotnet build .\ai-finance-tracker.csproj --no-restore` przechodzi po dodaniu katalogu frontendowego.
- `npm install` w `frontend/` instaluje zaleznosci projektu.
- `npm run typecheck` w `frontend/` przechodzi.
- `npm run build` w `frontend/` przechodzi.

#### Manual Verification:

- Frontend uruchamia sie przez `npm run dev` w `frontend/`.
- Kod frontendowy nie zawiera goals, dashboardu, auth, AI, chmury, edycji/usuwania ani custom kategorii.

**Implementation Note**: Po tej fazie warto uruchomic frontend i backend rownolegle tylko na tyle, zeby potwierdzic, ze proxy nie blokuje requestow do `/api`.

---

## Phase 2: API Client And Typed Finance Contracts

### Overview

Dodac frontendowe typy i klienta API dla trzech istniejacych endpointow, z obsluga sukcesow i problem responses z backendu.

### Changes Required:

#### 1. Finance API types

**File**: `frontend/src/api/financeTypes.ts`

**Intent**: Odwzorowac publiczny kontrakt backendu w frontendzie, bez generowania dodatkowej domeny.

**Contract**: Typy obejmuja `TransactionType`, `CategoryResponse`, `CreateTransactionRequest`, `TransactionResponse` i prosty ksztalt bledu API. `TransactionType` dopuszcza tylko `"Income"` i `"Expense"`.

#### 2. Finance API client

**File**: `frontend/src/api/financeApi.ts`

**Intent**: Oddzielic fetch/JSON/error handling od komponentow UI.

**Contract**: Klient eksportuje funkcje dla:

- `getCategories()`
- `getTransactions(limit?: number)`
- `createTransaction(request)`

Funkcje uzywaja `/api/categories` i `/api/transactions`, parsują JSON i zwracaja czytelny blad dla response spoza zakresu `2xx`.

#### 3. API error normalization

**File**: `frontend/src/api/financeApi.ts`

**Intent**: Pokazywac uzytkownikowi sensowna informacje, kiedy backend zwroci `ValidationProblem` albo `ProblemDetails`.

**Contract**: Dla `ValidationProblem` klient laczy komunikaty z `errors`; dla `ProblemDetails` uzywa `title`/`detail`; dla innych bledow zwraca bezpieczny komunikat ogolny. Nie loguje danych finansowych do zewnetrznych uslug.

### Success Criteria:

#### Automated Verification:

- `npm run typecheck` w `frontend/` przechodzi.
- `npm run build` w `frontend/` przechodzi.

#### Manual Verification:

- Przy dzialajacym backendzie `GET /api/categories` i `GET /api/transactions` sa wykonywane z frontendu przez proxy.
- Przy zatrzymanym backendzie UI pokazuje czytelny stan bledu zamiast pustego lub zawieszonego ekranu.

---

## Phase 3: Single Transaction Workspace UI

### Overview

Zbudowac jeden ekran roboczy, ktory pozwala dodac transakcje i przejrzec historie zgodna z aktualnym backendem.

### Changes Required:

#### 1. Application layout

**File**: `frontend/src/App.tsx`, `frontend/src/App.css`

**Intent**: Dac uzytkownikowi jeden roboczy widok finansow bez marketingowego hero i bez nawigacji do nieistniejacych funkcji.

**Contract**: Ekran zawiera sekcje formularza transakcji, sekcje historii oraz kompaktowe stany loading/error/empty. Layout jest czytelny na desktopie i laptopie.

#### 2. Transaction form

**File**: `frontend/src/components/TransactionForm.tsx`

**Intent**: Umozliwic szybkie dodanie przychodu albo wydatku z kategoria.

**Contract**: Formularz zawiera typ transakcji, kwote, date, kategorie i opcjonalny opis. Kwota musi byc dodatnia po stronie UI, data wymagana, opis ograniczony do 500 znakow. Kategorie sa filtrowane do zgodnych z typem oraz `Other`. Po sukcesie formularz czysci pola pomocnicze i odswieza historie.

#### 3. Transaction history

**File**: `frontend/src/components/TransactionHistory.tsx`

**Intent**: Pokazac ostatnie transakcje w czytelnej kolejnosci zgodnej z backendiem.

**Contract**: Historia pobiera `GET /api/transactions` i wyswietla date, typ, kwote, kategorie i opis. Nie dodaje lokalnego sortowania, ktore mogloby ukryc kontrakt backendu; frontend prezentuje kolejnosc otrzymana z API.

#### 4. Category-driven UX

**File**: `frontend/src/App.tsx`, `frontend/src/components/TransactionForm.tsx`

**Intent**: Nie hardcodowac kategorii w UI i zachowac user-correctable categorization z AGENTS.md.

**Contract**: Kategorie sa pobierane z `GET /api/categories`. Gdy lista kategorii nie jest dostepna, formularz nie pozwala wyslac transakcji i pokazuje jasny komunikat. `Other` pozostaje dostepne jako fallback.

#### 5. Basic styling

**File**: `frontend/src/styles.css` lub `frontend/src/App.css`

**Intent**: Zrobic prosty, uzytkowy interfejs bez biblioteki UI i bez efektow spoza domeny.

**Contract**: CSS definiuje responsywny, dashboardowy uklad, czytelne pola formularza, przyciski, komunikaty i liste historii. Paleta nie powinna byc monotematyczna; UI ma byc spokojny, narzedziowy i wygodny do skanowania.

### Success Criteria:

#### Automated Verification:

- `npm run typecheck` w `frontend/` przechodzi.
- `npm run build` w `frontend/` przechodzi.

#### Manual Verification:

- Uzytkownik moze uruchomic backend i frontend lokalnie, otworzyc ekran i zobaczyc formularz oraz historie.
- Uzytkownik moze dodac `Income` z kategoria dochodowa i zobaczyc transakcje w historii.
- Uzytkownik moze dodac `Expense` z kategoria wydatkowa i zobaczyc transakcje w historii.
- `Other` jest dostepne jako fallback dla obu typow transakcji.
- Niepoprawna transakcja pokazuje czytelny blad z backendu lub walidacji UI.
- UI nie pokazuje goals, dashboardu, auth, AI, chmury, edycji/usuwania ani custom kategorii.

**Implementation Note**: Po automatycznej weryfikacji zatrzymac sie na manualny smoke test w przegladarce, bo to pierwszy frontend w repo.

---

## Phase 4: Verification And Repository Guidance

### Overview

Domknac change przez pelna weryfikacje backendu i frontendu oraz aktualizacje instrukcji repo, jesli nowe komendy frontendowe zmieniaja workflow.

### Changes Required:

#### 1. Verification commands

**File**: terminal only

**Intent**: Udowodnic, ze frontend nie popsul backendu i sam przechodzi podstawowa weryfikacje.

**Contract**: Uruchomic z repo root i `frontend/` odpowiednie komendy restore/build/test/typecheck/build. Jesli `npm install` wymaga sieci, odnotowac to jako zaleznosc wykonawcza.

#### 2. Repository guidance update

**File**: `AGENTS.md`

**Intent**: Utrzymac onboarding dla kolejnych agentow zgodny z nowa struktura.

**Contract**: Dodac krotka sekcje albo punkty dla `frontend/`, komend `npm install`, `npm run dev`, `npm run typecheck`, `npm run build`. Zachowac hard rules local-first i zakres bez goals/dashboardu tylko tam, gdzie dotyczy tego change'a.

#### 3. Change metadata

**File**: `context/changes/mvp-frontend/change.md`

**Intent**: Utrzymac stan change'a zgodny z cyklem 10x.

**Contract**: Podczas implementacji aktualizowac status i notatki tylko wtedy, gdy zmieni sie istotny stan. Nie przenosic nic do `context/archive/` w tym change'u.

### Success Criteria:

#### Automated Verification:

- `dotnet restore .\ai-finance-tracker.csproj` przechodzi.
- `dotnet build .\ai-finance-tracker.csproj --no-restore` przechodzi.
- `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj` przechodzi.
- `dotnet list .\ai-finance-tracker.csproj package --vulnerable --include-transitive` przechodzi.
- `npm run typecheck` w `frontend/` przechodzi.
- `npm run build` w `frontend/` przechodzi.

#### Manual Verification:

- Backend i frontend uruchamiaja sie lokalnie razem.
- Smoke test w przegladarce potwierdza pobranie kategorii, dodanie transakcji i odswiezenie historii.
- Dokumentacja repo opisuje nowe frontendowe komendy.
- Zakres change'a nadal odpowiada tylko obecnemu backendowi.

---

## Testing Strategy

### Unit Tests:

- Nie dodajemy testow jednostkowych frontendowych w tym change'u, bo wybrana weryfikacja to typecheck/build plus manualny smoke test.
- Jesli implementacja wyciagnie nietrywialne formatowanie kwot, dat albo normalizacje bledow, mozna dodac male testy jednostkowe, ale nie jest to wymaganie fazy.

### Integration Tests:

- Backendowe testy endpointow pozostaja zrodlem ochrony kontraktu API.
- Frontend nie dostaje e2e w tym change'u; reczny smoke test pokrywa pierwszy przeplyw w przegladarce.

### Manual Testing Steps:

1. Uruchomic backend: `dotnet run --project .\ai-finance-tracker.csproj --launch-profile http`.
2. W `frontend/` uruchomic `npm run dev`.
3. Otworzyc lokalny adres Vite.
4. Potwierdzic, ze formularz laduje kategorie z backendu.
5. Dodac wydatek z kategoria wydatkowa.
6. Dodac przychod z kategoria dochodowa.
7. Dodac transakcje z `Other` dla obu typow.
8. Potwierdzic, ze historia pokazuje nowe transakcje z data, typem, kwota, kategoria i opisem.
9. Sprobowac wyslac niepoprawna kwote albo niepoprawne dane i potwierdzic czytelny blad.
10. Potwierdzic, ze UI nie zawiera goals, dashboardu, auth, AI, chmury, edycji/usuwania ani custom kategorii.

## Performance Considerations

MVP ma male lokalne wolumeny danych. Frontend pobiera limitowana historie z backendu i nie potrzebuje paginacji, virtualizacji ani cache. Formularz i historia powinny dzialac plynnie przy typowym lokalnym uzyciu, a dashboardowe obliczenia pozostaja poza zakresem.

## Migration Notes

Ten change nie wymaga migracji bazy danych. Uzywa istniejacych tabel profilu lokalnego, kategorii i transakcji. Jesli podczas implementacji pojawi sie potrzeba nowej tabeli albo endpointu, nalezy zatrzymac implementacje i zaktualizowac plan zamiast rozszerzac zakres po cichu.

## References

- Product contract: `context/foundation/prd.md`
- Stack hand-off: `context/foundation/tech-stack.md`
- Roadmap: `context/foundation/roadmap.md`
- Backend API plan: `context/changes/transaction-entry-history/plan.md`
- Repository rules: `AGENTS.md`
- API entrypoint: `Program.cs`
- API contracts: `Contracts/FinanceContracts.cs`
- API endpoints: `Endpoints/FinanceEndpoints.cs`
- Local launch profile: `Properties/launchSettings.json`

## Progress

> Convention: `- [ ]` pending, `- [x]` done. Append `- <commit sha>` when a step lands. Do not rename step titles.

### Phase 1: Frontend Scaffold And Local API Wiring

#### Automated

- [x] 1.1 `dotnet build .\ai-finance-tracker.csproj --no-restore` przechodzi po dodaniu katalogu frontendowego
- [x] 1.2 `npm install` w `frontend/` instaluje zaleznosci projektu
- [x] 1.3 `npm run typecheck` w `frontend/` przechodzi
- [x] 1.4 `npm run build` w `frontend/` przechodzi

#### Manual

- [x] 1.5 Frontend uruchamia sie przez `npm run dev` w `frontend/`
- [x] 1.6 Kod frontendowy nie zawiera goals, dashboardu, auth, AI, chmury, edycji/usuwania ani custom kategorii

### Phase 2: API Client And Typed Finance Contracts

#### Automated

- [ ] 2.1 `npm run typecheck` w `frontend/` przechodzi
- [ ] 2.2 `npm run build` w `frontend/` przechodzi

#### Manual

- [ ] 2.3 Przy dzialajacym backendzie `GET /api/categories` i `GET /api/transactions` sa wykonywane z frontendu przez proxy
- [ ] 2.4 Przy zatrzymanym backendzie UI pokazuje czytelny stan bledu zamiast pustego lub zawieszonego ekranu

### Phase 3: Single Transaction Workspace UI

#### Automated

- [ ] 3.1 `npm run typecheck` w `frontend/` przechodzi
- [ ] 3.2 `npm run build` w `frontend/` przechodzi

#### Manual

- [ ] 3.3 Uzytkownik moze uruchomic backend i frontend lokalnie, otworzyc ekran i zobaczyc formularz oraz historie
- [ ] 3.4 Uzytkownik moze dodac `Income` z kategoria dochodowa i zobaczyc transakcje w historii
- [ ] 3.5 Uzytkownik moze dodac `Expense` z kategoria wydatkowa i zobaczyc transakcje w historii
- [ ] 3.6 `Other` jest dostepne jako fallback dla obu typow transakcji
- [ ] 3.7 Niepoprawna transakcja pokazuje czytelny blad z backendu lub walidacji UI
- [ ] 3.8 UI nie pokazuje goals, dashboardu, auth, AI, chmury, edycji/usuwania ani custom kategorii

### Phase 4: Verification And Repository Guidance

#### Automated

- [ ] 4.1 `dotnet restore .\ai-finance-tracker.csproj` przechodzi
- [ ] 4.2 `dotnet build .\ai-finance-tracker.csproj --no-restore` przechodzi
- [ ] 4.3 `dotnet test .\tests\AiFinanceTracker.Tests\AiFinanceTracker.Tests.csproj` przechodzi
- [ ] 4.4 `dotnet list .\ai-finance-tracker.csproj package --vulnerable --include-transitive` przechodzi
- [ ] 4.5 `npm run typecheck` w `frontend/` przechodzi
- [ ] 4.6 `npm run build` w `frontend/` przechodzi

#### Manual

- [ ] 4.7 Backend i frontend uruchamiaja sie lokalnie razem
- [ ] 4.8 Smoke test w przegladarce potwierdza pobranie kategorii, dodanie transakcji i odswiezenie historii
- [ ] 4.9 Dokumentacja repo opisuje nowe frontendowe komendy
- [ ] 4.10 Zakres change'a nadal odpowiada tylko obecnemu backendowi
