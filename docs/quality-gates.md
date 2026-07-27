# Quality gates

## Lokalna weryfikacja

Z katalogu głównego repozytorium uruchom:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify.ps1
```

Skrypt wykonuje kolejno:

1. restore projektu API i projektu testowego,
2. build backendu z `UseAppHost=false`,
3. pełny zestaw testów backendu,
4. frontend typecheck,
5. frontend build.

Po poprawnym restore można skrócić kolejne uruchomienie:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify.ps1 -SkipRestore
```

Każdy niezerowy kod wyjścia zatrzymuje skrypt. Nie należy pomijać failing testów
ani używać tej opcji do ukrycia problemu z restore.

## CI i hooki

GitHub Actions pozostaje obowiązkową bramką dla pushy i pull requestów do
`main`. Workflow uruchamia backend i frontend jako osobne joby.

Repozytorium nie ustawia automatycznie `core.hooksPath` i nie instaluje narzędzia
do hooków. Taka konfiguracja jest lokalna dla konkretnego klona, a wymuszanie jej
przez skrypt projektu byłoby zaskakującym efektem ubocznym. Przed pushem należy
uruchomić `scripts/verify.ps1`; zespół może później podpiąć tę samą komendę do
lokalnego hooka, jeśli pojawi się realna potrzeba.
