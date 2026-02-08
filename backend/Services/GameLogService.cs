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
                Score = session.Level == 1 ? session.GameLogic.GetScore() : null,
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
