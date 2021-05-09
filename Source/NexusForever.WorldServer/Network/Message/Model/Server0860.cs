using System;
using System.Collections.Generic;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.Server0860)]
    public class Server0860 : IWritable
    {
        public uint tradeSkillId { get; set; }

        public uint XP { get; set; }

        public uint IsActive { get; set; }

        public uint Unk0 { get; set; }

        public uint TalentPoints { get; set; }

        public uint[] ActiveTradeskillBonusIDs { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(tradeSkillId);
            writer.Write(XP);
            writer.Write(IsActive);
            writer.Write(Unk0);
            writer.Write(TalentPoints);

            // We write 40 bytes i.e. 10 integers
            for(int i = 0; i < 10; i++)
                writer.Write(i < ActiveTradeskillBonusIDs.Length ? ActiveTradeskillBonusIDs[i] : 0);
        }
    }
}
