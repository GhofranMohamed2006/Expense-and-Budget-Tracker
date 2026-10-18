@echo off
setlocal

where dotnet >nul 2>nul
if errorlevel 1 (
  echo .NET SDK was not found. Install the .NET 8 SDK, then run this file again.
  pause
  exit /b 1
)

echo Starting the Pennywise backend...
pushd "%~dp0Backend"
if errorlevel 1 (
  echo Could not find the Backend directory.
  pause
  exit /b 1
)
start /B dotnet run --no-launch-profile
popd

echo Waiting for the backend to become available...
powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass -Command "$deadline = (Get-Date).AddSeconds(45); $lastError = 'No health response yet.'; do { try { $response = Invoke-WebRequest -UseBasicParsing -Uri 'http://localhost:5005/api/auth/health' -TimeoutSec 2; if ($response.StatusCode -eq 200) { Start-Process 'http://localhost:5005/index.html'; exit 0 } } catch { $lastError = $_.Exception.Message }; Start-Sleep -Seconds 1 } while ((Get-Date) -lt $deadline); Write-Error ('The backend did not start within 45 seconds. Last health check: ' + $lastError); exit 1"
if errorlevel 1 (
  echo The backend did not start. Check the output above for errors.
  pause
  exit /b 1
)

echo Pennywise is ready in your browser.
