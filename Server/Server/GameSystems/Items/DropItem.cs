namespace Server.GameSystems.Items
{
    public class DropItem
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // DB와 연관없음
        public Guid OwnerId { get; set; } = Guid.Empty; // 소유자만 획득가능 , Guid.Empty면 월드 소유자만 가능
        public required string BaseItemCode { get; set; }
        public int Count { get; set; }
        public required string SourceAchievementCode { get; set; } = string.Empty;
    }
}
