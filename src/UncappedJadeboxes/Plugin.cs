using BepInEx;
using HarmonyLib;
using LBoL.Presentation.UI.Panels;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;


namespace UncappedJadeboxes
{
    [BepInPlugin(GUID, "Uncapped Jadeboxes", version)]
    [BepInProcess("LBoL.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "neo.lbol.qol.uncappedJadeboxes";
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



        [HarmonyPatch(typeof(StartGamePanel), nameof(StartGamePanel.RefreshJadeBoxIcon))]
        class RefreshJadeBoxIcon_Patch
        {

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var patchedInstructions = new CodeMatcher(instructions, generator)
                    .MatchForward(false, OpCodes.Ldc_I4_3)
                    .SetAndAdvance(OpCodes.Ldstr, "∞")
                    .RemoveInstruction()
                    .End()
                    .MatchBack(false, new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(StartGamePanel), nameof(StartGamePanel.ActiveJadeBoxCount))))
                    .Advance(1)
                    .Set(OpCodes.Ldc_I4, int.MaxValue)
                    .InstructionEnumeration();


                // fix for HarmonyX bug
                foreach (var ci in patchedInstructions)
                {
                    if (ci.opcode == OpCodes.Leave)
                    {
                        yield return ci;
                        yield return new CodeInstruction(OpCodes.Nop);

                    }
                    else
                    {
                        yield return ci;
                    }
                }
            }






        }





    }
}
