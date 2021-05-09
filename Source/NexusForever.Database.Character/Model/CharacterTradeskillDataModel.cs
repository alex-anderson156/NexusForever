namespace NexusForever.Database.Character.Model
{
    public class CharacterTradeskillDataModel
    {
        public ulong CharacterId { get; set; }
        public uint TradeskillId { get; set; }
        public bool IsActive { get; set; }
        public uint CurrentXp { get; set; }
        public uint TalentPoints { get; set; }        
    }
}
