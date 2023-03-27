using BepInEx;
using HarmonyLib;
using LBoL.ConfigData;
using LBoL.Presentation;
using System;
using UnityEngine;


namespace NerfTenshi
{
    // BePinEx attributes required for BePinEx to load the plugin in the first place
    [BepInPlugin(GUID, "Reduce Tenshi's spellcard damage", version)]
    // conditional dependency for watermark
    [BepInDependency(AddWatermark.API.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("LBoL.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "neo.gameplay.example.Tenshi";
        public const string version = "1.0.0";

        private static readonly Harmony harmony = new Harmony(GUID);

        internal static BepInEx.Logging.ManualLogSource log;

        
        // BaseUnityPlugin extends MonoBehaviour so any MonoBehaviour message method
        // like Update, FixedUpdate and so on can be used
        private void Awake()
        {
            log = Logger;

            // very important. Without this the entry point MonoBehaviour gets destroyed
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            harmony.PatchAll();

            // an example how to add conditional dependency which almost always works
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(AddWatermark.API.GUID))
                WatermarkWrapper.ActivateWatermark();

        }

        private void OnDestroy()
        {
            // needed for BePinEx scriptengine
            if (harmony != null)
                harmony.UnpatchSelf();
        }


        // indicates that this type defines a Harmony patch
        // in this case, it targets GameEntry.StartAsync method meaning the patch will add some logic to the said method
        // originally GameEntry.StartAsync was a private method making the 'GameEntry.StartAsync' symbol illegal 
        // but thanks to assembly publicizer it can easily be accessed without use of reflection
        [HarmonyPatch(typeof(GameEntry), nameof(GameEntry.StartAsync))]
        class ConfigData_Patch
        {

            static float spellcardDamageMul = 0.5f;

            static int  spellcardDamageMod = -16;


            static int? ModDmg(int? dmg, float mul = 1f, int mod = 0)
            {
                if (dmg == null)
                    return null;
                return (int?)Math.Max((Math.Max((int)dmg+mod, 1)* mul), 1);   
            }

            // code in static Postfix method is appended to end of the targeted method, effectively
            // executing it every time after the target method is called
            // GameEntry.StartAsync is called only once (I think) after the game has started to initialize all the data 
            static public void Postfix()
            {
                var tenshiId = "Tianzi";
                var tenshiConfig = EnemyUnitConfig.FromId(tenshiId);

                // after original values have been loaded, they can be modified however we want
                if (tenshiConfig != null)
                {
                   
                    tenshiConfig.Damage3 = ModDmg(tenshiConfig.Damage3, mod: spellcardDamageMod);
                    tenshiConfig.Damage3Hard = ModDmg(tenshiConfig.Damage3Hard, mod: spellcardDamageMod);
                    tenshiConfig.Damage3Lunatic = ModDmg(tenshiConfig.Damage3Lunatic, mod: spellcardDamageMod);

                }
                else
                {
                    log.LogWarning($"Failed to find EnemyUnitConfig with Id: {tenshiId}");
                }

            }

        }


    }
}
