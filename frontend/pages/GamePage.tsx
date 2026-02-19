import { useState, useCallback, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import type { GameState } from "../types";
import * as api from "../services/gameApi";
import { playValidSound, playInvalidSound, playWinSound } from "../utils/sounds";

const GamePage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const navState = location.state as {
    username?: string; gameId?: string; gameState?: GameState; isViewingCompleted?: boolean;
  } | null;

  const [gameId] = useState<string | null>(navState?.gameId ?? null);
  const isViewingCompleted = navState?.isViewingCompleted ?? false;
  const [gameState, setGameState] = useState<GameState | null>(navState?.gameState ?? null);
  const [busy, setBusy] = useState(false);
  const [toast, setToast] = useState<{ text: string; type: "ok" | "err" } | null>(null);
  const [undoSteps, setUndoSteps] = useState(1);
  const [keepOne, setKeepOne] = useState(true);
  const [showReset, setShowReset] = useState(false);

  const username = navState?.username ?? gameState?.playerUsername ?? "Player";

  useEffect(() => {
    if (!gameState) navigate("/select-level", { state: { username } });
    else if (!gameId && !isViewingCompleted) navigate("/select-level", { state: { username } });
  }, [gameId, gameState, isViewingCompleted, navigate, username]);

  // auto-dismiss toast
  useEffect(() => {
    if (!toast) return;
    const t = setTimeout(() => setToast(null), 2500);
    return () => clearTimeout(t);
  }, [toast]);

  const msg = (text: string, type: "ok" | "err" = "ok") => setToast({ text, type });

  // --- Place number ---
  const handleCell = useCallback(async (r: number, c: number) => {
    if (!gameId || !gameState || busy || gameState.hasWon) return;
    if (gameState.board[r][c] !== 0) return;
    setBusy(true);
    try {
      const res = await api.placeNumber(gameId, r, c);
      if (res.gameState) setGameState(res.gameState);
      if (res.success && res.isValid) {
        if (res.gameState?.hasWon) {
          playWinSound();
          msg(`Level ${res.gameState.level} complete!`);
          // Progress is saved in backend logs; frontend loads from API
        } else {
          playValidSound(); // US2: sound on valid move
        }
      } else {
        playInvalidSound(); // US6: sound on invalid move
        msg(res.errorMessage || "Invalid placement", "err");
      }
    } catch (e) { msg(e instanceof Error ? e.message : "Error", "err"); }
    finally { setBusy(false); }
  }, [gameId, gameState, busy]);

  // --- Undo ---
  const handleUndo = useCallback(async () => {
    if (!gameId || busy) return;
    setBusy(true);
    try {
      const res = await api.undo(gameId, Math.max(1, undoSteps));
      if (res.gameState) { setGameState(res.gameState); msg(`Undid ${undoSteps} move${undoSteps > 1 ? "s" : ""}`); }
    } catch { msg("Nothing to undo", "err"); }
    finally { setBusy(false); }
  }, [gameId, undoSteps, busy]);

  // --- Clear ---
  const handleClear = useCallback(async () => {
    if (!gameId || busy) return;
    setBusy(true); setShowReset(false);
    try {
      const res = await api.clearBoard(gameId, keepOne);
      if (res.gameState) {
        setGameState(res.gameState);
        msg(gameState?.level === 2 ? "Outer ring cleared" : "Board cleared");
      }
    } catch { msg("Clear failed", "err"); }
    finally { setBusy(false); }
  }, [gameId, keepOne, busy, gameState?.level]);

  // --- Expand to Level 2 ---
  const handleExpand = useCallback(async () => {
    if (!gameId || busy || !gameState || gameState.level !== 1 || !gameState.hasWon) return;
    setBusy(true);
    try {
      const res = await api.expandToLevel2(gameId);
      if (res.gameState) { setGameState(res.gameState); msg("Welcome to Level 2!"); }
    } catch { msg("Expand failed", "err"); }
    finally { setBusy(false); }
  }, [gameId, gameState, busy]);

  // --- Expand to Level 3 ---
  const handleExpandLevel3 = useCallback(async () => {
    if (!gameId || busy || !gameState || gameState.level !== 2 || !gameState.hasWon) return;
    setBusy(true);
    try {
      const res = await api.expandToLevel3(gameId);
      if (res.gameState) { 
        setGameState(res.gameState); 
        msg("Welcome to Level 3! Solve the puzzle using the outer ring numbers."); 
      }
    } catch { msg("Expand failed", "err"); }
    finally { setBusy(false); }
  }, [gameId, gameState, busy]);

  if (!gameState || (!gameId && !isViewingCompleted)) return null;

  const board = gameState.board;
  const cols = board[0]?.length ?? 5;
  const isL2 = gameState.level === 2;
  const isL3 = gameState.level === 3;
  const next = gameState.nextNumberToPlace ?? gameState.currentNumber;
  const max = 25; // All levels: place numbers up to 25
  const placedCount = gameState.hasWon ? max : Math.max(0, next - 1);
  const pct = Math.round((placedCount / max) * 100);

  return (
    <div className="min-h-screen bg-bg p-4 sm:p-6">
      <div className="max-w-5xl mx-auto">

        {/* Top bar */}
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-3">
            <button
              onClick={() => navigate("/select-level", { state: { username } })}
              className="w-9 h-9 rounded-lg flex items-center justify-center bg-card border border-border hover:bg-card-hover transition-colors"
            >
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="m15 18-6-6 6-6"/></svg>
            </button>
            <div>
              <h1 className="text-lg font-bold text-white leading-tight">
                Level {gameState.level}
                <span className="text-slate-500 text-sm font-medium ml-1.5">
                  {isL2 ? "7x7" : "5x5"}
                </span>
              </h1>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-xs text-slate-400 bg-card border border-border px-2.5 py-1 rounded-lg">
              {username}
            </span>
            <span className={`text-xs font-bold px-2.5 py-1 rounded-lg ${
              gameState.score >= 0 
                ? 'text-amber bg-amber/10 border border-amber/20'
                : 'text-red-400 bg-red-400/10 border border-red-400/20'
            }`}>
              {gameState.score} pts
            </span>
          </div>
        </div>

        {/* Main: board + sidebar */}
        <div className="flex flex-col lg:flex-row gap-6">

          {/* Left: board area */}
          <div className="flex-1 flex flex-col items-center gap-5">

            {/* Progress */}
            <div className="w-full max-w-sm">
              <div className="flex justify-between text-xs text-slate-500 mb-1">
                <span>Progress</span>
                <span>{placedCount} / {max}</span>
              </div>
              <div className="w-full h-2 rounded-full bg-card border border-border overflow-hidden">
                <div
                  className="h-full rounded-full bg-primary transition-all duration-300"
                  style={{ width: `${pct}%` }}
                />
              </div>
            </div>

            {/* Board */}
            <div className="bg-card border border-border rounded-xl p-3 sm:p-4">
              <div
                className="grid gap-1.5 sm:gap-2"
                style={{ gridTemplateColumns: `repeat(${cols}, 1fr)` }}
              >
                {board.map((row, r) =>
                  row.map((val, c) => {
                    const filled = val !== 0;
                    const outer = (isL2 || isL3) && (r === 0 || r === 6 || c === 0 || c === 6);
                    const last = gameState.lastRow === r && gameState.lastCol === c;
                    const off = busy || filled || gameState.hasWon;

                    // Level 3 special styling: outer ring is read-only reference
                    const l3Outer = isL3 && outer;

                    return (
                      <button
                        key={`${r}-${c}`}
                        onClick={() => handleCell(r, c)}
                        disabled={off || l3Outer}
                        className={[
                          "w-10 h-10 sm:w-12 sm:h-12 rounded-lg flex items-center justify-center",
                          "text-sm font-bold transition-colors",
                          filled
                            ? outer
                              ? last
                                ? "bg-accent/30 border-2 border-accent text-white"
                                : isL3 
                                  ? "bg-slate-700/40 border border-slate-600 text-slate-400" // Muted for L3 reference
                                  : "bg-accent/15 border border-accent/40 text-white"
                              : last
                                ? "bg-primary/20 border-2 border-primary text-white"
                                : "bg-primary/10 border border-primary/25 text-white"
                            : outer
                              ? isL3
                                ? "bg-slate-800/20 border border-slate-700 opacity-20" // Empty outer in L3
                                : "bg-accent/5 border-2 border-accent/30 hover:bg-accent/15 hover:border-accent/50 cursor-pointer"
                              : "bg-card border border-border hover:bg-card-hover hover:border-border-light cursor-pointer",
                          (off || l3Outer) && !filled ? "opacity-30 cursor-not-allowed" : "",
                        ].join(" ")}
                      >
                        {filled ? val : ""}
                      </button>
                    );
                  })
                )}
              </div>
            </div>

            {/* Controls - hide when viewing completed */}
            {!isViewingCompleted && (
            <div className="flex flex-wrap items-center justify-center gap-2">
              {/* Undo */}
              <button
                onClick={handleUndo}
                disabled={busy}
                className="h-10 px-4 rounded-lg flex items-center gap-1.5 text-sm font-semibold
                           bg-card border border-border text-white hover:bg-card-hover transition-colors
                           disabled:opacity-30 disabled:cursor-not-allowed"
              >
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><polyline points="1 4 1 10 7 10"/><path d="M3.51 15a9 9 0 1 0 2.13-9.36L1 10"/></svg>
                Undo
              </button>
              <div className="h-10 flex items-center gap-1 px-2 rounded-lg bg-card border border-border">
                <input
                  type="number" min={1} max={max} value={undoSteps}
                  onChange={(e) => setUndoSteps(Math.max(1, parseInt(e.target.value, 10) || 1))}
                  className="w-8 text-center bg-transparent text-white text-sm font-semibold focus:outline-none"
                />
                <span className="text-[10px] text-slate-500 uppercase font-semibold">steps</span>
              </div>

              {/* Reset */}
              <div className="relative">
                <button
                  onClick={() => setShowReset(!showReset)}
                  className="h-10 px-4 rounded-lg flex items-center gap-1.5 text-sm font-semibold
                             bg-amber text-black hover:bg-amber-dark transition-colors"
                >
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><path d="M21 12a9 9 0 0 0-9-9 9.75 9.75 0 0 0-6.74 2.74L3 8"/><path d="M3 3v5h5"/></svg>
                  Reset
                </button>
                {showReset && (
                  <div className="absolute top-full mt-2 left-0 w-56 z-20 bg-card border border-border rounded-lg p-3 shadow-lg">
                    <p className="text-[10px] font-semibold text-slate-500 uppercase tracking-wide mb-2">
                      {isL2 || isL3 ? "Clear board" : "Restart options"}
                    </p>
                    {!isL2 && !isL3 && (
                      <label className="flex items-center gap-2 text-sm text-slate-300 mb-3 cursor-pointer">
                        <input
                          type="checkbox" checked={keepOne}
                          onChange={(e) => setKeepOne(e.target.checked)}
                          className="w-3.5 h-3.5 rounded accent-primary"
                        />
                        Keep #1 in same cell
                      </label>
                    )}
                    {isL2 && <p className="text-xs text-slate-500 mb-3">Inner 5x5 stays. Only ring cleared.</p>}
                    {isL3 && <p className="text-xs text-slate-500 mb-3">Outer ring stays. Inner 5x5 cleared (except #1).</p>}
                    <div className="flex gap-2">
                      <button onClick={handleClear} disabled={busy}
                        className="flex-1 py-1.5 rounded-md bg-amber text-black text-xs font-bold hover:bg-amber-dark transition-colors disabled:opacity-40">
                        Confirm
                      </button>
                      <button onClick={() => setShowReset(false)}
                        className="flex-1 py-1.5 rounded-md bg-bg border border-border text-slate-400 text-xs font-bold hover:bg-card transition-colors">
                        Cancel
                      </button>
                    </div>
                  </div>
                )}
              </div>
            </div>
            )}
          </div>

          {/* Right sidebar */}
          <div className="w-full lg:w-64 flex flex-col gap-4">

            {/* Next Number - US3 */}
            {!gameState.hasWon && (
              <div className="bg-card border border-border rounded-xl overflow-hidden">
                <div className="bg-primary/10 border-b border-border px-4 py-5 text-center">
                  <p className="text-[10px] font-semibold text-slate-400 uppercase tracking-wide mb-1">
                    Next Number
                  </p>
                  <p className="text-5xl font-black text-primary leading-none">{next}</p>
                </div>
                <div className="px-4 py-2.5 text-xs text-slate-400 text-center">
                  {isL2 ? "Place on outer ring" : isL3 ? "Place at intersection" : "Place in adjacent cell"}
                </div>
              </div>
            )}

            {/* Level 1 won */}
            {gameState.level === 1 && gameState.hasWon && (
              <div className="bg-card border border-green/20 rounded-xl p-5 text-center">
                <p className="text-green text-lg font-bold mb-1">Level 1 Complete!</p>
                <p className="text-slate-400 text-sm mb-4">Score: {gameState.score} pts</p>
                {isViewingCompleted ? (
                  <div className="flex flex-col gap-2">
                    <p className="text-xs text-slate-500">Viewing your completed result</p>
                    <button
                      onClick={() => navigate("/select-level", { state: { username, playAgainLevel: 1 } })}
                      className="w-full py-2.5 rounded-lg text-sm font-bold bg-primary text-white hover:bg-primary-dark transition-colors"
                    >
                      Play Again
                    </button>
                  </div>
                ) : (
                  <button
                    onClick={handleExpand}
                    disabled={busy}
                    className="w-full py-2.5 rounded-lg text-sm font-bold bg-primary text-white hover:bg-primary-dark transition-colors disabled:opacity-50"
                  >
                    Expand to Level 2
                  </button>
                )}
              </div>
            )}

            {/* Level 2 won */}
            {gameState.level === 2 && gameState.hasWon && (
              <div className="bg-card border border-accent/20 rounded-xl p-5 text-center">
                <p className="text-accent text-lg font-bold mb-1">Level 2 Complete!</p>
                <p className="text-slate-400 text-sm mb-4">Final Score: {gameState.score} pts</p>
                {isViewingCompleted ? (
                  <div className="flex flex-col gap-2">
                    <p className="text-xs text-slate-500">Viewing your completed result</p>
                    <button
                      onClick={() => navigate("/select-level", { state: { username, playAgainLevel: 2 } })}
                      className="w-full py-2.5 rounded-lg text-sm font-bold bg-accent text-white hover:bg-accent/90 transition-colors"
                    >
                      Play Again
                    </button>
                  </div>
                ) : (
                  <button
                    onClick={handleExpandLevel3}
                    disabled={busy}
                    className="w-full py-2.5 rounded-lg text-sm font-bold bg-accent text-white hover:bg-accent/90 transition-colors disabled:opacity-50"
                  >
                    Expand to Level 3
                  </button>
                )}
              </div>
            )}

            {/* Level 3 won */}
            {gameState.level === 3 && gameState.hasWon && (
              <div className="bg-card border border-amber/20 rounded-xl p-5 text-center">
                <p className="text-amber text-lg font-bold mb-1">Level 3 Complete!</p>
                <p className="text-slate-400 text-sm mb-2">You are a Number Logic Master!</p>
                <p className="text-slate-500 text-xs mb-4">Final Score: {gameState.score} pts</p>
                {isViewingCompleted && (
                  <div className="flex flex-col gap-2">
                    <p className="text-xs text-slate-500">Viewing your completed result</p>
                    <button
                      onClick={() => navigate("/select-level", { state: { username, playAgainLevel: 3 } })}
                      className="w-full py-2.5 rounded-lg text-sm font-bold bg-amber text-black hover:bg-amber/90 transition-colors"
                    >
                      Play Again
                    </button>
                  </div>
                )}
              </div>
            )}

            {/* How to play */}
            <div className="bg-card border border-border rounded-xl p-4">
              <p className="text-[10px] font-semibold text-slate-500 uppercase tracking-wide mb-2">Rules</p>
              <ul className="space-y-1.5 text-xs text-slate-400 leading-relaxed">
                <li>Level 1: +1 point for diagonal moves only</li>
                <li>Level 2 & 3: +1 point for every number placed</li>
                <li>Undo/Reset/Clear: -1 point per number rolled back</li>
                <li>Diagonal placement gives +1 bonus point (L1 only)</li>
                {isL2 && <li className="text-accent">Numbers 2-25 go on the outer ring</li>}
                {isL3 && (
                  <>
                    <li className="text-amber">Place numbers at the unique intersection</li>
                    <li className="text-amber">Yellow corners must be on diagonals</li>
                  </>
                )}
              </ul>
            </div>
          </div>
        </div>

        {/* Toast */}
        {toast && (
          <div className="fixed bottom-5 left-1/2 -translate-x-1/2 z-50">
            <div className={`px-4 py-2.5 rounded-lg text-sm font-semibold shadow-lg ${
              toast.type === "err"
                ? "bg-red/10 border border-red/20 text-red"
                : "bg-green/10 border border-green/20 text-green"
            }`}>
              {toast.text}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default GamePage;
