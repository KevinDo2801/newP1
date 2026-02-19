using backendAPI.Models;
using System.Text.Json;

namespace backendAPI
{
    public class GameLogService
    {
        private readonly string _logsDirectory = "logs";

        public GameLogService()
        {
            if (!Directory.Exists(_logsDirectory))
            {
                Directory.CreateDirectory(_logsDirectory);
            }
        }

        /// <summary>Returns which levels the player has completed and total score (from logs).</summary>
        public ProgressDto GetProgress(string username)
        {
            var completed = new HashSet<int>();
            int totalScore = 0;
            var files = Directory.GetFiles(_logsDirectory, "game_log_*.json");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var log = JsonSerializer.Deserialize<GameLogDto>(json, options);
                    if (log == null) continue;
                    if (!string.Equals(log.PlayerUsername?.Trim(), username?.Trim(), StringComparison.OrdinalIgnoreCase))
                        continue;
                    
                    if (log.Level >= 1 && log.Level <= 3)
                        completed.Add(log.Level);
                    
                    totalScore += log.Score ?? 0;
                }
                catch { /* skip invalid files */ }
            }

            return new ProgressDto
            {
                Level1Completed = completed.Contains(1),
                Level2Completed = completed.Contains(2),
                Level3Completed = completed.Contains(3),
                TotalScore = totalScore
            };
        }

        /// <summary>Returns the most recent completed game state for viewing.</summary>
        public GameStateDto? GetCompletedGame(string username, int level)
        {
            var files = Directory.GetFiles(_logsDirectory, "game_log_*.json");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            GameLogDto? best = null;
            DateTime? bestTime = null;

            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var log = JsonSerializer.Deserialize<GameLogDto>(json, options);
                    if (log == null || log.Level != level) continue;
                    if (!string.Equals(log.PlayerUsername?.Trim(), username?.Trim(), StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (!DateTime.TryParse(log.CompletedAt, out var dt)) continue;
                    if (bestTime == null || dt > bestTime)
                    {
                        best = log;
                        bestTime = dt;
                    }
                }
                catch { /* skip */ }
            }

            if (best == null) return null;

            (int row, int col) pos25 = (-1, -1);
            var board = best.Board;
            for (int r = 0; r < board.Length; r++)
            {
                for (int c = 0; c < board[r].Length; c++)
                {
                    if (board[r][c] == 25) { pos25 = (r, c); break; }
                }
                if (pos25.row >= 0) break;
            }

            return new GameStateDto
            {
                PlayerUsername = best.PlayerUsername ?? string.Empty,
                Level = best.Level,
                Board = best.Board ?? Array.Empty<int[]>(),
                CurrentNumber = 25,
                NextNumberToPlace = 25,
                StartTime = best.CompletedAt,
                Score = best.Score ?? 0,
                LastRow = pos25.row >= 0 ? pos25.row : null,
                LastCol = pos25.col >= 0 ? pos25.col : null,
                IsValid = true,
                HasWon = true
            };
        }

        public void LogGameCompletion(GameStateService.GameSession session, int duration)
        {
            var board = session.GameLogic.GetBoard();
            int rows = session.GameLogic.GetBoardRows();
            int cols = session.GameLogic.GetBoardCols();
            var boardArray = new int[rows][];
            for (int i = 0; i < rows; i++)
            {
                boardArray[i] = new int[cols];
                for (int j = 0; j < cols; j++)
                {
                    boardArray[i][j] = board[i, j];
                }
            }

            var log = new GameLogDto
            {
                PlayerUsername = session.PlayerUsername,
                CompletedAt = DateTime.UtcNow.ToString("O"),
                Duration = duration,
                Level = session.Level,
                Score = session.GameLogic.GetScore(),
                Board = boardArray
            };

            var fileName = $"game_log_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{session.GameId}.json";
            var filePath = Path.Combine(_logsDirectory, fileName);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(log, options);
            File.WriteAllText(filePath, json);
        }
    }
}
