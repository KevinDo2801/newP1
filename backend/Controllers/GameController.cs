using backendAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace backendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly GameStateService _gameStateService;
        private readonly GameLogService _gameLogService;

        public GameController(GameStateService gameStateService, GameLogService gameLogService)
        {
            _gameStateService = gameStateService;
            _gameLogService = gameLogService;
        }

        [HttpPost("new")]
        public IActionResult CreateNewGame([FromBody] NewGameRequest request)
        {
            var gameId = _gameStateService.CreateNewGame(request);
            var gameState = _gameStateService.GetGameState(gameId);
            return Ok(new { GameId = gameId, GameState = gameState });
        }

        [HttpGet("progress/{username}")]
        public IActionResult GetProgress(string username)
        {
            var progress = _gameLogService.GetProgress(username);
            return Ok(progress);
        }

        [HttpGet("completed/{username}/{level:int}")]
        public IActionResult GetCompletedGame(string username, int level)
        {
            if (level < 1 || level > 3)
                return BadRequest(new { Error = "Level must be 1, 2, or 3" });
            var gameState = _gameLogService.GetCompletedGame(username, level);
            if (gameState == null)
                return NotFound(new { Error = "No completed game found" });
            return Ok(gameState);
        }

        [HttpGet("{gameId}")]
        public IActionResult GetGameState(string gameId)
        {
            try
            {
                var gameState = _gameStateService.GetGameState(gameId);
                return Ok(gameState);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Error = "Game not found" });
            }
        }

        [HttpPost("place")]
        public IActionResult PlaceNumber([FromBody] PlaceNumberRequest request)
        {
            var response = _gameStateService.PlaceNumber(request.GameId, request.Row, request.Col);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }

            // Check if game is won
            if (response.GameState?.HasWon == true)
            {
                var finalScore = _gameStateService.CalculateFinalScore(request.GameId);
                response.GameState.Score = finalScore;

                var session = _gameStateService.GetSession(request.GameId);
                if (session != null)
                {
                    var duration = (int)(DateTime.UtcNow - session.StartTime).TotalSeconds;
                    
                    // Log with final score
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
                        Score = finalScore,
                        Board = boardArray
                    };

                    var fileName = $"game_log_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{session.GameId}.json";
                    var filePath = Path.Combine("logs", fileName);
                    var json = System.Text.Json.JsonSerializer.Serialize(log, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    System.IO.File.WriteAllText(filePath, json);
                }
            }

            return Ok(response);
        }

        [HttpPost("undo")]
        public IActionResult Undo([FromBody] UndoRequest request)
        {
            var count = request.Count <= 0 ? 1 : request.Count;
            var success = _gameStateService.Undo(request.GameId, count);
            if (!success)
            {
                return BadRequest(new { Error = "Cannot undo" });
            }

            var gameState = _gameStateService.GetGameState(request.GameId);
            return Ok(new { GameState = gameState });
        }

        [HttpPost("clear")]
        public IActionResult ClearBoard([FromBody] ClearBoardRequest request)
        {
            var success = _gameStateService.ClearBoard(request.GameId, request.KeepOneInPlace);
            if (!success)
            {
                return BadRequest(new { Error = "Cannot clear board" });
            }

            var gameState = _gameStateService.GetGameState(request.GameId);
            return Ok(new { GameState = gameState });
        }

        /// <summary>Expand to Level 2 after Level 1 is won. Inner 5x5 kept, outer ring of 24 cells added. US8.</summary>
        [HttpPost("expand-level2")]
        public IActionResult ExpandToLevel2([FromBody] ExpandLevel2Request request)
        {
            var success = _gameStateService.ExpandToLevel2(request.GameId);
            if (!success)
            {
                return BadRequest(new { Error = "Cannot expand to Level 2 (Level 1 must be completed first)" });
            }

            var gameState = _gameStateService.GetGameState(request.GameId);
            return Ok(new { GameState = gameState });
        }

        /// <summary>Expand to Level 3 after Level 2 is won. Inner 5x5 erased except 1, outer ring kept. US9.</summary>
        [HttpPost("expand-level3")]
        public IActionResult ExpandToLevel3([FromBody] ExpandLevel3Request request)
        {
            var success = _gameStateService.ExpandToLevel3(request.GameId);
            if (!success)
            {
                return BadRequest(new { Error = "Cannot expand to Level 3 (Level 2 must be completed first)" });
            }

            var gameState = _gameStateService.GetGameState(request.GameId);
            return Ok(new { GameState = gameState });
        }
    }
}
