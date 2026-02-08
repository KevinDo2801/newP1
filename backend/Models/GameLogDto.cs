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
}
