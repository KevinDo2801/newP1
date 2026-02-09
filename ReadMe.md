# Number Logic Game — CEN4020 Project 1

A grid-based puzzle game where players place numbers sequentially on a board following strict adjacency rules. Built with a .NET backend API and a React + TypeScript frontend.

## Team Members

| Member | Responsibility |
|--------|----------------|
| **Member 1 (Ketchup)** | Core Game Logic — US3, US4, US5, US8 |
| **Member 2 (Yo)** | UI / Interaction — US1, Buttons (Undo/Reset/Level), UI ↔ GameLogic |
| **Member 3 (Ketchup's friend)** | Sound & Validation — US2, US6, US7, Validation feedback, README |

## User Stories Implemented

| US | Description | Status |
|----|-------------|--------|
| US1 | GUI — 5×5 board with number 1 randomly placed; Level 2 adds outer ring | Done |
| US2 | Sound/beep on valid number placement | Done |
| US3 | Next number auto-generated and displayed (not auto-placed) | Done |
| US4 | Clear board — keep #1 in place or re-randomize; Level 2 clears outer ring only | Done |
| US5 | Undo/rollback as many moves as desired from most recent | Done |
| US6 | Sound/beep on invalid placement to warn player | Done |
| US7 | Log and save completed game (player name, date/time, level, score, board) | Done |
| US8 | Level 2 expansion — 7×7 board (inner 5×5 + 24-cell outer ring) | Done |

## Project Structure

```
Project-1-CEN4020-master/
├── backend/                  # .NET 10 Web API
│   ├── Controllers/          # API endpoints
│   ├── Models/               # DTOs (GameStateDto, etc.)
│   ├── Services/             # GameStateService, GameLogService
│   ├── GameLogic.cs          # Core game rules & board logic
│   └── Program.cs            # App entry point
├── frontend/                 # React 18 + TypeScript + Vite
│   ├── pages/                # OnboardPage, SelectLevelPage, GamePage
│   ├── services/             # API client (gameApi.ts)
│   ├── utils/                # Sound effects (sounds.ts)
│   └── types/                # TypeScript interfaces
├── start.sh                  # One-command launcher for both servers
└── ReadMe.md
```

## Prerequisites

- **.NET 10 SDK** — [Download](https://dotnet.microsoft.com/download)
- **Node.js 18+** (includes npm) — [Download](https://nodejs.org)

## Quick Start

### One Command (Recommended)

```bash
./start.sh
```

This starts both backend and frontend together. Press **Ctrl+C** to stop both.

### Manual Setup (Two Terminals)

**Terminal 1 — Backend:**
```bash
cd backend
dotnet restore
dotnet build
dotnet run
```
Backend runs on **http://localhost:5000** (Swagger UI at `/swagger` in dev mode).

**Terminal 2 — Frontend:**
```bash
cd frontend
npm install
npm run dev
```
Frontend runs on **http://localhost:5173**.

## How to Play

1. Enter your name on the landing page and click **Start Game**.
2. Select **Level 1** to begin.
3. Number **1** is already placed randomly on the 5×5 board.
4. Click any empty **adjacent cell** (including diagonals) to place the next number.
5. The **Next Number** is displayed in the sidebar — you cannot skip or reorder.
6. Diagonal placements earn **+1 bonus point**.
7. Use **Undo** to rollback any number of recent moves.
8. Use **Reset** to clear the board (option to keep #1 in place or re-randomize).
9. Complete all 25 numbers to finish **Level 1**, then click **Expand to Level 2**.
10. In Level 2, place numbers **26–49** on the outer ring of the expanded 7×7 board.

## Tech Stack

### Backend
- .NET 10 / ASP.NET Core Web API
- In-memory game state (ConcurrentDictionary)
- Swashbuckle (Swagger) for API docs
- CORS enabled for frontend origin

### Frontend
- React 18 + TypeScript
- Vite (dev server & build)
- Tailwind CSS v4 (styling)
- Web Audio API (sound effects)
- React Router DOM (navigation)

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Game/new` | Create a new game |
| GET | `/api/Game/{gameId}` | Get current game state |
| POST | `/api/Game/place` | Place the next number at (row, col) |
| POST | `/api/Game/undo` | Undo last N moves |
| POST | `/api/Game/clear` | Clear/reset the board |
| POST | `/api/Game/expand-level2` | Expand to Level 2 after Level 1 win |

## Game Logs

Completed games are saved as JSON files in `backend/logs/` with:
- Player username
- Completion date/time
- Duration (seconds)
- Level
- Score (diagonal bonus points)
- Full board state

## Development Notes

- Backend must be running before frontend (frontend proxies `/api` requests to backend).
- Game state is in-memory — resets on server restart.
- CORS is configured for `http://localhost:5173`.

## Git Workflow

- Direct commits to `master` are **not allowed**.
- Create a **feature branch** for your work.
- Submit a **Pull Request (PR)** for review.
- PRs require **approval** before merge.

## Troubleshooting

| Issue | Fix |
|-------|-----|
| Backend won't start | Check port 5000 is free. Verify `dotnet --version` shows .NET 10. |
| Frontend won't start | Check port 5173 is free. Run `rm -rf node_modules && npm install`. |
| CORS errors | Ensure backend is on port 5000. Check `Program.cs` CORS policy. |
| No sound | Click the page first (browsers require user interaction before audio). |
| API errors | Check backend is running at `http://localhost:5000/swagger`. |
