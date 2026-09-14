using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VirtualDimension
{
    /// <summary>
    /// The shared "dimension" inventory. There is a single global storage per save,
    /// every Virtual Dimension Tower uploads to / draws from it. Persisted through
    /// the DSPModSave IModCanSave interface.
    /// </summary>
    public class DimensionStorage
    {
        public static readonly DimensionStorage Instance = new DimensionStorage();

        // itemId -> stored amount
        public readonly Dictionary<int, int> Counts = new Dictionary<int, int>();

        // itemId -> explicit cap. Items that were never configured use the default rule.
        public readonly Dictionary<int, int> Limits = new Dictionary<int, int>();

        public void Reset()
        {
            Counts.Clear();
            Limits.Clear();
        }

        public int GetCount(int itemId)
        {
            return Counts.TryGetValue(itemId, out int v) ? v : 0;
        }

        /// <summary>Default cap: 50 for buildable entities (buildings), 2,000,000 otherwise.</summary>
        public int DefaultLimit(int itemId)
        {
            ItemProto proto = LDB.items.Select(itemId);
            if (proto != null && proto.IsEntity)
                return VDMod.DEFAULT_BUILDING_LIMIT;
            return VDMod.DEFAULT_ITEM_LIMIT;
        }

        /// <summary>Effective cap, honoring explicit 0..10,000,000 user settings.</summary>
        public int GetLimit(int itemId)
        {
            if (Limits.TryGetValue(itemId, out int v))
                return v;
            return DefaultLimit(itemId);
        }

        public void SetLimit(int itemId, int value)
        {
            value = Mathf.Clamp(value, 0, VDMod.HARD_MAX_LIMIT);
            Limits[itemId] = value;
            if (GetCount(itemId) > value)
                Counts[itemId] = value;
            Prune();
        }

        public void ResetLimit(int itemId)
        {
            Limits.Remove(itemId);
            if (GetCount(itemId) > GetLimit(itemId))
                Counts[itemId] = GetLimit(itemId);
            Prune();
        }

        /// <summary>Insert items into the dimension. Returns the amount actually accepted.</summary>
        public int Insert(int itemId, int want)
        {
            if (want <= 0 || itemId <= 0)
                return 0;
            int limit = GetLimit(itemId);
            int space = limit - GetCount(itemId);
            int move = want < space ? want : space;
            if (move <= 0)
                return 0;
            Counts[itemId] = GetCount(itemId) + move;
            return move;
        }

        /// <summary>Pull items out of the dimension. Returns the amount actually taken.</summary>
        public int TakeOut(int itemId, int want)
        {
            if (want <= 0 || itemId <= 0)
                return 0;
            int cur = GetCount(itemId);
            int move = want < cur ? want : cur;
            if (move <= 0)
                return 0;
            int left = cur - move;
            if (left <= 0)
                Counts.Remove(itemId);
            else
                Counts[itemId] = left;
            return move;
        }

        private void Prune()
        {
            if (Counts.ContainsKey(0))
                Counts.Remove(0);
        }

        // ---------- persistence ----------

        private const byte SAVE_VERSION = 1;

        public void Save(BinaryWriter w)
        {
            w.Write(SAVE_VERSION);

            // Only persist entries that actually hold items.
            int countEntries = 0;
            foreach (KeyValuePair<int, int> kv in Counts)
                if (kv.Value > 0)
                    countEntries++;
            w.Write(countEntries);
            foreach (KeyValuePair<int, int> kv in Counts)
            {
                if (kv.Value <= 0)
                    continue;
                w.Write(kv.Key);
                w.Write(kv.Value);
            }

            w.Write(Limits.Count);
            foreach (KeyValuePair<int, int> kv in Limits)
            {
                w.Write(kv.Key);
                w.Write(kv.Value);
            }
        }

        public void Load(BinaryReader r)
        {
            Reset();
            byte ver = r.ReadByte();

            int countEntries = r.ReadInt32();
            for (int i = 0; i < countEntries; i++)
            {
                int id = r.ReadInt32();
                int cnt = r.ReadInt32();
                if (id > 0 && cnt > 0)
                    Counts[id] = cnt;
            }

            int limitEntries = r.ReadInt32();
            for (int i = 0; i < limitEntries; i++)
            {
                int id = r.ReadInt32();
                int lim = r.ReadInt32();
                if (id > 0)
                    Limits[id] = Mathf.Clamp(lim, 0, VDMod.HARD_MAX_LIMIT);
            }
        }
    }
}
