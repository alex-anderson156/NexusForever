using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Client084F)]
    public class Client084F : IReadable
    {
        /**
         * These 3 come from a struct and are the same as 0x084F and 0x084E ?
         */
        public uint Unk0 { get; set; }
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }

        public ulong Unk3 { get; set; }

        public uint Unk4 { get; set; }

        public uint Unk5 { get; set; }

        public uint Unk6 { get; set; }

        public uint[] Unk7 { get; set; }

        public void Read(GamePacketReader reader)
        {
            Unk0 = reader.ReadUInt();
            Unk1 = reader.ReadUInt();
            Unk2 = reader.ReadUInt();

            Unk3 = reader.ReadULong();

            Unk4 = reader.ReadUInt(18);
            Unk5 = reader.ReadUInt();
            Unk6 = reader.ReadUInt(3);

            Unk7 = new uint[Unk6];
            for (int i = 0; i < Unk6; i++)
                Unk7[i] = reader.ReadUInt();
        }
    }
}
