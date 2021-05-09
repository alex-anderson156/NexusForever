using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Tradeskills.Static;
using NexusForever.WorldServer.Network.Message.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.WorldServer.Game.Tradeskills
{
    public class Tradeskill: IBuildable<Server0860>
    {
        public enum SaveMask
        {
            None            = 0,
            Create          = 1,
            IsActive        = 2,
            XP              = 4,
            TalentPoints    = 8,
            TalentBonuses   = 16,
        }

        private SaveMask saveMask;

        public TradeskillEntry Entry { get; }
         
        public uint TradeskillId => Entry.Id;         
        public TradeskillFlags Flags => (TradeskillFlags)Entry.Flags;

        public bool IsAutoLearn => Flags.HasFlag(TradeskillFlags.IsAutoLearn);
        public bool IsHobby => Flags.HasFlag(TradeskillFlags.IsHobby);
        public bool IsHarvesting => Flags.HasFlag(TradeskillFlags.IsHarvesting);
        public bool IsCircuitBoardCrafting => Flags.HasFlag(TradeskillFlags.IsCircuitBoardCrafting);
        public bool IsCoordinateCrafting => Flags.HasFlag(TradeskillFlags.IsCoordinateCrafting);

        private bool isActive;
        public bool IsActive {
            get => isActive;
            set {
                isActive = value;
                saveMask |= SaveMask.IsActive;
            }
        }

        public uint xp;
        public uint XP
        {
            get => xp;
            set
            {
                xp = value;
                saveMask |= SaveMask.XP;
            }
        }

        public uint talentPoints;
        public uint TalentPoints
        {
            get => talentPoints;
            set
            {
                talentPoints = value;
                saveMask |= SaveMask.TalentPoints;
            }
        }

        private uint currentTier;
        public uint CurrentTier => currentTier;

        public uint[] TalentBonuses { get; private set; }
         
        public Tradeskill(TradeskillEntry entry)
        {
            Entry = entry;
            isActive = false;
            xp = 0;
            talentPoints = 0;
            currentTier = GetTierFromCurrentXp();
            TalentBonuses = new uint[10] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            saveMask = SaveMask.Create;
        }

        public Tradeskill(TradeskillEntry entry, CharacterTradeskillDataModel model)
        {
            Entry = entry;
            isActive = model.IsActive;
            xp = model.CurrentXp;
            currentTier = GetTierFromCurrentXp();
            talentPoints = model.TradeskillId;
            TalentBonuses = new uint[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            saveMask = SaveMask.None;
        }

        public void Save(CharacterContext context, ulong ownerCharacterId)
        {
            if (saveMask == 0)
                return;

            if (saveMask.HasFlag(SaveMask.Create))
            {
                context.Add(new CharacterTradeskillDataModel
                {
                    CharacterId = ownerCharacterId,
                    TradeskillId = TradeskillId,
                    IsActive = IsActive,
                    CurrentXp = XP,
                    TalentPoints = TalentPoints
                });

                saveMask = SaveMask.None;
                return;
            }

            CharacterTradeskillDataModel model = new CharacterTradeskillDataModel
            {
                CharacterId = ownerCharacterId,
                TradeskillId = TradeskillId,
            };

            EntityEntry<CharacterTradeskillDataModel>  entity = context.Attach(model);
            if (saveMask.HasFlag(SaveMask.IsActive))
            {
                model.IsActive = IsActive;
                entity.Property(p => p.IsActive).IsModified = true;
            }

            if (saveMask.HasFlag(SaveMask.XP))
            {
                model.CurrentXp = XP;
                entity.Property(p => p.CurrentXp).IsModified = true;
            }

            if (saveMask.HasFlag(SaveMask.TalentPoints))
            {
                model.TalentPoints = TalentPoints;
                entity.Property(p => p.TalentPoints).IsModified = true;
            }

            saveMask = SaveMask.None;
        }

        #region Talents

        public ulong GetTalentResetCost()
        {
            ulong cost = 0;
            for (uint index = 0; index < 10; index++)
                cost += TalentBonuses[index] > 0 ? GetTradeskillTalentTierEntry(index).RespecCost : 0;

            return cost;
        }

        public bool PickTalentTier(uint tier, uint bonusId)
        {
            TradeskillTalentTierEntry tierEntry = GetTradeskillTalentTierEntry(tier);
            TradeskillBonusEntry bonusEntry = GetTradeskillBonusEntry(bonusId);

            if (TalentPoints < tierEntry.PointsToUnlock)
                return false;

            TalentBonuses[tier] = bonusId;
            saveMask |= SaveMask.TalentBonuses;
            return true;
        }

        public void ResetTalents()
        {
            TalentBonuses = new uint[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            saveMask |= SaveMask.TalentBonuses;
        }

        #endregion

        private uint GetTierFromCurrentXp()
        {
            uint currentTier = 0;
            foreach (TradeskillTierEntry tierEnty in GetTradeskillTierEntries())
            {
                if (xp >= tierEnty.RequiredXp)
                    currentTier = tierEnty.Tier;
                else
                    break;
            }

            return currentTier;
        }

        #region Game Tables

        private ImmutableArray<TradeskillTierEntry> GetTradeskillTierEntries()
        {
            return GameTableManager.Instance.TradeskillTier.Entries.Where(e => e.TradeSkillId == TradeskillId).OrderBy(t => t.Tier).ToImmutableArray();
        }

        private TradeskillTierEntry GetTradeskillTierEntry(uint tier)
        {
            return GameTableManager.Instance.TradeskillTier.Entries.Single(e => e.TradeSkillId == TradeskillId && e.Tier == tier);
        }

        private ImmutableArray<TradeskillTalentTierEntry> GetTradeskillTalentTierEntries()
        {
            return GameTableManager.Instance.TradeskillTalentTier.Entries.Where(e => e.TradeSkillId == TradeskillId).OrderBy(e => e.RespecCost).ToArray().ToImmutableArray();
        }

        private TradeskillTalentTierEntry GetTradeskillTalentTierEntry(uint tier)
        {
            var entiries = GetTradeskillTalentTierEntries();
            if (tier < 0 || tier >= entiries.Length)
                throw new IndexOutOfRangeException();

            return entiries[(int)tier];
        }

        private TradeskillBonusEntry GetTradeskillBonusEntry(uint bonusId)
        {
            return GameTableManager.Instance.TradeskillBonus.Entries.Single(e => e.Id == bonusId);
        }

        #endregion

        public Server0860 Build()
        {
            return new Server0860
            {
                tradeSkillId = TradeskillId,
                XP = XP,
                IsActive = IsActive ? 1u : 0u,
                ActiveTradeskillBonusIDs = TalentBonuses,
                TalentPoints = TalentPoints,
                Unk0 = 0,               
            };
        }

        
    }
}
