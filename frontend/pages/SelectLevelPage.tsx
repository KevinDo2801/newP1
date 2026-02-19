import { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { createNewGame } from "../services/gameApi";

const SelectLevelPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const username = (location.state as { username?: string })?.username || "Player";
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [level1Completed, setLevel1Completed] = useState(false);
  const [level2Completed, setLevel2Completed] = useState(false);

  // Check if Level 1/2 are completed
  useEffect(() => {
    const progressKey = `game_progress_${username}`;
    const savedProgress = localStorage.getItem(progressKey);
    if (savedProgress) {
      try {
        const progress = JSON.parse(savedProgress);
        if (progress.level1Completed) {
          setLevel1Completed(true);
        }
        if (progress.level2Completed) {
          setLevel2Completed(true);
        }
      } catch (e) {
        // Invalid JSON, ignore
      }
    }
  }, [username]);

  const handlePlay = async (level: number = 1) => {
    setLoading(true);
    setError(null);
    try {
      const res = await createNewGame(username, level);
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

          <div className="px-3 py-1.5 rounded-lg bg-card border border-border text-sm">
            <span className="text-slate-400">Player: </span>
            <span className="text-white font-semibold">{username}</span>
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
              <span className="text-xs font-bold text-primary bg-primary/10 px-2.5 py-1 rounded-md uppercase tracking-wide">
                Level 1
              </span>
              <span className="text-xs text-slate-500">5x5 Grid</span>
            </div>
            <h2 className="text-lg font-bold text-white mb-1">The Inner Grid</h2>
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
              {loading ? "Starting..." : "Play Now"}
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
                level1Completed 
                  ? 'text-accent bg-accent/10 border-accent/20' 
                  : 'text-slate-500 bg-card border-border'
              }`}>
                Level 2
              </span>
              <span className="text-xs text-slate-500">7x7 Grid</span>
            </div>
            <h2 className="text-lg font-bold text-white mb-1">Outer Ring Expansion</h2>
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
                {loading ? "Starting..." : "Play Level 2"}
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
                level2Completed 
                  ? 'text-amber bg-amber/10 border-amber/20' 
                  : 'text-slate-500 bg-card border-border'
              }`}>
                Level 3
              </span>
              <span className="text-xs text-slate-500">7x7 Grid</span>
            </div>
            <h2 className="text-lg font-bold text-white mb-1">Intersection Master</h2>
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
                {loading ? "Starting..." : "Play Level 3"}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default SelectLevelPage;
