using BepInEx;
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

        internal static BepInEx.Configuration.ConfigEntry<bool> isDebugConfig;

        internal static BepInEx.Configuration.ConfigEntry<bool> isExtraDebugConfig;




        private void Awake()
        {
            log = Logger;

            // very important. Without this the entry point MonoBehaviour gets destroyed
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            isDebugConfig = Config.Bind("Main", "CheckAchievementCount", false);

            isExtraDebugConfig = Config.Bind("Main", "DetailedDebug", false);


            harmony.PatchAll();

            if(isDebugConfig.Value)
                log.LogDebug($"UnlockAchievement calls patched: {DisableJadeboxCheck_Patch.totalACount}, v1.4 + 12 for char clears, + 11 for boss kills, + 1 for clear with jadebox, + 1 for Rumia = {DisableJadeboxCheck_Patch.totalACount + 12 + 11 + 1 + 1}, total achievements: {Enum.GetNames(typeof(AchievementKey)).Length}");

        }

        private void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();

            DisableJadeboxCheck_Patch.totalACount = 0;
        }


    }
}
