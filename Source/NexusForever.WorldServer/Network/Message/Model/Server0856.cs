using System.Collections.Generic;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Server0856)]
    public class Server0856 : IWritable
    { 
        public struct Unknown1 : IWritable
        {
            public uint Unk0 { get; set; }
            public uint Unk1 { get; set; }
            public uint Unk2 { get; set; }

            public void Write(GamePacketWriter writer)
            {
                writer.Write(Unk0);
                writer.Write(Unk1);
                writer.Write(Unk2);
            }
        }

        public List<Server0860> Tradeskills { get; set; } = new List<Server0860>();

        public List<uint> KnownSchematicIDs { get; set; } = new List<uint>();

        public List<Unknown1> Unknown1s { get; set; } = new List<Unknown1>();

        public List<uint> UnknownNumbers { get; set; } = new List<uint>();

        public uint SwapCooldownInSeconds { get; set; } = 0; // something cooldown related 

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Tradeskills.Count);
            Tradeskills.ForEach(t => t.Write(writer));

            writer.Write(KnownSchematicIDs.Count);
            KnownSchematicIDs.ForEach(t => writer.Write(t));

            writer.Write(Unknown1s.Count);
            Unknown1s.ForEach(t => t.Write(writer));

            writer.Write(UnknownNumbers.Count);
            UnknownNumbers.ForEach(t => writer.Write(t));

            writer.Write(SwapCooldownInSeconds);
        }
    }
}
