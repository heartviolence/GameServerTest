
using MessagePack;
using Shared.Server.GameServer.DTO;

public class CharacterData
{
    public string BaseCharacterCode { get; set; }
    public int Level { get; set; }
    public int EXP { get; set; }

    static public CharacterData From(CharacterDataDTO dto)
    {
        return new CharacterData()
        {
            BaseCharacterCode = dto.BaseCharacterCode,
            Level = dto.Level,
            EXP = dto.EXP
        };
    }
}