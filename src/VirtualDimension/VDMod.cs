namespace VirtualDimension
{
    /// <summary>
    /// Global constants and runtime config for the Virtual Dimension mod.
    /// </summary>
    public static class VDMod
    {
        public const string GUID = "org.trae.virtualdimension";
        public const string NAME = "VirtualDimension";
        public const string VERSION = "1.2.0";

        // The vanilla interstellar logistics station (ILS, 2104) IS the virtual dimension tower.
        // We do not register any custom proto; the mod only adds dimension behavior on top of it.
        public const int TOWER_ITEM_ID = 2104;

        public const string KEYBIND_NAME = "OpenVirtualDimensionUI";

        // Dimension defaults.
        public const int HARD_MAX_LIMIT = 10_000_000;
        public const int DEFAULT_ITEM_LIMIT = 2_000_000;
        public const int DEFAULT_BUILDING_LIMIT = 50;
        public const int DEFAULT_TRANSFER_PER_TICK = 3600;

        public const float DEFAULT_WINDOW_WIDTH = 1000f;
        public const float DEFAULT_WINDOW_HEIGHT = 620f;

        public static BepInEx.Configuration.ConfigFile ConfigFile;
        public static BepInEx.Configuration.ConfigEntry<float> WindowWidthEntry;
        public static BepInEx.Configuration.ConfigEntry<float> WindowHeightEntry;

        public static int TransferPerTick = DEFAULT_TRANSFER_PER_TICK;
    }
}
