import { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { createNewGame, getProgress, getCompletedGame } from "../services/gameApi";

const SelectLevelPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const stateUsername = (location.state as { username?: string })?.username;
  const username = (stateUsername?.trim() || "Player");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [level1Completed, setLevel1Completed] = useState(false);
  const [level2Completed, setLevel2Completed] = useState(false);
  const [level3Completed, setLevel3Completed] = useState(false);
  const [totalScore, setTotalScore] = useState(0);

  const needsRedirect = !stateUsername || !String(stateUsername).trim();

  useEffect(() => {
    if (needsRedirect) {
      navigate("/", { replace: true });
    }
  }, [needsRedirect, navigate]);
  // Load progress from backend logs
  useEffect(() => {
    getProgress(username)
      .then((p) => {
        setLevel1Completed(p.level1Completed);
        setLevel2Completed(p.level2Completed);
        setLevel3Completed(p.level3Completed);
        setTotalScore(p.totalScore);
      })
      .catch(() => { /* ignore */ });
  }, [username]);

  if (needsRedirect) return null;

  const getCompletedLevel1Board = async (user: string): Promise<number[][] | undefined> => {
    try {
      const state = await getCompletedGame(user, 1);
      const board = state?.board;
      if (!Array.isArray(board) || board.length !== 5) return undefined;
      for (const row of board) if (!Array.isArray(row) || row.length !== 5) return undefined;
      return board;
    } catch { return undefined; }
  };

  const getCompletedLevel2Board = async (user: string): Promise<number[][] | undefined> => {
    try {
      const state = await getCompletedGame(user, 2);
      const board = state?.board;
      if (!Array.isArray(board) || board.length !== 7) return undefined;
      for (const row of board) if (!Array.isArray(row) || row.length !== 7) return undefined;
      return board;
    } catch { return undefined; }
  };

  // Handle "Play Again" from completed view
  useEffect(() => {
    const playAgainLevel = (location.state as { playAgainLevel?: number })?.playAgainLevel;
    if (playAgainLevel == null || loading) return;
    const run = async () => {
      setLoading(true);
      setError(null);
      try {
        const level1Board = playAgainLevel === 2 ? await getCompletedLevel1Board(username) : undefined;
        const level2Board = playAgainLevel === 3 ? await getCompletedLevel2Board(username) : undefined;
        const res = await createNewGame(username, playAgainLevel, level1Board, level2Board);
        navigate("/game", {
          state: { username, gameId: res.gameId, gameState: res.gameState },
          replace: true,
        });
      } catch (e) {
        setError(e instanceof Error ? e.message : "Failed to create game");
      } finally {
        setLoading(false);
      }
    };
    run();
  }, [username, navigate, loading]);

  const handlePlay = async (level: number = 1) => {
    const isCompleted = level === 1 ? level1Completed : level === 2 ? level2Completed : level3Completed;

    if (isCompleted) {
      try {
        const gameState = await getCompletedGame(username, level);
        navigate("/game", {
          state: { username, gameState, isViewingCompleted: true },
        });
        return;
      } catch {
        // Fall through to create new game if fetch fails
      }
    }

    setLoading(true);
    setError(null);
    try {
      const level1Board = level === 2 ? await getCompletedLevel1Board(username) : undefined;
      const level2Board = level === 3 ? await getCompletedLevel2Board(username) : undefined;
      const res = await createNewGame(username, level, level1Board, level2Board);
      navigate("/game", {
        state: { username, gameId: res.gameId, gameState: res.gameState },
      });
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to create game");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-bg p-6">
      <div className="max-w-3xl mx-auto">

        {/* Header */}
        <div className="flex items-center justify-between mb-8">
          <div className="flex items-center gap-3">
            <button
              onClick={() => navigate("/")}
              className="w-9 h-9 rounded-lg flex items-center justify-center bg-card border border-border hover:bg-card-hover transition-colors"
            >
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="m15 18-6-6 6-6"/></svg>
            </button>
            <h1 className="text-xl font-bold text-white">Select Level</h1>
          </div>

          <div className="flex items-center gap-2">
            <div className="px-3 py-1.5 rounded-lg bg-card border border-border text-sm">
              <span className="text-slate-400">Player: </span>
              <span className="text-white font-semibold">{username}</span>
            </div>
            <div className="px-3 py-1.5 rounded-lg bg-amber/10 border border-amber/20 text-sm">
              <span className="text-amber font-bold">{totalScore} pts</span>
            </div>
          </div>
        </div>

        {error && (
          <div className="mb-4 p-3 rounded-lg bg-red/10 border border-red/20 text-red text-sm">
            {error}
          </div>
        )}

        {/* Level cards */}
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">

          {/* Level 1 */}
          <div className="bg-card rounded-xl border border-border p-5 flex flex-col">
            <div className="flex items-center justify-between mb-3">
              <span className={`text-xs font-bold px-2.5 py-1 rounded-md uppercase tracking-wide ${
                level1Completed
                  ? 'text-accent bg-accent/10 border border-accent/20'
                  : 'text-primary bg-primary/10'
              }`}>
                Level 1 {level1Completed && '✓'}
              </span>
              <span className="text-xs text-slate-500">5x5 Grid</span>
            </div>
            <h2 className="text-lg font-bold text-white mb-1">The Inner Grid</h2>
            <div className="flex items-center gap-2 mb-3">
              <span className="text-[10px] bg-primary/10 text-primary px-1.5 py-0.5 rounded font-bold uppercase tracking-wider">⏱️ 60s limit</span>
            </div>
            <p className="text-sm text-slate-400 mb-4 flex-1">
              Place numbers 1-25 sequentially in adjacent cells. Diagonal placements earn bonus points.
            </p>
            <button
              onClick={() => handlePlay(1)}
              disabled={loading}
              className="w-full py-2.5 rounded-lg text-sm font-bold
                         bg-primary text-white hover:bg-primary-dark transition-colors
                         disabled:opacity-50"
            >
              {loading ? "Starting..." : level1Completed ? "View Result" : "Play Now"}
            </button>
          </div>

          {/* Level 2 */}
          <div className={`bg-card rounded-xl border border-border p-5 flex flex-col relative overflow-hidden ${!level1Completed ? 'opacity-60' : ''}`}>
            {!level1Completed && (
              <div className="absolute inset-0 bg-bg/60 flex flex-col items-center justify-center z-10">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="text-slate-500 mb-1"><rect width="18" height="11" x="3" y="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                <p className="text-sm font-semibold text-slate-400">Locked</p>
                <p className="text-xs text-slate-500">Complete Level 1 first</p>
              </div>
            )}
            <div className="flex items-center justify-between mb-3">
              <span className={`text-xs font-bold px-2.5 py-1 rounded-md uppercase tracking-wide border ${
                level2Completed
                  ? 'text-accent bg-accent/10 border-accent/20'
                  : level1Completed
                    ? 'text-accent bg-accent/10 border-accent/20'
                    : 'text-slate-500 bg-card border-border'
              }`}>
                Level 2 {level2Completed && '✓'}
              </span>
              <span className="text-xs text-slate-500">7x7 Grid</span>
            </div>
            <h2 className="text-lg font-bold text-white mb-1">Outer Ring Expansion</h2>
            <div className="flex items-center gap-2 mb-3">
              <span className="text-[10px] bg-accent/10 text-accent px-1.5 py-0.5 rounded font-bold uppercase tracking-wider">⏱️ 120s limit</span>
            </div>
            <p className="text-sm text-slate-400 mb-4 flex-1">
              The board expands with a ring of 24 cells. Place numbers 2-25 on the outer ring based on their positions in the inner board.
            </p>
            {level1Completed && (
              <button
                onClick={() => handlePlay(2)}
                disabled={loading}
                className="w-full py-2.5 rounded-lg text-sm font-bold
                           bg-accent text-white hover:bg-accent/90 transition-colors
                           disabled:opacity-50"
              >
                {loading ? "Starting..." : level2Completed ? "View Result" : "Play Level 2"}
              </button>
            )}
          </div>

          {/* Level 3 */}
          <div className={`bg-card rounded-xl border border-border p-5 flex flex-col relative overflow-hidden ${!level2Completed ? 'opacity-60' : ''}`}>
            {!level2Completed && (
              <div className="absolute inset-0 bg-bg/60 flex flex-col items-center justify-center z-10">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="text-slate-500 mb-1"><rect width="18" height="11" x="3" y="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                <p className="text-sm font-semibold text-slate-400">Locked</p>
                <p className="text-xs text-slate-500">Complete Level 2 first</p>
              </div>
            )}
            <div className="flex items-center justify-between mb-3">
              <span className={`text-xs font-bold px-2.5 py-1 rounded-md uppercase tracking-wide border ${
                level3Completed
                  ? 'text-amber bg-amber/10 border-amber/20'
                  : level2Completed
                    ? 'text-amber bg-amber/10 border-amber/20'
                    : 'text-slate-500 bg-card border-border'
              }`}>
                Level 3 {level3Completed && '✓'}
              </span>
              <span className="text-xs text-slate-500">7x7 Grid</span>
            </div>
            <h2 className="text-lg font-bold text-white mb-1">Intersection Master</h2>
            <div className="flex items-center gap-2 mb-3">
              <span className="text-[10px] bg-amber/10 text-amber px-1.5 py-0.5 rounded font-bold uppercase tracking-wider">⏱️ 180s limit</span>
            </div>
            <p className="text-sm text-slate-400 mb-4 flex-1">
              The final challenge. Fill the inner 5x5 grid using the outer ring as your guide. Master the intersection and diagonal rules.
            </p>
            {level2Completed && (
              <button
                onClick={() => handlePlay(3)}
                disabled={loading}
                className="w-full py-2.5 rounded-lg text-sm font-bold
                           bg-amber text-black hover:bg-amber/90 transition-colors
                           disabled:opacity-50"
              >
                {loading ? "Starting..." : level3Completed ? "View Result" : "Play Level 3"}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default SelectLevelPage;
