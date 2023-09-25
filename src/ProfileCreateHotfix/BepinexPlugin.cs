using BepInEx;
using HarmonyLib;
using LBoL.Presentation;
using LBoL.Presentation.UI.Panels;
using UnityEngine;


namespace ProfileCreateHotfix
{
    [BepInPlugin(ProfileCreateHotfix.PInfo.GUID, ProfileCreateHotfix.PInfo.Name, ProfileCreateHotfix.PInfo.version)]

    [BepInProcess("LBoL.exe")]
    public class BepinexPlugin : BaseUnityPlugin
    {

        private static readonly Harmony harmony = ProfileCreateHotfix.PInfo.harmony;

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







        [HarmonyPatch(typeof(StartupSelectLocalePanel), nameof(StartupSelectLocalePanel.Awake))]
        class StartupSelectLocalePanel_Patch
        {
            static void Prefix()
            {
                var ge = GameObject.Find("GameEntry");
                ge.GetComponent<GameEntry>().alioth.gameObject.SetActive(false);
                ge.GetComponent<GameEntry>().shanghaiAlice.gameObject.SetActive(false);

            }

        }




    }
}
