using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Client0857)]
    public class Client0857 : IReadable
    {
        public uint TradeskillId { get; set; }

        public uint toForgetId { get; set; }

        public void Read(GamePacketReader reader)
        {
            TradeskillId = reader.ReadUInt();
            toForgetId = reader.ReadUInt();
        }
    }
}
