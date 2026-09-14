using BepInEx.Logging;
using CommonAPI.Systems.ModLocalization;

namespace VirtualDimension
{
    /// <summary>
    /// The vanilla planetary logistics station (PLS, item 2103) is used as-is — no custom
    /// protos are registered. The mod only adds dimension upload/download behavior on top
    /// of it, plus the Alt+5 keybind text.
    /// </summary>
    public static class Content
    {
        public static ManualLogSource Log;

        public static void Register(ManualLogSource log)
        {
            Log = log;

            LocalizationModule.RegisterTranslation("KEY" + VDMod.KEYBIND_NAME,
                "Open Virtual Dimension inventory",
                "打开虚拟维度空间界面",
                "Ouvrir l'inventaire de la dimension virtuelle");

            log.LogInfo($"Dimension behavior bound to vanilla PLS item {VDMod.TOWER_ITEM_ID}.");
        }

        /// <summary>True when the station entity is a planetary logistics station (our dimension tower).</summary>
        public static bool IsTowerEntity(PlanetFactory factory, StationComponent station)
        {
            if (factory == null || station == null || station.entityId <= 0)
                return false;
            EntityData[] pool = factory.entityPool;
            if (pool == null || station.entityId >= pool.Length)
                return false;
            return pool[station.entityId].protoId == (short)VDMod.TOWER_ITEM_ID;
        }
    }
}
