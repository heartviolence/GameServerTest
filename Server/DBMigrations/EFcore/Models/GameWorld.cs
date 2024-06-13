
namespace Server.EFcore.Models
{
    public class GameWorld
    {
        public Guid Id { get; set; }
        public required GameAccount Owner { get; set; }
        public List<CompletedAchievement> Achievements { get; set; } = new List<CompletedAchievement>();
    }
}
