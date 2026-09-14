using BepInEx.Logging;
using HarmonyLib;

namespace VirtualDimension
{
    /// <summary>
    /// The game sizes SpaceSector.PrefabDescByModelIndex / PlanetFactory.PrefabDescByModelIndex
    /// as (model count + 64) but indexes them with raw model IDs. Any mod model whose ID is large
    /// (ours is 9010) throws IndexOutOfRange when CommonAPI re-initializes proto data
    /// (SpaceSector.InitPrefabDescArray). Rebuild both arrays sized by the max model ID instead.
    /// </summary>
    [HarmonyPatch(typeof(SpaceSector), nameof(SpaceSector.InitPrefabDescArray))]
    public static class SpaceSectorPrefabArrayFix
    {
        private static readonly ManualLogSource Log = Logger.CreateLogSource("VirtualDimension.PrefabFix");

        [HarmonyPrefix]
        public static bool Prefix()
        {
            if (SpaceSector.PrefabDescByModelIndex != null)
                return false;

            SpaceSector.PrefabDescByModelIndex = BuildArray();
            return false;
        }

        public static PrefabDesc[] BuildArray()
        {
            ModelProto[] models = LDB.models.dataArray;
            int max = 0;
            for (int i = 0; i < models.Length; i++)
            {
                if (models[i] != null && models[i].ID > max)
                    max = models[i].ID;
            }

            PrefabDesc[] arr = new PrefabDesc[max + 1];
            for (int i = 0; i < models.Length; i++)
            {
                if (models[i] != null)
                    arr[models[i].ID] = models[i].prefabDesc;
            }

            Log.LogInfo($"PrefabDescByModelIndex rebuilt: {models.Length} models, max id {max}, size {arr.Length}");
            return arr;
        }
    }

    [HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.InitPrefabDescArray))]
    public static class PlanetFactoryPrefabArrayFix
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            if (PlanetFactory.PrefabDescByModelIndex != null)
                return false;

            PlanetFactory.PrefabDescByModelIndex = SpaceSectorPrefabArrayFix.BuildArray();
            return false;
        }
    }
}
