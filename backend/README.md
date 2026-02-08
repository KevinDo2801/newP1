# Number Logic Game - Backend API

ASP.NET Core Web API backend for Number Logic Game.

## Setup

```bash
dotnet restore
dotnet build
```

## Run

```bash
dotnet run
```

API runs on http://localhost:5000

## API Endpoints

- `POST /api/game/new` - Create new game
- `GET /api/game/{gameId}` - Get game state
- `POST /api/game/place` - Place a number
- `POST /api/game/undo` - Undo last move
- `POST /api/game/clear` - Clear board

## Swagger

When running in Development mode, Swagger UI is available at:
http://localhost:5000/swagger

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- CORS enabled for frontend (localhost:5173)
