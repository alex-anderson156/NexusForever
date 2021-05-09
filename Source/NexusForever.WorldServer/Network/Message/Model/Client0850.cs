using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Client0850)]
    public class Client0850 : IReadable
    {
        /**
         * These 3 come from a struct and are the same as 0x084F and 0x084E ?
         */
        public uint Unk0 { get; set; }
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }
         

        public void Read(GamePacketReader reader)
        {
            Unk0 = reader.ReadUInt();
            Unk1 = reader.ReadUInt();
            Unk2 = reader.ReadUInt(); 
        }
    }
}
