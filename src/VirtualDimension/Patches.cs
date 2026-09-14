using HarmonyLib;

namespace VirtualDimension
{
    // ---------- station behavior ----------
    //
    // Design: the vanilla ILS (interstellar logistics station) keeps working exactly as
    // before (local drone logistics, ships, belt I/O, power). Our dimension transfer runs
    // right after the vanilla local tick: remote-supply slots upload into the shared
    // dimension, remote-demand slots pull from it — cross-planet transfer without ships.

    [HarmonyPatch(typeof(StationComponent), nameof(StationComponent.InternalTickLocal))]
    public static class StationInternalTickLocalPatch
    {
        public static void Postfix(StationComponent __instance, PlanetFactory factory, float power)
        {
            if (power > 0.01f && Content.IsTowerEntity(factory, __instance))
                StationLogic.Service(__instance, factory);
        }
    }

    // ---------- station window UI ----------

    [HarmonyPatch(typeof(UIStationWindow), "_OnUpdate")]
    public static class UIStationWindowUpdatePatch
    {
        private const string Title = "虚拟维度塔";

        public static void Postfix(UIStationWindow __instance)
        {
            try
            {
                if (__instance.transport == null)
                    return;
                int stationId = Traverse.Create(__instance).Field("_stationId").GetValue<int>();
                if (stationId <= 0)
                    return;
                StationComponent station = __instance.transport.GetStationComponent(stationId);
                if (!Content.IsTowerEntity(__instance.transport.factory, station))
                    return;

                if (__instance.titleText != null)
                    __instance.titleText.text = Title;
            }
            catch
            {
                // UI patching must never break the station window
            }
        }
    }

    // ---------- save lifecycle ----------

    [HarmonyPatch(typeof(GameData), nameof(GameData.NewGame))]
    public static class GameDataNewGamePatch
    {
        public static void Postfix()
        {
            DimensionStorage.Instance.Reset();
        }
    }
}
