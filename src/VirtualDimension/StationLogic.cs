using UnityEngine;

namespace VirtualDimension
{
    /// <summary>
    /// Core transfer between the tower (vanilla ILS) and the shared dimension.
    /// Runs right after the vanilla station local tick, so ALL vanilla behavior (drones,
    /// ships, belt I/O, power) is untouched. Only 星际供应 (remote supply) slots upload and
    /// only 星际需求 (remote demand) slots extract.
    ///
    /// Local logistics have priority: the lower half of a slot's cap stays local.
    /// 星际供应 slots only push: what exceeds half of the cap goes to the dimension, and
    /// they never pull from it. Only stations explicitly set to 星际需求 ever receive items
    /// from the shared dimension (backfilling up to half of the cap).
    /// </summary>
    public static class StationLogic
    {
        public static void Service(StationComponent station, PlanetFactory factory)
        {
            if (station == null || station.storage == null)
                return;

            DimensionStorage dim = DimensionStorage.Instance;
            int budget = Mathf.Max(1, VDMod.TransferPerTick);

            for (int i = 0; i < station.storage.Length; i++)
            {
                StationStore s = station.storage[i];
                if (s.itemId <= 0 || s.max <= 0)
                    continue;

                // The lower half of the slot cap is the local working buffer.
                int half = s.max / 2;

                if (s.remoteLogic == ELogisticStorage.Supply)
                {
                    // Upload only the part above the local half, capped per tick.
                    // Supply slots never pull from the dimension.
                    int excess = s.count - half;
                    if (excess > 0)
                    {
                        int moved = dim.Insert(s.itemId, Mathf.Min(excess, budget));
                        if (moved > 0)
                            s.count -= moved;
                    }
                }
                else if (s.remoteLogic == ELogisticStorage.Demand)
                {
                    // Backfill from the dimension up to the local half, capped per tick.
                    int want = half - s.count;
                    if (want > 0)
                    {
                        int moved = dim.TakeOut(s.itemId, Mathf.Min(want, budget));
                        if (moved > 0)
                            s.count += moved;
                    }
                }

                // StationStore is a value type; write the changed copy back.
                station.storage[i] = s;
            }
        }
    }
}
