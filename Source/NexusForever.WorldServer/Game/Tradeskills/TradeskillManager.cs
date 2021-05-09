using System;
using System.Linq;
using System.Collections.Generic;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Tradeskills.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.Shared;
using NexusForever.WorldServer.Game.CharacterCache;
using System.Collections.Immutable;

namespace NexusForever.WorldServer.Game.Tradeskills
{
    public class TradeskillManager : ISaveCharacter, IUpdate
    {
        public static readonly uint TradeskillLockoutInDays = 3;


        private enum SaveMask
        {
            None                = 0,
            Tradeskills         = 1
        }

        private ulong ownerCharacterId; 
        private SaveMask saveMask = 0;
         
        private Dictionary<TradeskillEnum, Tradeskill> knownTradeskills;
        private DateTime? tradeskillUnlockTime;


        private uint knownTradeskillCount => (uint)knownTradeskills.Values.Count(t => t.IsActive);
        private uint knownProfessionCount => (uint)knownTradeskills.Values.Count(t => t.IsActive && !t.IsHobby);
        private uint knownHobbyCount => (uint)knownTradeskills.Values.Count(t => t.IsActive && t.IsHobby);

        private CraftingSessionBase pendingCraftingSession;
        public CraftingSessionBase PendingCraftingSession => pendingCraftingSession;


        /// <summary>
        /// Creates a new instance of the tradeskill manger for a specified player.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> to create the manager for.</param>
        /// <param name="model">The characters Stored information</param>
        public TradeskillManager(Player player, CharacterModel model)
        {
            ownerCharacterId = player.CharacterId;
            knownTradeskills = new Dictionary<TradeskillEnum, Tradeskill>();

            //TODO: Load the Dictionary up from the model                        
            //foreach(CharacterTradeskillDataModel tskill in model.Tradeskills)
            //{
            //    TradeskillEntry entry = GetTradeskillTableEntry(tskill.Id);
            //    knownTradeskills.Add((TradeskillEnum)tskill.Id, new Tradeskill(entry, model));
            //}

            // we always know runecrafting.
            knownTradeskills.Add(TradeskillEnum.Runecrafting, new Tradeskill(GetTradeskillEntry((uint)TradeskillEnum.Runecrafting)));
            saveMask = SaveMask.None;
        }


        public void Update(double lastTick)
        {
            if (tradeskillUnlockTime.HasValue && tradeskillUnlockTime <= DateTime.Now)
                UpdateCooldown(null);
        }

        public void Save(CharacterContext context)
        {
            if (saveMask == SaveMask.None)
                return;
             
            if (saveMask.HasFlag(SaveMask.Tradeskills))
            {
                foreach(Tradeskill tradeskill in knownTradeskills.Values)
                    tradeskill.Save(context, ownerCharacterId);
            }
        }

        public void SendInitialPackets()
        {
            if (!GetPlayer(out Player player))
                return;

            player.Session.EnqueueMessageEncrypted(new Server0856
            {
                Tradeskills = knownTradeskills.Values.Select(t => t.Build()).ToList(),

                KnownSchematicIDs = new List<uint>()
                {
                    // I think these are the known non-auto-learnt schematic Ids
                },

                Unknown1s = new List<Server0856.Unknown1>() {
                    // I havent the foggiest
                },

                UnknownNumbers = new List<uint>() {
                    // I havent the foggiest
                },

                SwapCooldownInSeconds = tradeskillUnlockTime.HasValue ? (uint)(tradeskillUnlockTime.Value - DateTime.Now).TotalSeconds : 0
            });
        }

        public bool IsTradeskillKnown(uint tradeskillId)
        {
            return GetTradeskill((TradeskillEnum)tradeskillId, out _);
        }

        public bool IsSchematicKnown(uint schematicId)
        {
            TradeskillSchematic2Entry schemEntry = GameTableManager.Instance.TradeskillSchematic2.Entries.SingleOrDefault(e => e.Id == schematicId);
            if (!GetTradeskill((TradeskillEnum)schemEntry.TradeSkillId, out Tradeskill tradeskill))
                return false;

            if (schemEntry.Tier > tradeskill.CurrentTier)
                return false;

            if ((schemEntry.Flags & 1) == 0)
            {
                // This schematic is not auto-learnt so we need to assert that the player has learnt this schematic.
                // TODO;
                return false; 
            }

            return true;
        }

        public bool Learn(uint toLearn, uint toForget)
        {
            TradeskillEntry tradeskillToLearnDefinition = GetTradeskillEntry(toLearn);
            if (tradeskillToLearnDefinition == null)
                return false;

            GetTradeskill((TradeskillEnum)tradeskillToLearnDefinition.Id, out Tradeskill tradeskill);
            bool notPreviouslyKnown = tradeskill == null;
            if (notPreviouslyKnown)
                tradeskill = new Tradeskill(tradeskillToLearnDefinition);
            else if (tradeskill.IsActive)
                return true; //we already know it - so this is pointless.

            Tradeskill tradeskillToForget = null;
            if (toForget > 0 && !tradeskill.IsHobby)
            {                
                if (!GetTradeskill((TradeskillEnum)toForget, out tradeskillToForget))
                    return false;
            }

            if (CanILearn(tradeskill, tradeskillToForget, out DateTime? unlockTime))
            {
                if (!GetPlayer(out Player player))
                    return false;

                    // Deactive the toForget if Set.
                if (tradeskillToForget != null)
                {
                    tradeskillToForget.IsActive = false; 
                    player.Session.EnqueueMessageEncrypted(tradeskillToForget.Build());
                }

                // Learn it.
                tradeskill.IsActive = true;
                if (notPreviouslyKnown) 
                    knownTradeskills.Add((TradeskillEnum)tradeskill.TradeskillId, tradeskill);

                saveMask |= SaveMask.Tradeskills;
                player.Session.EnqueueMessageEncrypted(tradeskill.Build());                 

                if (unlockTime.HasValue)
                    UpdateCooldown(unlockTime.Value);

                return true;
            }
             
            return false;
        }

        public bool PickTalentTier(uint tradeskillId, uint tier, uint bonusId)
        {
            if (!GetTradeskill((TradeskillEnum)tradeskillId, out Tradeskill tradeSkill))
                return false;

            if (!GetPlayer(out Player player))
                return false;

            if (tradeSkill.PickTalentTier(tier, bonusId))
            {
                player.Session.EnqueueMessageEncrypted(tradeSkill.Build());
                return true;
            }

            return false;
        }

        public bool ResetTalents(uint tradeskillId)
        {
            if (!GetTradeskill((TradeskillEnum)tradeskillId, out Tradeskill tradeSkill))
                return false;

            if (!GetPlayer(out Player player))
                return false;

            ulong resetCost = tradeSkill.GetTalentResetCost();
            if (!player.CurrencyManager.CanAfford(Entity.Static.CurrencyType.CraftingVoucher, resetCost))
                return false;

            player.CurrencyManager.CurrencySubtractAmount(Entity.Static.CurrencyType.CraftingVoucher, resetCost);
            tradeSkill.ResetTalents();
            player.Session.EnqueueMessageEncrypted(tradeSkill.Build());
            return true;
        }

        public bool StartCraftingSession(CraftingSessionBase craftingSession)
        {
            if (pendingCraftingSession != null)
                return false;

            if (!IsSchematicKnown(craftingSession.SchematicId))
                return false;

            if (!GetPlayer(out Player player))
                return false; // player is offline now.

            ImmutableArray<Tuple<uint, uint>> schematicMaterials = craftingSession.GetMaterialRequirements();
            List<Tuple<Item, uint>> materials = new List<Tuple<Item, uint>>();
             
            //foreach(Tuple<uint, uint> itemCount in schematicMaterials)
            //{
            //    Item item = player.Inventory.GetItem(itemCount.Item1);
            //    if (item == null || item.StackCount < itemCount.Item2)
            //        return false;
            //
            //    materials.Add(new Tuple<Item, uint>(item, itemCount.Item2));
            //}
            //
            //foreach (Tuple<Item, uint> itemCount in materials)
            //    player.Inventory.ItemDelete(itemCount.Item1.Id, itemCount.Item2);

            pendingCraftingSession = craftingSession;
            craftingSession.UpdateCraft();
            return true;
        }
       
        
        
        private bool GetPlayer(out Player player)
        {
            player = CharacterManager.Instance.GetPlayer(ownerCharacterId);
            return player != null;
        }

        private bool GetTradeskill(TradeskillEnum tradeskill, out Tradeskill knownTrade)
        {
           return knownTradeskills.TryGetValue(tradeskill, out knownTrade);
        }

        private TradeskillEntry GetTradeskillEntry(uint id)
        {
            TradeskillEntry entry = GameTableManager.Instance.Tradeskill.Entries.First(e => e.Id == id); 
            return entry;
        }

        private bool CanILearn(Tradeskill toLearn, Tradeskill toForget, out DateTime? unlockTime)
        { 
            unlockTime = null; 
            
            // If i am locked i cannot swap.
            if (tradeskillUnlockTime.HasValue && tradeskillUnlockTime.Value > DateTime.Now)
                return false;

            if (toLearn.IsHobby && !toLearn.IsActive)
                return true; // There is only 1 hobby in the game currently, fishing / farming was never implemented, as such we can always learn cooking.

            if (knownProfessionCount < 2 && toForget == null) // this is just a sanity check If i know less than 2 professions i shouldnt be trying to forget one.
                return true; // If i know less than 2 - I can learn with no penalty.

            if (!toForget.IsActive || toLearn.IsActive) // If the one i want to forget is inactive - likewise if the one i want to learn I laready know - I cant swap it out.
                return false;

            // Assume we learn a new one, add 3 days to the lockout.
            //unlockTime = DateTime.Now.AddDays(TradeskillLockoutInDays);
            unlockTime = DateTime.Now.AddMinutes(TradeskillLockoutInDays);
            return true;
        }

        private void UpdateCooldown(DateTime? unlockTime)
        {
            tradeskillUnlockTime = unlockTime;

            uint seconds = 0;
            if (tradeskillUnlockTime.HasValue && tradeskillUnlockTime >= DateTime.Now)
                seconds = (uint)(tradeskillUnlockTime.Value - DateTime.Now).TotalSeconds;

            if (GetPlayer(out Player player))
                player.Session.EnqueueMessageEncrypted(new Server0861{ CooldownTimeInSeconds = seconds});
        } 
    }
}
