using BepInEx;
using HarmonyLib;
using LBoL.EntityLib.Adventures;
using LBoL.Presentation;
using UnityEngine;


namespace HelpMeErin
{
    [BepInPlugin(GUID, "Help me, Eirin!", version)]
    [BepInProcess("LBoL.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "neo.lbol.qol.helpMeEirin";
        public const string version = "1.0.0";

        private static readonly Harmony harmony = new Harmony(GUID);

        internal static BepInEx.Logging.ManualLogSource log;

        private void Awake()
        {
            log = Logger;

            // very important. Without this the entry point MonoBehaviour gets destroyed
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            harmony.PatchAll();

        }

        private void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }



        [HarmonyPatch(typeof(Debut), nameof(Debut.InitVariables))]
        class Debut_Patch
        {
            static void Prefix()
            {
                if(GameMaster.Instance?.CurrentGameRun != null)
                    GameMaster.Instance.CurrentGameRun.HasClearBonus = true;
            }
        }





    }
}
