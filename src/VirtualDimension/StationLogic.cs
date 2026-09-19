using UnityEngine;

namespace VirtualDimension
{
    /// <summary>
    /// Core transfer between towers (vanilla ILS) / orbital collectors and the shared
    /// dimension. Runs right after the vanilla local tick, so ALL vanilla behavior (drones,
    /// ships, belt I/O, power) is untouched. Transfers are instant and uncapped.
    ///
    /// ILS: only 星际供应 (remote supply) slots upload and only 星际需求 (remote demand)
    /// slots extract. Supply keeps the lower half of the slot cap local and pushes the
    /// entire surplus into the dimension; demand slots fill all the way up to the full cap.
    ///
    /// Orbital collectors (气态行星轨道采集器): every storage slot uploads everything above
    /// half of its cap — the lower half stays available to vanilla pickup ships.
    /// </summary>
    public static class StationLogic
    {
        public static void Service(StationComponent station, PlanetFactory factory)
        {
            if (station == null || station.storage == null)
                return;

            if (station.isCollector)
            {
                ServiceCollector(station);
                return;
            }

            DimensionStorage dim = DimensionStorage.Instance;

            for (int i = 0; i < station.storage.Length; i++)
            {
                StationStore s = station.storage[i];
                if (s.itemId <= 0 || s.max <= 0)
                    continue;

                if (s.remoteLogic == ELogisticStorage.Supply)
                {
                    // Push the entire surplus above the local half. Supply slots never pull.
                    int excess = s.count - s.max / 2;
                    if (excess > 0)
                    {
                        int moved = dim.Insert(s.itemId, excess);
                        if (moved > 0)
                            s.count -= moved;
                    }
                }
                else if (s.remoteLogic == ELogisticStorage.Demand)
                {
                    // Fill the whole slot from the dimension.
                    int want = s.max - s.count;
                    if (want > 0)
                    {
                        int moved = dim.TakeOut(s.itemId, want);
                        if (moved > 0)
                            s.count += moved;
                    }
                }

                // StationStore is a value type; write the changed copy back.
                station.storage[i] = s;
            }
        }

        /// <summary>Orbital collector: upload the entire surplus above half of each slot's cap.</summary>
        private static void ServiceCollector(StationComponent station)
        {
            DimensionStorage dim = DimensionStorage.Instance;

            for (int i = 0; i < station.storage.Length; i++)
            {
                StationStore s = station.storage[i];
                if (s.itemId <= 0 || s.max <= 0)
                    continue;

                int excess = s.count - s.max / 2;
                if (excess > 0)
                {
                    int moved = dim.Insert(s.itemId, excess);
                    if (moved > 0)
                    {
                        s.count -= moved;
                        station.storage[i] = s;
                    }
                }
            }
        }
    }
}
