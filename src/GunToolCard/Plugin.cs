using BepInEx;
using HarmonyLib;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Resource;
using System.Reflection;
using UnityEngine;


namespace GunToolCard
{
    [BepInPlugin(GUID, "Gun tool", version)]
    [BepInProcess("LBoL.exe")]
    [BepInDependency(LBoLEntitySideloader.PluginInfo.GUID, BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "neo.lbol.card.memes.gunTool";
        public const string version = "1.0.0";

        private static readonly Harmony harmony = new Harmony(GUID);

        internal static BepInEx.Logging.ManualLogSource log;


        internal static TemplateSequenceTable sequenceTable = new TemplateSequenceTable();

        internal static IResourceSource embeddedSource = new EmbeddedSource(Assembly.GetExecutingAssembly());

        private void Awake()
        {
            log = Logger;

            // very important. Without this the entry point MonoBehaviour gets destroyed
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;


            EntityManager.RegisterSelf();

            harmony.PatchAll();

        }

        private void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }








    }
}
