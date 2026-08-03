@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command ^
  "$root = [IO.Path]::GetFullPath('%~dp0');" ^
  "Start-Process cmd.exe -ArgumentList '/k', 'dotnet run --project .\ai-finance-tracker.csproj --launch-profile http -p:UseAppHost=false' -WorkingDirectory $root -WindowStyle Normal;" ^
  "Start-Process cmd.exe -ArgumentList '/k', 'npm run dev' -WorkingDirectory (Join-Path $root 'frontend') -WindowStyle Normal"
exit /b 0
