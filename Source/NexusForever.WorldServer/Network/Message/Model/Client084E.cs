using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Client084E)]
    public class Client084E : IReadable
    {
        public uint TradeskillId { get; set; }
        public uint Tier { get; set; }
        public uint BonusId { get; set; }

        public void Read(GamePacketReader reader)
        {
            TradeskillId = reader.ReadUInt();
            Tier = reader.ReadUInt();
            BonusId = reader.ReadUInt();
        }
    }
}
