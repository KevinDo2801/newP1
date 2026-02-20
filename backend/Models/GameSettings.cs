namespace backendAPI.Models
{
    public class GameSettings
    {
        public TimeLimitsConfig TimeLimits { get; set; } = new();
    }

    public class TimeLimitsConfig
    {
        public bool Enabled { get; set; } = false;
        public int Level1 { get; set; } = 0;
        public int Level2 { get; set; } = 0;
        public int Level3 { get; set; } = 0;

        public int? GetTimeLimitForLevel(int level)
        {
            if (!Enabled) return null;
            return level switch
            {
                1 => Level1 > 0 ? Level1 : null,
                2 => Level2 > 0 ? Level2 : null,
                3 => Level3 > 0 ? Level3 : null,
                _ => null
            };
        }
    }
}
