using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Storefront;
using NexusForever.WorldServer.Game.Tradeskills;
using NexusForever.WorldServer.Network.Message.Model;
using System.Linq;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class TradeskillHandler
    {
        private static void AssertKnownTradeskill(WorldSession session, uint tradeskillId)
        {
            if (!session.Player.TradeskillManager.IsTradeskillKnown(tradeskillId))
                throw new InvalidPacketValueException();
        }

        [MessageHandler(GameMessageOpcode.Client0857)]
        public static void HandleClient0857(WorldSession session, Client0857 client0857)
        {
            if (!session.Player.TradeskillManager.Learn(client0857.TradeskillId, client0857.toForgetId))
                throw new InvalidPacketValueException();
        }

        [MessageHandler(GameMessageOpcode.Client084E)]
        public static void HandleClient084E(WorldSession session, Client084E client084E)
        {
            AssertKnownTradeskill(session, client084E.TradeskillId);
            if (!session.Player.TradeskillManager.PickTalentTier(client084E.TradeskillId, client084E.Tier, client084E.BonusId))
                throw new InvalidPacketValueException();
        }

        [MessageHandler(GameMessageOpcode.Client0858)]
        public static void HandleClient0858(WorldSession session, Client0858 client0858)
        {
            AssertKnownTradeskill(session, client0858.TradeskillId);
            if (!session.Player.TradeskillManager.ResetTalents(client0858.TradeskillId))
                throw new InvalidPacketValueException();
        }

        // -- 
        // Crafting Session

        [MessageHandler(GameMessageOpcode.Client0851)]
        public static void HandleClient0851(WorldSession session, Client0851 client0851)
        {              
            if (session.Player.TradeskillManager.PendingCraftingSession != null)
                throw new InvalidPacketValueException();

            CoordianteCraftingSession craftingSession = new CoordianteCraftingSession(client0851.Unk2, session.Player.CharacterId);
            if (!session.Player.TradeskillManager.StartCraftingSession(craftingSession))
                throw new InvalidPacketValueException();
        }

        [MessageHandler(GameMessageOpcode.Client0852)]
        public static void HandleClient0852(WorldSession session, Client0852 client0852)
        {
            if (session.Player.TradeskillManager.PendingCraftingSession != null)
                throw new InvalidPacketValueException();

            CircuitBoardCraftingSession craftingSession = new CircuitBoardCraftingSession(client0852.Unk2, session.Player.CharacterId);
            if (!session.Player.TradeskillManager.StartCraftingSession(craftingSession))
                throw new InvalidPacketValueException();
        }


        [MessageHandler(GameMessageOpcode.Client084A)]
        public static void HandleClient084A(WorldSession session, Client084A client084A)
        {
            CraftingSessionBase pendingCraftingSession = session.Player.TradeskillManager.PendingCraftingSession;
            if (pendingCraftingSession  is null)
                throw new InvalidPacketValueException();

            if (pendingCraftingSession is CoordianteCraftingSession craftingSession) { 
                if (!craftingSession.AddAdditive(client084A.Unk1))
                    throw new InvalidPacketValueException();
            }
        }


        // -- 
        // Complete Crafting Session

        [MessageHandler(GameMessageOpcode.Client084F)]
        public static void HandleClient084F(WorldSession session, Client084F client084F)
        {

        }

        [MessageHandler(GameMessageOpcode.Client0850)] // client finsh coordinate crafting
        public static void HandleClient0850(WorldSession session, Client0850 client0850)
        {
            //   2 <-- Possible subschematic index ?
            // 328 <-- this number again!
            // 374 <-- looks like schematicId 
        }
    }
}