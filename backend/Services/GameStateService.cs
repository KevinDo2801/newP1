using backendAPI.Models;
using System.Collections.Concurrent;

namespace backendAPI
{
    public class GameStateService
    {
        private readonly ConcurrentDictionary<string, GameSession> _sessions = new();
        private readonly Random _random = new();

        public class GameSession
        {
            public string GameId { get; set; } = string.Empty;
            public string PlayerUsername { get; set; } = string.Empty;
            public int Level { get; set; }
            public GameLogic GameLogic { get; set; } = new();
            public DateTime StartTime { get; set; }
            public Stack<MoveHistory> MoveHistory { get; set; } = new();
            public int FirstNumberRow { get; set; } = -1;
            public int FirstNumberCol { get; set; } = -1;
        }

        public class MoveHistory
        {
            public int[,] Board { get; set; } = new int[0, 0];
            public int CurrentNumber { get; set; }
            public int Score { get; set; }
            public int LastRow { get; set; }
            public int LastCol { get; set; }
        }

        public string CreateNewGame(string playerUsername, int level, int[][]? level1Board = null, int[][]? level2Board = null)
        {
            if (level < 1 || level > 3)
                level = 1;
            var gameId = Guid.NewGuid().ToString();
            var session = new GameSession
            {
                GameId = gameId,
                PlayerUsername = playerUsername,
                Level = level,
                StartTime = DateTime.UtcNow
            };

            if (level == 1)
            {
                session.GameLogic.InitializeNewGame();
                var row = _random.Next(0, 5);
                var col = _random.Next(0, 5);
                session.GameLogic.PlaceFirstNumber(row, col);
                session.FirstNumberRow = row;
                session.FirstNumberCol = col;
            }
            else if (level == 2)
            {
                var inner5x5 = TryParseLevel1Board(level1Board);
                if (inner5x5 != null)
                    session.GameLogic.InitializeLevel2FromInnerBoard(inner5x5);
                else
                    session.GameLogic.InitializeLevel2Game(_random);
                // For Level 2, first number is already placed (1-25 are pre-filled)
                // Find position of number 25 (last number in inner grid)
                var board = session.GameLogic.GetBoard();
                for (int r = 1; r <= 5; r++)
                {
                    for (int c = 1; c <= 5; c++)
                    {
                        if (board[r, c] == 25)
                        {
                            session.FirstNumberRow = r;
                            session.FirstNumberCol = c;
                            break;
                        }
                    }
                    if (session.FirstNumberRow >= 0) break;
                }
            }
            else if (level == 3)
            {
                var board7x7 = TryParseLevel2Board(level2Board);
                if (board7x7 != null)
                    session.GameLogic.InitializeLevel3FromLevel2Board(board7x7);
                else
                    session.GameLogic.InitializeLevel3Game(_random);
                // Find position of number 1
                var board = session.GameLogic.GetBoard();
                for (int r = 1; r <= 5; r++)
                {
                    for (int c = 1; c <= 5; c++)
                    {
                        if (board[r, c] == 1)
                        {
                            session.FirstNumberRow = r;
                            session.FirstNumberCol = c;
                            break;
                        }
                    }
                    if (session.FirstNumberRow >= 0) break;
                }
            }

            _sessions[gameId] = session;
            return gameId;
        }

        /// <summary>Expand current game to Level 2 after Level 1 is won. Inner 5x5 kept, outer ring added.</summary>
        public bool ExpandToLevel2(string gameId)
        {
            var session = GetSession(gameId);
            if (session == null || session.Level != 1 || !session.GameLogic.HasWon())
                return false;
            if (!session.GameLogic.ExpandToLevel2())
                return false;
            session.Level = 2;
            session.MoveHistory.Clear();
            return true;
        }

        /// <summary>Expand current game to Level 3 after Level 2 is won. Inner 5x5 erased except number 1, outer ring kept.</summary>
        public bool ExpandToLevel3(string gameId)
        {
            var session = GetSession(gameId);
            if (session == null || session.Level != 2 || !session.GameLogic.HasWon())
                return false;
            if (!session.GameLogic.ExpandToLevel3())
                return false;
            session.Level = 3;
            session.MoveHistory.Clear();
            return true;
        }

        /// <summary>Parse and validate Level 1 board (5x5, numbers 1-25). Returns null if invalid.</summary>
        private static int[,]? TryParseLevel1Board(int[][]? arr)
        {
            if (arr == null || arr.Length != 5) return null;
            for (int i = 0; i < 5; i++)
                if (arr[i] == null || arr[i].Length != 5) return null;
            var board = new int[5, 5];
            var seen = new HashSet<int>();
            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    int v = arr[r][c];
                    if (v < 1 || v > 25 || !seen.Add(v))
                        return null;
                    board[r, c] = v;
                }
            }
            return seen.Count == 25 ? board : null;
        }

        /// <summary>Parse and validate Level 2 completed board (7x7). Outer ring has 2-25, inner 5x5 has 1-25. Returns null if invalid.</summary>
        private static int[,]? TryParseLevel2Board(int[][]? arr)
        {
            if (arr == null || arr.Length != 7) return null;
            for (int i = 0; i < 7; i++)
                if (arr[i] == null || arr[i].Length != 7) return null;
            var board = new int[7, 7];
            var outerSeen = new HashSet<int>();
            bool hasOne = false;
            for (int r = 0; r < 7; r++)
            {
                for (int c = 0; c < 7; c++)
                {
                    int v = arr[r][c];
                    bool isOuter = r == 0 || r == 6 || c == 0 || c == 6;
                    if (isOuter)
                    {
                        if (v < 2 || v > 25 || !outerSeen.Add(v)) return null;
                    }
                    else
                    {
                        if (v == 1) hasOne = true;
                        // Inner can have 1-25 (completed L2) or just 1
                    }
                    board[r, c] = v;
                }
            }
            return outerSeen.Count == 24 && hasOne ? board : null;
        }

        public GameSession? GetSession(string gameId)
        {
            return _sessions.TryGetValue(gameId, out var session) ? session : null;
        }

        public GameStateDto GetGameState(string gameId)
        {
            var session = GetSession(gameId);
            if (session == null)
                throw new KeyNotFoundException("Game not found");

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

            return new GameStateDto
            {
                PlayerUsername = session.PlayerUsername,
                Level = session.GameLogic.GetLevel(),
                Board = boardArray,
                CurrentNumber = session.GameLogic.GetCurrentNumber(),
                NextNumberToPlace = session.GameLogic.GetCurrentNumber(),
                StartTime = session.StartTime.ToString("O"),
                Score = session.GameLogic.GetScore(),
                LastRow = session.GameLogic.GetLastRow() >= 0 ? session.GameLogic.GetLastRow() : null,
                LastCol = session.GameLogic.GetLastCol() >= 0 ? session.GameLogic.GetLastCol() : null,
                HasWon = session.GameLogic.HasWon()
            };
        }

        public PlaceNumberResponse PlaceNumber(string gameId, int row, int col)
        {
            var session = GetSession(gameId);
            if (session == null)
            {
                return new PlaceNumberResponse
                {
                    Success = false,
                    ErrorMessage = "Game not found"
                };
            }

            int rows = session.GameLogic.GetBoardRows();
            int cols = session.GameLogic.GetBoardCols();
            var currentBoard = session.GameLogic.GetBoard();
            var boardCopy = new int[rows, cols];
            Array.Copy(currentBoard, boardCopy, rows * cols);

            var history = new MoveHistory
            {
                Board = boardCopy,
                CurrentNumber = session.GameLogic.GetCurrentNumber(),
                Score = session.GameLogic.GetScore(),
                LastRow = session.GameLogic.GetLastRow(),
                LastCol = session.GameLogic.GetLastCol()
            };

            bool success = session.GameLogic.PlaceNumber(row, col);

            if (!success)
            {
                return new PlaceNumberResponse
                {
                    Success = false,
                    IsValid = false,
                    GameState = GetGameState(gameId),
                    ErrorMessage = "Invalid placement"
                };
            }

            session.MoveHistory.Push(history);

            return new PlaceNumberResponse
            {
                Success = true,
                IsValid = true,
                GameState = GetGameState(gameId)
            };
        }

        /// <summary>Undo the last count moves (default 1). Returns true if at least one move was undone.</summary>
        public bool Undo(string gameId, int count = 1)
        {
            var session = GetSession(gameId);
            if (session == null || count <= 0)
                return false;
            int undone = 0;
            while (session.MoveHistory.Count > 0 && undone < count)
            {
                var history = session.MoveHistory.Pop();
                session.GameLogic.LoadGame(
                    history.Board,
                    history.CurrentNumber,
                    history.Score,
                    history.LastRow,
                    history.LastCol
                );
                undone++;
            }
            return undone > 0;
        }

        /// <summary>Clear board. Level 1: optional keep 1 in place or random. Level 2: clear outer ring only.</summary>
        public bool ClearBoard(string gameId, bool keepOneInPlace = true)
        {
            var session = GetSession(gameId);
            if (session == null)
                return false;

            session.GameLogic.ClearBoard(keepOneInPlace, keepOneInPlace ? null : _random);

            if (session.GameLogic.GetLevel() == 1 && !keepOneInPlace)
            {
                session.FirstNumberRow = session.GameLogic.GetLastRow();
                session.FirstNumberCol = session.GameLogic.GetLastCol();
            }

            session.MoveHistory.Clear();
            return true;
        }
    }
}
