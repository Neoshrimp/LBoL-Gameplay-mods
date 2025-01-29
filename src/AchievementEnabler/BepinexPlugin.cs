using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using LBoL.Core;
using System;
using UnityEngine;


namespace AchievementEnabler
{
    [BepInPlugin(PInfo.GUID, PInfo.Name, PInfo.version)]
    [BepInDependency("neo.lbol.frameworks.entitySideloader", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("LBoL.exe")]
    public class BepinexPlugin : BaseUnityPlugin
    {

        private static readonly Harmony harmony = PInfo.harmony;

        internal static BepInEx.Logging.ManualLogSource log;

        private static BepInEx.Configuration.ConfigEntry<bool> isDebugConfig;

        private static BepInEx.Configuration.ConfigEntry<bool> isExtraDebugConfig;

        internal static bool IsDebugConfig { get => isDebugConfig.Value; }
        internal static bool IsExtraDebugConfig { get => isExtraDebugConfig.Value; }

        private void Awake()
        {
            log = Logger;

            // very important. Without this the entry point MonoBehaviour gets destroyed
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            isDebugConfig = Config.Bind("Main", "CheckAchievementCount", false);

            isExtraDebugConfig = Config.Bind("Main", "DetailedDebug", false);


            harmony.PatchAll();

            if(IsDebugConfig)
                log.LogInfo($"UnlockAchievement calls counted: {DisableJadeboxCheck_Patch.totalACount} (includes finish with JB achievement), v1.6 + 12 for char clears, + 11 for boss kills, + 1 for Mystia, + 1 for Rumia(doesn't check JB) = {DisableJadeboxCheck_Patch.totalACount + 12 + 11 + 1 + 1}, total achievements: {Enum.GetNames(typeof(AchievementKey)).Length}");

        }

        private void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();

            DisableJadeboxCheck_Patch.totalACount = 0;
        }


    }
}
