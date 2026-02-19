namespace backendAPI.Models
{
    public class GameLogDto
    {
        public string PlayerUsername { get; set; } = string.Empty;
        public string CompletedAt { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int Level { get; set; }
        public int? Score { get; set; }
        public int[][] Board { get; set; } = Array.Empty<int[]>();
    }

    public class ProgressDto
    {
        public bool Level1Completed { get; set; }
        public bool Level2Completed { get; set; }
        public bool Level3Completed { get; set; }
        public int TotalScore { get; set; }
    }
}
