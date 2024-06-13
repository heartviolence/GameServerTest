using Server.EFcore.Models;

namespace Server.GameSystems.Player.Characters
{
    public class Character
    {
        public CharacterData Data { get; set; }

        public Character(CharacterData data)
        {
            Data = data;
        }       

    }
}
