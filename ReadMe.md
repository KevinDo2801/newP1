# Number Logic Game - Project README

A grid-based puzzle game where players place numbers sequentially on a board following strict adjacency rules.

## Project Structure

```
Project_1/
├── backend/          # .NET 10 Web API
├── frontend/         # React + TypeScript + Vite
```

## Prerequisites

### Backend
- .NET 10 SDK
- Windows/Linux/macOS

### Frontend
- Node.js 18+ (npm included)
- Modern web browser

## Quick Start

### 1. Backend Setup

```bash
cd backend
dotnet restore
dotnet build
```

### 2. Run Backend

```bash
cd backend
dotnet run
```

Backend API runs on: **http://localhost:5000**

**Swagger UI** (Development mode): http://localhost:5000/swagger

### 3. Frontend Setup

Open a new terminal:

```bash
cd frontend
npm install
```

### 4. Run Frontend

```bash
cd frontend
npm run dev
```

Frontend runs on: **http://localhost:5173**

## Running Both Services

### One Command (Recommended)

```bash
./start.sh
```

This starts both backend and frontend together. Press **Ctrl+C** to stop both.

### Two Terminals (Alternative)

**Terminal 1 (Backend):**
```bash
cd backend
dotnet run
```

**Terminal 2 (Frontend):**
```bash
cd frontend
npm run dev
```

## Tech Stack

### Backend
- .NET 10
- ASP.NET Core Web API
- Swashbuckle.AspNetCore (Swagger)
- CORS enabled for frontend

### Frontend
- React 18
- TypeScript
- Vite
- ESLint

## Development Notes

- Backend must run before frontend (frontend proxies API calls to backend)
- CORS is configured to allow requests from `http://localhost:5173`
- Game logs are saved to `backend/logs/` directory
- Game state is stored in-memory (resets on server restart)

## Git Workflow & Branch Protection

This project uses a protected `master` branch to ensure code stability and proper collaboration.

- Direct commits to the `master` branch are **not allowed**.
- All team members must create a **separate feature branch** for their work.
- Changes must be submitted via a **Pull Request (PR)**.
- Each PR requires **approval before being merged** into `master`.

This workflow follows standard Agile and Scrum best practices and helps maintain code quality throughout the project.

## Troubleshooting

### Backend won't start
- Check if port 5000 is already in use
- Verify .NET 10 SDK is installed: `dotnet --version`
- Clean and rebuild: `dotnet clean && dotnet build`

### Frontend won't start
- Check if port 5173 is already in use
- Verify Node.js is installed: `node --version`
- Delete `node_modules` and reinstall: `rm -rf node_modules && npm install`

### CORS errors
- Ensure backend is running on port 5000
- Check CORS configuration in `backend/Program.cs`
- Verify frontend is accessing `http://localhost:5173`

### API connection issues
- Verify backend is running: http://localhost:5000/swagger
- Check browser console for errors
- Verify proxy configuration in `frontend/vite.config.ts`
