using System;

namespace backendAPI
{
    public class ConsoleUI
    {
        private GameLogic game;
        private SaveLoadManager saveLoad;

        // Initializes the UI and creates a new game instance
        public ConsoleUI()
        {
            game = new GameLogic();
            saveLoad = new SaveLoadManager();
        }

        // Starts the game loop: displays board, asks for first move, then continuously asks for next moves
        public void Start()
        {
            Console.WriteLine("1. New Game");
            Console.WriteLine("2. Load Game");
            Console.Write("Choose: ");

            string choice = Console.ReadLine();

            if (choice == "2")
            {
                LoadGame();
            }
            else
            {
                game.InitializeNewGame();
                DisplayBoard();
                AskForFirstMove();
            }

            while (true)
            {
                DisplayBoard();

                if (game.HasWon())
                {
                    Console.WriteLine("YOU WIN!");
                    Console.WriteLine($"Final Score: {game.GetScore()}");
                    Console.WriteLine();
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                AskForNextMove();
            }
        }

        // Displays the current game board and game state information
        private void DisplayBoard()
        {
            int[,] board = game.GetBoard();

            Console.WriteLine("Current board:");
            Console.WriteLine();

            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    if (board[r, c] == 0)
                        Console.Write(" . ");
                    else
                        Console.Write($"{board[r, c],2} ");
                }
                Console.WriteLine();
            }

            if (!game.HasWon())
            {
                Console.WriteLine();
                Console.WriteLine($"Next number: {game.GetCurrentNumber()}");
                Console.WriteLine($"Score: {game.GetScore()}");
            }
        }

        // Prompts the user to enter the position for the first number (number 1, can be anywhere)
        private void AskForFirstMove()
        {
            Console.WriteLine("Enter position for number 1");
            Console.Write("Row (0-4): ");
            int row = int.Parse(Console.ReadLine());

            Console.Write("Col (0-4): ");
            int col = int.Parse(Console.ReadLine());

            bool success = game.PlaceFirstNumber(row, col);

            if (!success)
            {
                Console.WriteLine("Invalid position. Game over.");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        // Prompts the user to enter the position for the next number (must be adjacent)
        private void AskForNextMove()
        {
            Console.WriteLine($"Enter position for number {game.GetCurrentNumber()}");
            Console.Write("Row (0-4) or S to save: ");
            string input = Console.ReadLine();

            if (input.ToUpper() == "S")
            {
                saveLoad.Save(
                    game.GetBoard(),
                    game.GetCurrentNumber(),
                    game.GetScore(),
                    game.GetLastRow(),
                    game.GetLastCol(),
                    "savegame.txt"
                );

                Console.WriteLine("Game saved to savegame.txt");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            int row = int.Parse(input);

            Console.Write("Col (0-4): ");
            int col = int.Parse(Console.ReadLine());

            bool success = game.PlaceNumber(row, col);

            if (!success)
            {
                Console.WriteLine("Invalid move. Game over.");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        // Loads the game from the save file
        private void LoadGame()
        {
            try
            {
                saveLoad.Load(
                    "savegame.txt",
                    out int[,] board,
                    out int currentNumber,
                    out int score,
                    out int lastRow,
                    out int lastCol
                );

                game.LoadGame(board, currentNumber, score, lastRow, lastCol);

                Console.WriteLine("Game loaded successfully.");
            }
            catch
            {
                Console.WriteLine("Failed to load game. Exiting.");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
    }
}
