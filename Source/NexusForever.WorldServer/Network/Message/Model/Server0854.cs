using System.Collections.Generic;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Server0854)]
    public class Server0854 : IWritable
    {
        public uint InProgressSchematicId { get; set; } 
        public ulong Unk8 { get; set; } 
        public uint Unk10 { get; set; }
        public uint SchematicOutputCount { get; set; }
        public uint AdditiveCount { get; set; }
        public uint Unk1C { get; set; }
        public uint[] UnknownNumbers { get; set; } = new uint[5] { 0, 0, 0, 0, 0 };
        public uint GlobalCatalystItemId { get; set; }
        public float VectorX { get; set; }
        public float VectorY { get; set; }
        public uint Unk40 { get; set; }
        public uint Unk44 { get; set; }
        public uint Unk48 { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(InProgressSchematicId, 15);
            writer.Write(Unk8);
            writer.Write(Unk10);
            writer.Write(SchematicOutputCount);
            writer.Write(AdditiveCount);
            writer.Write(Unk1C);

            for (int i = 0; i < 5; i++)
                writer.Write(i <= UnknownNumbers.Length ? UnknownNumbers[i] : 0u);

            writer.Write(GlobalCatalystItemId, 18);
            writer.Write(VectorX);
            writer.Write(VectorY);
            writer.Write(Unk40);
            writer.Write(Unk44);
            writer.Write(Unk48);
        }
    }
}
