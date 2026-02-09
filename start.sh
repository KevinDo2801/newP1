#!/bin/bash
# Start both backend and frontend with a single command
# Usage: ./start.sh

echo "=== Number Logic Game ==="
echo ""

# Navigate to script directory (works when run from anywhere)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Kill both processes on exit (Ctrl+C)
trap 'echo ""; echo "Shutting down..."; kill 0; exit' SIGINT SIGTERM

# Start backend
echo "[Backend] Starting on http://localhost:5000 ..."
cd backend && dotnet run &
BACKEND_PID=$!

# Give backend a moment to boot
sleep 3

# Start frontend
echo "[Frontend] Starting on http://localhost:5173 ..."
cd frontend && npm run dev &
FRONTEND_PID=$!

echo ""
echo "Both servers running. Press Ctrl+C to stop."
echo ""

# Wait for both
wait
