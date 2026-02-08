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
            var gameId = _gameStateService.CreateNewGame(request.PlayerUsername, request.Level);
            var gameState = _gameStateService.GetGameState(gameId);
            return Ok(new { GameId = gameId, GameState = gameState });
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
                var session = _gameStateService.GetSession(request.GameId);
                if (session != null)
                {
                    var duration = (int)(DateTime.UtcNow - session.StartTime).TotalSeconds;
                    _gameLogService.LogGameCompletion(session, duration);
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
    }
}
