namespace backendAPI.Models
{
    public class GameStateDto
    {
        public string PlayerUsername { get; set; } = string.Empty;
        public int Level { get; set; }
        public int[][] Board { get; set; } = Array.Empty<int[]>();
        public int CurrentNumber { get; set; }
        /// <summary>Next number to place (US3: displayed to player, not auto-placed).</summary>
        public int NextNumberToPlace { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public int Score { get; set; }
        public int? LastRow { get; set; }
        public int? LastCol { get; set; }
        public bool IsValid { get; set; } = true;
        public bool HasWon { get; set; }
    }

    public class PlaceNumberRequest
    {
        public string GameId { get; set; } = string.Empty;
        public int Row { get; set; }
        public int Col { get; set; }
    }

    public class PlaceNumberResponse
    {
        public bool Success { get; set; }
        public bool IsValid { get; set; }
        public GameStateDto? GameState { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class NewGameRequest
    {
        public string PlayerUsername { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        /// <summary>Optional. When Level=2, use this 5x5 board as inner grid (from completed Level 1).</summary>
        public int[][]? Level1Board { get; set; }
        /// <summary>Optional. When Level=3, use this 7x7 board - outer ring kept, inner 5x5 cleared except 1 (from completed Level 2).</summary>
        public int[][]? Level2Board { get; set; }
    }

    public class UndoRequest
    {
        public string GameId { get; set; } = string.Empty;
        /// <summary>Number of moves to undo (default 1). US5: undo as many as wanted.</summary>
        public int Count { get; set; } = 1;
    }

    public class ClearBoardRequest
    {
        public string GameId { get; set; } = string.Empty;
        /// <summary>True = keep 1 in same cell; False = randomly re-allocate 1. US4.</summary>
        public bool KeepOneInPlace { get; set; } = true;
    }

    public class ExpandLevel2Request
    {
        public string GameId { get; set; } = string.Empty;
    }

    public class ExpandLevel3Request
    {
        public string GameId { get; set; } = string.Empty;
    }
}
