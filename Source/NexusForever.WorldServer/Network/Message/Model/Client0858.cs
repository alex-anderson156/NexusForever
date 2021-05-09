using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Client0858)]
    public class Client0858 : IReadable
    {
        public uint TradeskillId { get; set; } 

        public void Read(GamePacketReader reader)
        {
            TradeskillId = reader.ReadUInt(); 
        }
    }
}
