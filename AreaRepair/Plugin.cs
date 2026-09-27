using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace AreaRepair
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.frizzqol.arearepair";
        public const string PluginName = "FrizzQOL Area Repair";
        public const string PluginVersion = "0.1.0";

        internal static Plugin Instance { get; private set; }

        internal static ConfigEntry<float> RepairRadius;

        private Harmony _harmony;

        private void Awake()
        {
            Instance = this;
            RepairRadius = Config.Bind(
                "General",
                "RepairRadius",
                5f,
                "Sphere radius in meters around the hammer-repair target. 0 or less disables area repair.");
            _harmony = new Harmony(PluginGuid);
            try
            {
                _harmony.PatchAll();
                Logger.LogInfo($"{PluginName} {PluginVersion} loaded");
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Failed to patch Player.Repair: {ex.Message}");
            }
        }

        internal static float CurrentRadius()
        {
            return RepairRadius != null ? RepairRadius.Value : 5f;
        }

        internal static void LogInfo(string message)
        {
            if (Instance != null)
            {
                Instance.Logger.LogInfo(message);
            }
        }

        internal static void LogWarning(string message)
        {
            if (Instance != null)
            {
                Instance.Logger.LogWarning(message);
            }
        }

        internal static void LogError(string message)
        {
            if (Instance != null)
            {
                Instance.Logger.LogError(message);
            }
        }
    }
}
