@echo off
echo === Number Logic Game ===
echo.

echo [Backend] Starting on http://localhost:5000 ...
start "Backend" cmd /k "cd /d "%~dp0backend" && dotnet run"

echo Waiting for backend to start...
timeout /t 3 /nobreak > nul

echo [Frontend] Starting on http://localhost:5173 ...
start "Frontend" cmd /k "cd /d "%~dp0frontend" && npm run dev"

echo.
echo Both servers running. Close the Backend and Frontend windows to stop.
echo Open http://localhost:5173 in your browser to play.
exit
