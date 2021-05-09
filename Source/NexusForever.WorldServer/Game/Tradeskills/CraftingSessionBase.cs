using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.CharacterCache;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Network.Message.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.WorldServer.Game.Tradeskills
{
    public abstract class CraftingSessionBase
    {
        public TradeskillSchematic2Entry SchematicEntry { get; private set; }

        public uint SchematicId => SchematicEntry.Id;


        protected readonly ulong characterId;

        public CraftingSessionBase(uint schematicId, ulong characterId)
        {
            this.characterId = characterId; 
            SchematicEntry = GameTableManager.Instance.TradeskillSchematic2.Entries.Single(s => s.Id == schematicId);
        }
         
        public ImmutableArray<Tuple<uint, uint>> GetMaterialRequirements()
        {
            if (SchematicEntry is null)
                throw new InvalidOperationException("Cannot get materials before the schematic has been set.");

            List<Tuple<uint, uint>> returnVal = new List<Tuple<uint, uint>>();
            if (SchematicEntry.Item2IdMaterial00 > 0)
                returnVal.Add(new Tuple<uint, uint>(SchematicEntry.Item2IdMaterial00, SchematicEntry.MaterialCost00));

            if (SchematicEntry.Item2IdMaterial01 > 0)
                returnVal.Add(new Tuple<uint, uint>(SchematicEntry.Item2IdMaterial01, SchematicEntry.MaterialCost01));

            if (SchematicEntry.Item2IdMaterial02 > 0)
                returnVal.Add(new Tuple<uint, uint>(SchematicEntry.Item2IdMaterial02, SchematicEntry.MaterialCost02));

            if (SchematicEntry.Item2IdMaterial03 > 0)
                returnVal.Add(new Tuple<uint, uint>(SchematicEntry.Item2IdMaterial03, SchematicEntry.MaterialCost03));

            if (SchematicEntry.Item2IdMaterial04 > 0)
                returnVal.Add(new Tuple<uint, uint>(SchematicEntry.Item2IdMaterial04, SchematicEntry.MaterialCost04));

            return returnVal.ToImmutableArray();
        }


        public abstract void UpdateCraft();

        public abstract bool Complete();

        protected bool GetPlayer(out Player player)
        {
            player = CharacterManager.Instance.GetPlayer(characterId);
            return player != null;
        }
         
    }

    public sealed class CoordianteCraftingSession : CraftingSessionBase
    {
        private float currVectorX = 0f;
        private float currVectorY = 0f;

        private List<TradeskillAdditiveEntry> additives;
        private uint additiveCount => (uint)additives.Count; 


        public CoordianteCraftingSession(uint schematicId, ulong characterId)
            : base(schematicId, characterId)
        {
            additives = new List<TradeskillAdditiveEntry>();
        }


        public bool AddAdditive(uint itemID)
        {
            Item2Entry itemEntry = GameTableManager.Instance.Item.Entries.Single(a => a.Id == itemID);
            TradeskillAdditiveEntry entry = GameTableManager.Instance.TradeskillAdditive.Entries.Single(a => a.Id == itemEntry.TradeskillAdditiveId);
            if (entry is null)
                return false;

            // TODO: Validate That the additive can be used (additive tradeskillId / Tier)
            // TODO: Validate that i can add an addivive without exceeding the max additive count.

            currVectorX += entry.VectorX;
            currVectorY += entry.VectorY; 
            
            additives.Add(entry);
            UpdateCraft();
            return true;
        }

        private ImmutableArray<TradeskillSchematic2Entry> GetSubSchematics()
        {
            if (SchematicEntry is null)
                throw new InvalidOperationException("Cannot get materials before the schematic has been set.");

            return GameTableManager.Instance.TradeskillSchematic2.Entries.Where(s => s.TradeskillSchematic2IdParent == SchematicId).ToImmutableArray();
        }

        private uint GetMaxAddtitiveCount()
        {
            return SchematicEntry.MaxAdditives;
            //TODO: Talent bonuses.
        }

        public override void UpdateCraft()
        {
            if (!GetPlayer(out Player player))
                return;

            player.Session.EnqueueMessageEncrypted(new Server0854
            {
                InProgressSchematicId = SchematicId,
                SchematicOutputCount = SchematicEntry.OutputCount,
                AdditiveCount = additiveCount,
                GlobalCatalystItemId = 0,
                VectorX = currVectorX,
                VectorY = currVectorY,
            });
        }

        public override bool Complete()
        {
            return false;
        }
    }

    public sealed class CircuitBoardCraftingSession : CraftingSessionBase
    {
        public CircuitBoardCraftingSession(uint schematicId, ulong characterId)
            : base(schematicId, characterId)
        {

        }


        public override void UpdateCraft()
        {
            if (!GetPlayer(out Player player))
                return;

            player.Session.EnqueueMessageEncrypted(new Server0854
            {
                InProgressSchematicId = SchematicId,
                SchematicOutputCount = SchematicEntry.OutputCount,
            });
        }

        public override bool Complete()
        {
            return false;
        }
    }
}
