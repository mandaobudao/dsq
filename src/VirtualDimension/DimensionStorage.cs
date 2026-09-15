using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VirtualDimension
{
    /// <summary>
    /// The shared "dimension" inventory. There is a single global storage per save,
    /// every Virtual Dimension Tower uploads to / draws from it. Persisted through
    /// the DSPModSave IModCanSave interface.
    ///
    /// Thread safety: the game ticks factories in parallel on worker threads, so all
    /// access is guarded by a lock to prevent concurrent dictionary corruption.
    /// </summary>
    public class DimensionStorage
    {
        public static readonly DimensionStorage Instance = new DimensionStorage();

        private readonly object _lock = new object();

        // itemId -> stored amount
        private readonly Dictionary<int, int> _counts = new Dictionary<int, int>();

        // itemId -> explicit cap. Items that were never configured use the default rule.
        private readonly Dictionary<int, int> _limits = new Dictionary<int, int>();

        public void Reset()
        {
            lock (_lock)
            {
                _counts.Clear();
                _limits.Clear();
            }
        }

        public int GetCount(int itemId)
        {
            lock (_lock)
            {
                return _counts.TryGetValue(itemId, out int v) ? v : 0;
            }
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
            lock (_lock)
            {
                if (_limits.TryGetValue(itemId, out int v))
                    return v;
            }
            return DefaultLimit(itemId);
        }

        public void SetLimit(int itemId, int value)
        {
            value = Mathf.Clamp(value, 0, VDMod.HARD_MAX_LIMIT);
            lock (_lock)
            {
                _limits[itemId] = value;
                if (_counts.TryGetValue(itemId, out int c) && c > value)
                    _counts[itemId] = value;
                Prune();
            }
        }

        public void ResetLimit(int itemId)
        {
            lock (_lock)
            {
                _limits.Remove(itemId);
                int limit = DefaultLimit(itemId);
                if (_counts.TryGetValue(itemId, out int c) && c > limit)
                    _counts[itemId] = limit;
                Prune();
            }
        }

        /// <summary>Insert items into the dimension. Returns the amount actually accepted.</summary>
        public int Insert(int itemId, int want)
        {
            if (want <= 0 || itemId <= 0)
                return 0;
            lock (_lock)
            {
                int limit = _limits.TryGetValue(itemId, out int lv) ? lv : DefaultLimit(itemId);
                int cur = _counts.TryGetValue(itemId, out int cv) ? cv : 0;
                int space = limit - cur;
                int move = want < space ? want : space;
                if (move <= 0)
                    return 0;
                _counts[itemId] = cur + move;
                return move;
            }
        }

        /// <summary>Pull items out of the dimension. Returns the amount actually taken.</summary>
        public int TakeOut(int itemId, int want)
        {
            if (want <= 0 || itemId <= 0)
                return 0;
            lock (_lock)
            {
                int cur = _counts.TryGetValue(itemId, out int v) ? v : 0;
                int move = want < cur ? want : cur;
                if (move <= 0)
                    return 0;
                int left = cur - move;
                if (left <= 0)
                    _counts.Remove(itemId);
                else
                    _counts[itemId] = left;
                return move;
            }
        }

        /// <summary>Snapshot for UI display (thread-safe copy).</summary>
        public List<KeyValuePair<int, int>> GetCountsSnapshot()
        {
            lock (_lock)
            {
                var list = new List<KeyValuePair<int, int>>(_counts.Count);
                foreach (var kv in _counts)
                    list.Add(new KeyValuePair<int, int>(kv.Key, kv.Value));
                return list;
            }
        }

        /// <summary>Whether this item has an explicitly-set cap (not a default).</summary>
        public bool IsCustomLimit(int itemId)
        {
            lock (_lock)
            {
                return _limits.ContainsKey(itemId);
            }
        }

        /// <summary>Snapshot of limits for UI display (thread-safe copy).</summary>
        public List<KeyValuePair<int, int>> GetLimitsSnapshot()
        {
            lock (_lock)
            {
                var list = new List<KeyValuePair<int, int>>(_limits.Count);
                foreach (var kv in _limits)
                    list.Add(new KeyValuePair<int, int>(kv.Key, kv.Value));
                return list;
            }
        }

        private void Prune()
        {
            if (_counts.ContainsKey(0))
                _counts.Remove(0);
        }

        // ---------- persistence ----------

        private const byte SAVE_VERSION = 1;

        public void Save(BinaryWriter w)
        {
            lock (_lock)
            {
                w.Write(SAVE_VERSION);

                int countEntries = 0;
                foreach (KeyValuePair<int, int> kv in _counts)
                    if (kv.Value > 0)
                        countEntries++;
                w.Write(countEntries);
                foreach (KeyValuePair<int, int> kv in _counts)
                {
                    if (kv.Value <= 0)
                        continue;
                    w.Write(kv.Key);
                    w.Write(kv.Value);
                }

                w.Write(_limits.Count);
                foreach (KeyValuePair<int, int> kv in _limits)
                {
                    w.Write(kv.Key);
                    w.Write(kv.Value);
                }
            }
        }

        public void Load(BinaryReader r)
        {
            lock (_lock)
            {
                _counts.Clear();
                _limits.Clear();
                byte ver = r.ReadByte();

                int countEntries = r.ReadInt32();
                for (int i = 0; i < countEntries; i++)
                {
                    int id = r.ReadInt32();
                    int cnt = r.ReadInt32();
                    if (id > 0 && cnt > 0)
                        _counts[id] = cnt;
                }

                int limitEntries = r.ReadInt32();
                for (int i = 0; i < limitEntries; i++)
                {
                    int id = r.ReadInt32();
                    int lim = r.ReadInt32();
                    if (id > 0)
                        _limits[id] = Mathf.Clamp(lim, 0, VDMod.HARD_MAX_LIMIT);
                }
            }
        }
    }
}
