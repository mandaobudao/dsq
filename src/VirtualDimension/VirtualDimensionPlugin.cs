using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CommonAPI;
using CommonAPI.Systems;
using CommonAPI.Systems.ModLocalization;
using crecheng.DSPModSave;
using HarmonyLib;
using UnityEngine;

namespace VirtualDimension
{
    [BepInPlugin(VDMod.GUID, VDMod.NAME, VDMod.VERSION)]
    [BepInDependency(CommonAPIPlugin.GUID)]
    [BepInDependency("crecheng.DSPModSave")]
    [CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(CustomKeyBindSystem), nameof(LocalizationModule))]
    public class VirtualDimensionPlugin : BaseUnityPlugin, IModCanSave
    {
        private Harmony _harmony;
        private PressKeyBind _openUiKey;

        private void Awake()
        {
            VDMod.ConfigFile = Config;
            VDMod.WindowWidthEntry = Config.Bind("UI", "WindowWidth", VDMod.DEFAULT_WINDOW_WIDTH,
                "Saved width of the Alt+5 dimension window (resizable via the bottom-right grip).");
            VDMod.WindowHeightEntry = Config.Bind("UI", "WindowHeight", VDMod.DEFAULT_WINDOW_HEIGHT,
                "Saved height of the Alt+5 dimension window.");

            _harmony = new Harmony(VDMod.GUID);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            Content.Register(Logger);

            CustomKeyBindSystem.RegisterKeyBindWithReturn<PressKeyBind>(new BuiltinKey
            {
                key = new CombineKey((int)KeyCode.Alpha5, CombineKey.ALT_COMB,
                    ECombineKeyAction.OnceClick, false),
                conflictGroup = 2052,
                name = VDMod.KEYBIND_NAME,
                canOverride = true
            });

            Logger.LogInfo($"{VDMod.NAME} {VDMod.VERSION} loaded.");
        }

        private void Update()
        {
            if (_openUiKey == null)
                _openUiKey = CustomKeyBindSystem.GetKeyBind(VDMod.KEYBIND_NAME) as PressKeyBind;

            if (_openUiKey != null && _openUiKey.keyValue && InGame())
                DimensionUI.Toggle();

            // While typing into the dimension window, don't leak key presses into the game.
            if (DimensionUI.Visible && DimensionUI.TextFieldFocused)
                VFInput.inputing = true;
        }

        private void OnGUI()
        {
            if (DimensionUI.Visible && InGame())
                DimensionUI.OnGUI();
        }

        private static bool InGame()
        {
            return GameMain.data != null && GameMain.mainPlayer != null;
        }

        // ---------- IModCanSave ----------

        public void Export(BinaryWriter w)
        {
            try
            {
                DimensionStorage.Instance.Save(w);
            }
            catch (System.Exception e)
            {
                Logger.LogError("Dimension save failed: " + e);
            }
        }

        public void Import(BinaryReader r)
        {
            try
            {
                DimensionStorage.Instance.Load(r);
                Logger.LogInfo("Dimension storage loaded.");
            }
            catch (System.Exception e)
            {
                Logger.LogError("Dimension load failed: " + e);
                DimensionStorage.Instance.Reset();
            }
        }

        public void IntoOtherSave()
        {
            DimensionStorage.Instance.Reset();
            DimensionUI.Visible = false;
        }
    }
}
