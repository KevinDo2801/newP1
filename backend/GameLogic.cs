using System;

namespace backendAPI
{
    public class GameLogic
    {
        private const int Level1Size = 5;
        private const int Level2Size = 7;

        private int _level = 1;
        private int[,] _board5;
        private int[,]? _board7; // Level 2: 7x7, inner [1,1]-[5,5] is 5x5, rest is outer ring
        private int currentNumber;
        private int score;
        private int lastRow;
        private int lastCol;

        public GameLogic()
        {
            _board5 = new int[Level1Size, Level1Size];
        }

        public int GetLevel() => _level;

        // Initializes a new game: clears board, sets current number to 1, resets score
        public void InitializeNewGame()
        {
            _level = 1;
            _board7 = null;
            for (int r = 0; r < Level1Size; r++)
            {
                for (int c = 0; c < Level1Size; c++)
                {
                    _board5[r, c] = 0;
                }
            }
            currentNumber = 1;
            score = 0;
            lastRow = -1;
            lastCol = -1;
        }

        /// <summary>Returns board for current level: 5x5 for Level 1, 7x7 for Level 2.</summary>
        public int[,] GetBoard()
        {
            if (_level == 1)
                return _board5;
            return _board7!;
        }

        public int GetBoardRows() => _level == 1 ? Level1Size : Level2Size;
        public int GetBoardCols() => _level == 1 ? Level1Size : Level2Size;

        public int GetCurrentNumber() => currentNumber;
        public int GetScore() => score;
        public int GetLastRow() => lastRow;
        public int GetLastCol() => lastCol;

        /// <summary>Level 2: true if (row,col) is on the outer ring of 7x7.</summary>
        public static bool IsOuterRingCell(int row, int col)
        {
            return row == 0 || row == 6 || col == 0 || col == 6;
        }

        /// <summary>Level 2: true if (row,col) is in the inner 5x5.</summary>
        public static bool IsInnerCell(int row, int col)
        {
            return row >= 1 && row <= 5 && col >= 1 && col <= 5;
        }

        public void LoadGame(
            int[,] loadedBoard,
            int loadedCurrentNumber,
            int loadedScore,
            int loadedLastRow,
            int loadedLastCol
        )
        {
            int rows = loadedBoard.GetLength(0);
            int cols = loadedBoard.GetLength(1);
            if (rows == Level1Size && cols == Level1Size)
            {
                _level = 1;
                _board7 = null;
                _board5 = (int[,])loadedBoard.Clone();
            }
            else if (rows == Level2Size && cols == Level2Size)
            {
                _level = 2;
                _board7 = (int[,])loadedBoard.Clone();
            }
            currentNumber = loadedCurrentNumber;
            score = loadedScore;
            lastRow = loadedLastRow;
            lastCol = loadedLastCol;
        }

        public void LoadGameLevel1(int[,] board5, int loadedCurrentNumber, int loadedScore, int loadedLastRow, int loadedLastCol)
        {
            _level = 1;
            _board7 = null;
            _board5 = (int[,])board5.Clone();
            currentNumber = loadedCurrentNumber;
            score = loadedScore;
            lastRow = loadedLastRow;
            lastCol = loadedLastCol;
        }

        public void LoadGameLevel2(int[,] board7, int loadedCurrentNumber, int loadedScore, int loadedLastRow, int loadedLastCol)
        {
            _level = 2;
            _board7 = (int[,])board7.Clone();
            currentNumber = loadedCurrentNumber;
            score = loadedScore;
            lastRow = loadedLastRow;
            lastCol = loadedLastCol;
        }

        private bool IsAdjacent(int row, int col)
        {
            if (lastRow < 0 || lastCol < 0) return false;
            if (row == lastRow && col == lastCol) return false;
            return Math.Abs(row - lastRow) <= 1 && Math.Abs(col - lastCol) <= 1;
        }

        private bool IsDiagonal(int row, int col)
        {
            if (lastRow < 0 || lastCol < 0) return false;
            return Math.Abs(row - lastRow) == 1 && Math.Abs(col - lastCol) == 1;
        }

        public bool HasWon()
        {
            if (_level == 1)
                return currentNumber > 25;
            return currentNumber > 49;
        }

        public bool PlaceFirstNumber(int row, int col)
        {
            if (row < 0 || row >= Level1Size || col < 0 || col >= Level1Size)
                return false;
            _board5[row, col] = currentNumber;
            lastRow = row;
            lastCol = col;
            currentNumber++;
            return true;
        }

        public bool PlaceNumber(int row, int col)
        {
            if (_level == 1)
                return PlaceNumberLevel1(row, col);
            return PlaceNumberLevel2(row, col);
        }

        private bool PlaceNumberLevel1(int row, int col)
        {
            if (row < 0 || row >= Level1Size || col < 0 || col >= Level1Size)
                return false;
            if (_board5[row, col] != 0)
                return false;
            if (!IsAdjacent(row, col))
                return false;
            if (IsDiagonal(row, col))
                score++;
            _board5[row, col] = currentNumber;
            lastRow = row;
            lastCol = col;
            currentNumber++;
            return true;
        }

        private bool PlaceNumberLevel2(int row, int col)
        {
            if (row < 0 || row >= Level2Size || col < 0 || col >= Level2Size)
                return false;
            if (_board7![row, col] != 0)
                return false;
            // Numbers 26-49 must be placed on outer ring only
            if (currentNumber >= 26 && currentNumber <= 49)
            {
                if (!IsOuterRingCell(row, col))
                    return false;
            }
            if (!IsAdjacent(row, col))
                return false;
            if (IsDiagonal(row, col))
                score++;
            _board7[row, col] = currentNumber;
            lastRow = row;
            lastCol = col;
            currentNumber++;
            return true;
        }

        /// <summary>Expand to Level 2: keep inner 5x5 (1-25), add outer ring, next number 26.</summary>
        public bool ExpandToLevel2()
        {
            if (_level != 1 || currentNumber != 26)
                return false;
            _board7 = new int[Level2Size, Level2Size];
            for (int r = 0; r < Level1Size; r++)
            {
                for (int c = 0; c < Level1Size; c++)
                {
                    _board7[r + 1, c + 1] = _board5[r, c];
                }
            }
            _level = 2;
            currentNumber = 26;
            lastRow = lastRow + 1;
            lastCol = lastCol + 1;
            return true;
        }

        /// <summary>Clear board. Level 1: clear all; optionally keep or re-randomize 1. Level 2: clear only outer ring.</summary>
        public void ClearBoard(bool keepOneInPlace, Random? rng = null)
        {
            if (_level == 1)
            {
                int savedOneRow = -1, savedOneCol = -1;
                if (keepOneInPlace)
                {
                    for (int r = 0; r < Level1Size; r++)
                    {
                        for (int c = 0; c < Level1Size; c++)
                        {
                            if (_board5[r, c] == 1) { savedOneRow = r; savedOneCol = c; break; }
                        }
                        if (savedOneRow >= 0) break;
                    }
                }
                for (int r = 0; r < Level1Size; r++)
                    for (int c = 0; c < Level1Size; c++)
                        _board5[r, c] = 0;
                currentNumber = 1;
                score = 0;
                lastRow = -1;
                lastCol = -1;
                if (savedOneRow >= 0 && savedOneCol >= 0)
                {
                    _board5[savedOneRow, savedOneCol] = 1;
                    lastRow = savedOneRow;
                    lastCol = savedOneCol;
                    currentNumber = 2;
                }
                else if (rng != null)
                {
                    int r1 = rng.Next(0, Level1Size);
                    int c1 = rng.Next(0, Level1Size);
                    _board5[r1, c1] = 1;
                    lastRow = r1;
                    lastCol = c1;
                    currentNumber = 2;
                }
            }
            else
            {
                for (int i = 0; i < Level2Size; i++)
                {
                    _board7![0, i] = 0;
                    _board7![6, i] = 0;
                    _board7![i, 0] = 0;
                    _board7![i, 6] = 0;
                }
                currentNumber = 26;
                for (int r = 1; r <= 5; r++)
                    for (int c = 1; c <= 5; c++)
                        if (_board7[r, c] == 25)
                        {
                            lastRow = r;
                            lastCol = c;
                            return;
                        }
            }
        }
    }
}
