using System;
using System.Collections.Generic;
using System.Linq;

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
            // Level 2: win when all numbers 2-25 are placed (24 numbers)
            return currentNumber > 25;
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
            // Basic bounds check
            if (row < 0 || row >= Level2Size || col < 0 || col >= Level2Size)
                return false;
            
            // Cell must be empty
            if (_board7![row, col] != 0)
                return false;
            
            // Numbers 2-25 must be placed on outer ring only
            if (currentNumber >= 2 && currentNumber <= 25)
            {
                // Must be on outer ring
                if (!IsOuterRingCell(row, col))
                    return false;
                
                // Get valid positions based on corresponding inner number
                var validPositions = GetValidOuterRingPositions(currentNumber);
                
                // Check if (row, col) is in valid positions
                bool isValid = validPositions.Any(p => p.row == row && p.col == col);
                
                if (!isValid)
                    return false;
            }
            
            // Place the number (no score for Level 2 - score remains unchanged)
            _board7[row, col] = currentNumber;
            lastRow = row;
            lastCol = col;
            currentNumber++;
            return true;
        }

        /// <summary>Expand to Level 2: keep inner 5x5 (1-25), add outer ring, next number 2.</summary>
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
            currentNumber = 2; // Start placing numbers 2-25 in outer ring
            lastRow = lastRow + 1;
            lastCol = lastCol + 1;
            return true;
        }

        /// <summary>Initialize a new Level 2 game with inner 5x5 pre-filled with numbers 1-25 in a valid pattern.</summary>
        public void InitializeLevel2Game(Random rng)
        {
            _level = 2;
            _board7 = new int[Level2Size, Level2Size];
            
            // Fill inner 5x5 with numbers 1-25 using a snake pattern
            // This ensures all numbers are adjacent (row by row, alternating direction)
            int num = 1;
            
            // Use a simple pattern: fill row by row, alternating direction
            for (int row = 0; row < Level1Size; row++)
            {
                if (row % 2 == 0)
                {
                    // Left to right
                    for (int col = 0; col < Level1Size; col++)
                    {
                        _board7[row + 1, col + 1] = num++;
                    }
                }
                else
                {
                    // Right to left
                    for (int col = Level1Size - 1; col >= 0; col--)
                    {
                        _board7[row + 1, col + 1] = num++;
                    }
                }
            }
            
            // Find the last number (25) position
            lastRow = -1;
            lastCol = -1;
            for (int row = 1; row <= 5; row++)
            {
                for (int col = 1; col <= 5; col++)
                {
                    if (_board7[row, col] == 25)
                    {
                        lastRow = row;
                        lastCol = col;
                        break;
                    }
                }
                if (lastRow >= 0) break;
            }
            
            currentNumber = 2; // Start placing numbers 2-25 in outer ring
            score = 0; // Start with score 0 for Level 2
        }

        /// <summary>Level 2: Find the position of a number (2-25) in the inner 5x5 board. Returns (-1, -1) if not found.</summary>
        private (int row, int col) FindNumberPositionInInner(int number)
        {
            if (_board7 == null || number < 2 || number > 25)
                return (-1, -1);
            
            // Search in inner 5x5 (positions [1,1] to [5,5] in 7x7 board)
            for (int r = 1; r <= 5; r++)
            {
                for (int c = 1; c <= 5; c++)
                {
                    if (_board7[r, c] == number)
                    {
                        // Convert to inner board coordinates (0-4)
                        return (r - 1, c - 1);
                    }
                }
            }
            return (-1, -1);
        }

        /// <summary>Level 2: Check if inner board position (0-4, 0-4) is on the main diagonal.</summary>
        private static bool IsOnMainDiagonal(int innerRow, int innerCol)
        {
            return innerRow == innerCol;
        }

        /// <summary>Level 2: Check if inner board position (0-4, 0-4) is on the anti-diagonal.</summary>
        private static bool IsOnAntiDiagonal(int innerRow, int innerCol)
        {
            return innerRow + innerCol == 4;
        }

        /// <summary>Level 2: Get valid outer ring positions for placing a number (2-25). Returns list of (row, col) positions.</summary>
        private List<(int row, int col)> GetValidOuterRingPositions(int numberToPlace)
        {
            var validPositions = new List<(int row, int col)>();
            
            // Number to place (2-25) corresponds to the same number in inner board
            int innerNumber = numberToPlace;
            if (innerNumber < 2 || innerNumber > 25)
                return validPositions;
            
            // Find where this number exists in inner 5x5
            var (innerRow, innerCol) = FindNumberPositionInInner(innerNumber);
            if (innerRow < 0 || innerCol < 0)
                return validPositions;
            
            // Convert to 7x7 coordinates (inner board is at [1,1] to [5,5])
            int outerRow = innerRow + 1;  // 1-5
            int outerCol = innerCol + 1; // 1-5
            
            // Valid positions: ends of row and column
            validPositions.Add((outerRow, 0)); // Left end of row
            validPositions.Add((outerRow, 6)); // Right end of row
            validPositions.Add((0, outerCol)); // Top end of column
            validPositions.Add((6, outerCol)); // Bottom end of column
            
            // Diagonal bonus: if on main diagonal (0,0)->(4,4) or anti-diagonal (0,4)->(4,0)
            if (IsOnMainDiagonal(innerRow, innerCol))
            {
                validPositions.Add((0, 0)); // Top-left corner
                validPositions.Add((6, 6)); // Bottom-right corner
            }
            
            if (IsOnAntiDiagonal(innerRow, innerCol))
            {
                validPositions.Add((0, 6)); // Top-right corner
                validPositions.Add((6, 0)); // Bottom-left corner
            }
            
            // Remove duplicates and filter to only outer ring cells
            return validPositions
                .Where(p => IsOuterRingCell(p.row, p.col))
                .Distinct()
                .ToList();
        }

        /// <summary>Level 2: Check if there's a dead end (no valid placement for current number).</summary>
        public bool IsDeadEnd()
        {
            if (_level != 2 || _board7 == null)
                return false;
            
            // Get valid positions for current number
            var validPositions = GetValidOuterRingPositions(currentNumber);
            
            // Check if any valid position is empty
            foreach (var (row, col) in validPositions)
            {
                if (_board7[row, col] == 0)
                    return false; // At least one valid position is empty
            }
            
            // All valid positions are filled - dead end
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
                currentNumber = 2; // Reset to start placing numbers 2-25
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
