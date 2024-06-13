namespace Server.EFcore.Models
{
    public class CompletedAchievement
    {
        public Guid Id { get; set; }
        public required string AchievementCode { get; set; }
        public GameWorld GameWorld { get; set; }
        public Guid GameWorldId { get; set; }
    }
}
