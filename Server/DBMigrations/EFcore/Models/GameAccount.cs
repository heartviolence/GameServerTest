using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Server.EFcore.Models
{
    public class GameAccount
    {
        public Guid Id { get; set; }
        public int UID { get; set; }
        public int AccountLevel { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public required string LoginId { get; set; } = string.Empty;
        public required string LoginPassword { get; set; } = string.Empty;
        public List<Item> Inventory { get; set; } = new List<Item>();
        public required GameWorld GameWorld { get; set; }
        public List<CharacterData> CharacterDatas { get; set; } = new List<CharacterData>();

        protected GameAccount()
        {
            GameWorld = new GameWorld()
            { Owner = this };
        }

        [SetsRequiredMembers]
        public GameAccount(string loginId, string loginPassword)
        {
            this.LoginId = loginId;
            this.LoginPassword = loginPassword;
            this.GameWorld = new GameWorld() { Owner = this };
        }
    }
}
