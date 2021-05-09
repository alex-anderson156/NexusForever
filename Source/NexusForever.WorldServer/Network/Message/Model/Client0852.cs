using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Client0852)]
    public class Client0852 : IReadable
    {
        public uint Unk0 { get; set; }
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }
        public uint Unk3 { get; set; } 
         
        public void Read(GamePacketReader reader)
        {
            Unk0 = reader.ReadUInt();
            Unk1 = reader.ReadUInt();
            Unk2 = reader.ReadUInt();
            Unk3 = reader.ReadUInt(); 
        } 
    }
}
