using BepInEx;
using HarmonyLib;
using LBoL.ConfigData;
using LBoL.Presentation;
using LBoL.Presentation.UI.Panels;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;
using TMPro;
using System.Linq;

namespace BanList
{
    [BepInPlugin(GUID, "Ban list", version)]
    [BepInProcess("LBoL.exe")]
    [BepInDependency(AddWatermark.API.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "neo.lbol.mutators.banList";
        public const string version = "1.0.0";

        private static readonly Harmony harmony = new Harmony(GUID);

        internal static BepInEx.Logging.ManualLogSource log;

        private void Awake()
        {
            log = Logger;

            // very important. Without this the entry point MonoBehaviour gets destroyed
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;


            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(AddWatermark.API.GUID))
                WatermarkWrapper.ActivateWatermark();


            harmony.PatchAll();

        }

        private void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }


        enum FailureLevel 
        {
            None,
            Warning,
            Critical,
        }

        static List<string> banList = new List<string>();

        static List<string> upgradeRestrictList = new List<string>();

        static FailureLevel failureLevel = FailureLevel.None;


        static void LoadAndParseYaml()
        {

            try
            {
                var filePath = Path.Combine(Path.GetDirectoryName(BepInEx.Bootstrap.Chainloader.PluginInfos[GUID].Location), "BanList.yaml");

                using FileStream stream = new FileStream(filePath, FileMode.Open);

                using var reader = new StreamReader(stream, encoding: System.Text.Encoding.UTF8);

                var text = reader.ReadToEnd();

                try
                {
                    var deserializer = new DeserializerBuilder().Build();
                    var yamlObject = deserializer.Deserialize<Dictionary<string, List<string>>>(text);

                    if(yamlObject["Banlist"] != null)
                        banList = yamlObject["Banlist"];
                    if(yamlObject["UpgradeRestrictList"] != null)
                        upgradeRestrictList = yamlObject["UpgradeRestrictList"];

                }
                catch (Exception ex)
                {

                    failureLevel = FailureLevel.Critical;
                    log.LogError($"Error while parsing yaml. Check if the yaml format is valid.");
                    log.LogError($"{ex}");
                }
            }
            catch (Exception ex)
            {
                failureLevel = FailureLevel.Critical;
                log.LogError("Error while loading BanList.yaml. Are you sure BanList.yaml is in the same directory as BanList.dll?");
                log.LogError(ex);
            }
        }

        // oddly late injection point but it works
        [HarmonyPatch(typeof(ResourcesHelper), nameof(ResourcesHelper.InitializeAsync))]
        [HarmonyPriority(Priority.Low)]
        class CardConfig_Patch
        {

            public static void Postfix()
            {

                LoadAndParseYaml();

                foreach (var id in banList)
                {
                    var cc = CardConfig.FromId(id);
                    if (cc != null)
                    {
                        cc.IsPooled = false;
                    }
                    else
                    {
                        failureLevel = FailureLevel.Warning;
                        log.LogWarning($"Banlist: card with id: {id} was not found.");
                    }
                }

                foreach (var id in upgradeRestrictList)
                {
                    var cc = CardConfig.FromId(id);
                    if (cc != null)
                    {
                        cc.IsUpgradable = false;
                    }
                    else
                    {
                        failureLevel = FailureLevel.Warning;
                        log.LogWarning($"Upgrade restrict list: card with id: {id} was not found.");
                    }
                }
            }
        }


        // 2do make better/working ui feedback
        // [HarmonyPatch(typeof(StartGamePanel), nameof(StartGamePanel.OnShowing))]
        class StartGamePanel_Patch
        {
            static void Postfix(StartGamePanel __instance)
            {
                var hintText = __instance.difficultyPanelRoot.transform.Find("DifficultyHint")?.GetComponent<TextMeshProUGUI>();
                if (hintText != null)
                {
                    switch (failureLevel)
                    {
                        case FailureLevel.None:
                            hintText.text = "Banlist mode active!";
                            break;
                        case FailureLevel.Warning:
                            hintText.text = "Banlist mode active, but some card ids were wrong. Check log at BepInEx/LogOutput.log for details";

                            break;
                        case FailureLevel.Critical:
                            hintText.text = "Banlist mode  Check log at BepInEx/LogOutput.log for details";
                            break;
                    }
                }


            }
        }



    }
}
