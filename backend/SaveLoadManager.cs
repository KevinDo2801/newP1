using System;
using System.IO;

namespace backendAPI
{
    public class SaveLoadManager
    {
        // Saves the game to the specified file
        public void Save(
            int[,] board,
            int currentNumber,
            int score,
            int lastRow,
            int lastCol,
            string fileName
        )
        {
            // Writes the game to the specified file
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.WriteLine(currentNumber);
                writer.WriteLine(score);
                writer.WriteLine(lastRow);
                writer.WriteLine(lastCol);

                for (int r = 0; r < 5; r++)
                {
                    for (int c = 0; c < 5; c++)
                    {
                        writer.Write(board[r, c] + " ");
                    }
                    writer.WriteLine();
                }
            }
        }

        // Loads the game from the specified file
        public void Load(
            string fileName,
            out int[,] board,
            out int currentNumber,
            out int score,
            out int lastRow,
            out int lastCol
        )
        {
            board = new int[5, 5];
            // Reads the game from the specified file
            using (StreamReader reader = new StreamReader(fileName))
            {
                currentNumber = int.Parse(reader.ReadLine());
                score = int.Parse(reader.ReadLine());
                lastRow = int.Parse(reader.ReadLine());
                lastCol = int.Parse(reader.ReadLine());

                for (int r = 0; r < 5; r++)
                {
                    string[] parts = reader.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    for (int c = 0; c < 5; c++)
                    {
                        board[r, c] = int.Parse(parts[c]);
                    }
                }
            }
        }
    }
}
