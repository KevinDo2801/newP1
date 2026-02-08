import { useState } from "react";
import { useNavigate } from "react-router-dom";

const OnboardPage = () => {
  const [username, setUsername] = useState("");
  const navigate = useNavigate();

  const handleStart = () => {
    if (username.trim()) {
      navigate("/select-level", { state: { username: username.trim() } });
    }
  };

  return (
    <div className="min-h-screen bg-bg flex items-center justify-center p-6">
      <div className="w-full max-w-md">
        {/* Logo / Title */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-primary/10 border border-primary/20 mb-4">
            <span className="text-primary text-2xl font-black">#</span>
          </div>
          <h1 className="text-3xl font-black text-white tracking-tight">
            Number Logic Game
          </h1>
          <p className="text-slate-400 text-sm mt-2">
            Place numbers sequentially on the grid using adjacency logic.
          </p>
        </div>

        {/* Card */}
        <div className="bg-card rounded-2xl border border-border p-6">
          <label className="block text-xs font-semibold text-slate-400 uppercase tracking-wide mb-2">
            Player Name
          </label>
          <input
            type="text"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && handleStart()}
            placeholder="Enter your name"
            className="w-full px-4 py-3 rounded-xl bg-bg border border-border text-white
                       placeholder-slate-500 text-sm
                       focus:outline-none focus:border-primary/50 focus:ring-2 focus:ring-primary/20
                       transition-colors"
          />

          <button
            disabled={!username.trim()}
            onClick={handleStart}
            className="w-full mt-4 py-3 px-6 rounded-xl text-sm font-bold
                       bg-primary text-white
                       hover:bg-primary-dark transition-colors
                       disabled:opacity-30 disabled:cursor-not-allowed"
          >
            Start Game
          </button>
        </div>

        <p className="text-center text-slate-600 text-xs mt-6">
          CEN4020 - Project 1
        </p>
      </div>
    </div>
  );
};

export default OnboardPage;
