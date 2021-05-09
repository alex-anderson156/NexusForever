using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Server0861)]
    public class Server0861 : IWritable
    {
        public uint CooldownTimeInSeconds { get; set; } 

        public void Write(GamePacketWriter writer)
        {
            writer.Write(CooldownTimeInSeconds);
        }
    }
}
