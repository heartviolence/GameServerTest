using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Server.EFcore.Models
{
    public class CharacterData
    {
        public Guid Id { get; set; }
        public required string BaseCharacterCode { get; set; }
        public required GameAccount Owner { get; set; }
        public int Level { get; set; }
        public int EXP { get; set; }
    }
}
