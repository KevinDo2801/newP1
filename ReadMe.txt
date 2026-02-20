================================================================================
  Number Logic Game — CEN4020 Project 1
================================================================================

A grid-based puzzle game where players place numbers sequentially on a board
following strict adjacency rules. Built with a .NET backend API and a React +
TypeScript frontend.

--------------------------------------------------------------------------------
1. SOURCE CODE FILES — Description of Each
--------------------------------------------------------------------------------

BACKEND (C# / .NET):
  - Program.cs              Application entry point; configures ASP.NET Core Web API,
                            CORS, Swagger, and registers game services
  - GameLogic.cs            Core game rules, board state, placement validation,
                            undo logic, Level 1/2/3 board expansion
  - ConsoleUI.cs            Console-based UI for play-from-terminal; used with
                            GameLogic and SaveLoadManager
  - SaveLoadManager.cs      Saves/loads game state to/from text file (savegame.txt)
  - Controllers/GameController.cs   REST API endpoints: new, place, undo, clear,
                            expand-level2, expand-level3; delegates to GameStateService
  - Models/GameStateDto.cs  Data transfer objects for game state (board, score,
                            level, next number, etc.)
  - Models/GameLogDto.cs    DTO for completed game logs (player, date, duration,
                            level, score, board)
  - Models/GameSettings.cs  Time limit preset per level (Level1/2/3 in seconds)
  - Services/GameStateService.cs    In-memory game state management; creates
                            games, processes moves, undo, clear, expand
  - Services/GameLogService.cs      Persists completed games as JSON files in
                            backend/logs/
  - appsettings.json        Default configuration (URLs, logging, TimeLimits per level)
  - appsettings.Development.json   Development settings
  - Properties/launchSettings.json  Launch profiles for debugging
  - backendAPI.csproj       Project file; targets .NET 10, references Swashbuckle

FRONTEND (TypeScript / React):
  - main.tsx                Entry point; mounts React app to DOM
  - App.tsx                 Root component; defines routes (/, /select-level, /game)
  - index.html              HTML shell; loads main.tsx
  - pages/OnboardPage.tsx   Landing page; player name input and Start Game
  - pages/SelectLevelPage.tsx   Level selection (Level 1, 2, or 3)
  - pages/GamePage.tsx      Main game UI; 5x5/7x7 board, controls, sidebar, timer
  - services/gameApi.ts     API client; calls backend /api/Game/* endpoints
  - types/index.ts          TypeScript interfaces (GameState, etc.)
  - utils/sounds.ts         Sound effects via Web Audio API (valid/invalid beep)
  - global.css              Global styles
  - index.css               Base styles, Tailwind imports
  - vite.config.ts          Vite config; dev server, proxy /api to backend
  - tsconfig.json           TypeScript compiler options
  - tsconfig.node.json      TS config for Vite config file
  - package.json            npm dependencies (React, Vite, Tailwind, etc.)

DESIGN (HTML mockups):
  - design/onboard.html     Wireframe for onboarding page
  - design/selectLevel.html Wireframe for level selection
  - design/level123.html    Wireframe for game board

SCRIPTS:
  - start.bat               Windows: Double-click to run both backend and frontend
  - start.sh                Mac/Linux: Run ./start.sh to run both backend and frontend

--------------------------------------------------------------------------------
2. OS AND LANGUAGE(S) / TOOL(S) / PLATFORM
--------------------------------------------------------------------------------

OS:        Windows 10/11, macOS, or Linux
Languages: C# (backend), TypeScript (frontend)
Tools:     .NET 10 SDK, Node.js 18+, npm
Platform:  ASP.NET Core Web API (backend), React 18 + Vite (frontend)
Other:     Swashbuckle (Swagger), Tailwind CSS v4, React Router DOM

--------------------------------------------------------------------------------
3. COMPILATION PROCEDURE TO MAKE EXECUTABLE Proj1
--------------------------------------------------------------------------------

The application consists of a backend API (Proj1 executable) and a frontend.
To build and run:

A. Build Backend (Proj1 executable):
   1. Open terminal/command prompt
   2. cd backend
   3. dotnet restore
   4. dotnet build
   5. dotnet publish -c Release -o Proj1

   The published executable will be in backend/Proj1/ as:
   - backendAPI.exe (Windows) or backendAPI (Linux/macOS)
   Run with: cd Proj1 && ./backendAPI (or backendAPI.exe on Windows)

B. Build Frontend (optional for production):
   1. cd frontend
   2. npm install
   3. npm run build
   Output goes to frontend/dist/ — serve with any static file server

C. Quick Run (one-click — starts both backend and frontend):

   WINDOWS:
   - Double-click start.bat
   - Two windows will open (Backend + Frontend). Open http://localhost:5173

   MAC / LINUX:
   - Open Terminal, go to project folder
   - Run:  chmod +x start.sh    (first time only)
   - Run:  ./start.sh
   - Press Ctrl+C to stop both. Open http://localhost:5173

   MANUAL (two terminals):
   Terminal 1: cd backend && dotnet run
   Terminal 2: cd frontend && npm install && npm run dev

--------------------------------------------------------------------------------
4. USER STORIES IMPLEMENTED
--------------------------------------------------------------------------------

US1  GUI — 5x5 board with number 1 randomly placed; Level 2 adds outer ring  Done
US2  Sound/beep on valid number placement                                    Done
US3  Next number auto-generated and displayed (not auto-placed)              Done
US4  Clear board — keep #1 in place or re-randomize; L2 clears outer ring    Done
US5  Undo/rollback as many moves as desired from most recent                 Done
US6  Sound/beep on invalid placement to warn player                          Done
US7  Log and save completed game (player, date/time, level, score, board)    Done
US8  Level 2 expansion — 7x7 board (inner 5x5 + 24-cell outer ring)          Done
US9  Level 3 — final level (inner 5x5 reconstruction, intersection/diagonal  Done
     rules; expand after Level 2 win)
US10 Reward points — +1 per number placed (all levels); -1 per undo/rollback Done
     or clear; scores accumulated across levels
US11 Time limit preset per level; +1 pt per second unused; -1 pt per second  Done
     over limit

--------------------------------------------------------------------------------
5. HOW TO PLAY
--------------------------------------------------------------------------------

1. Enter your name on the landing page and click Start Game
2. Select Level 1, 2, or 3 to begin (Level 2/3 unlock after previous level win)
3. Number 1 is already placed randomly on the 5x5 board
4. Click any empty adjacent cell (including diagonals) to place the next number
5. The Next Number is displayed in the sidebar — you cannot skip or reorder
6. REWARDS (US10): +1 point per number successfully placed (all levels); -1
   point per cell rolled back (undo) or cleared (reset); scores accumulate
7. Diagonal placements earn +1 bonus point (Level 1)
8. Use Undo to rollback any number of recent moves
9. Use Reset to clear the board (option to keep #1 in place or re-randomize)
10. Complete all 25 numbers to finish Level 1, then click Expand to Level 2
11. In Level 2, place numbers 26–49 on the outer ring of the 7x7 board
12. Complete Level 2, then click Expand to Level 3 (US9) — inner 5x5 is cleared
    except #1; place numbers 2–25 back using intersection/diagonal rules
13. TIME LIMIT (US11): Each level has a preset time limit (e.g. 60/120/180 sec).
    Finish early: +1 point per second unused. Finish late: -1 point per second over

--------------------------------------------------------------------------------
6. API ENDPOINTS
--------------------------------------------------------------------------------

POST  /api/Game/new           Create a new game (TimeLimitSeconds optional)
GET   /api/Game/{gameId}      Get current game state
GET   /api/Game/progress/{username}  Get player progress (levels completed, total score)
POST  /api/Game/place         Place the next number at (row, col)
POST  /api/Game/undo          Undo last N moves
POST  /api/Game/clear         Clear/reset the board
POST  /api/Game/expand-level2 Expand to Level 2 after Level 1 win
POST  /api/Game/expand-level3 Expand to Level 3 after Level 2 win

--------------------------------------------------------------------------------
7. GAME LOGS (Sample File)
--------------------------------------------------------------------------------

Completed games are saved as JSON files in backend/logs/ with:
- Player username
- Completion date/time
- Duration (seconds)
- Level
- Score (placement points + time bonus/penalty; accumulated across levels)
- Time limit used (when applicable)
- Full board state

Time limits are configured in appsettings.json (GameSettings.TimeLimits:
  Level1, Level2, Level3 in seconds; 0 = no limit).

A sample completed game log is included: game_log_*.json

--------------------------------------------------------------------------------
8. TROUBLESHOOTING
--------------------------------------------------------------------------------

Backend won't start   -> Check port 5000 is free. Verify dotnet --version shows .NET 10
Frontend won't start  -> Check port 5173 is free. Run: rm -rf node_modules && npm install
CORS errors           -> Ensure backend is on port 5000. Check Program.cs CORS policy
No sound              -> Click the page first (browsers require user interaction)
API errors            -> Check backend is running at http://localhost:5000/swagger

================================================================================
