using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Client084A)]
    public class Client084A : IReadable
    {
        public uint Unk0 { get; set; }
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }

        public void Read(GamePacketReader reader)
        {
            Unk0 = reader.ReadUInt();
            Unk1 = reader.ReadUInt(18);
            Unk2 = reader.ReadUInt(18);
        }
    }
}
