using BepInEx;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;


namespace ReasonableRequests
{
    [BepInPlugin(GUID, "Reasonable Requests", version)]
    [BepInProcess("LBoL.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "neo.lbol.gameplay.reasonableRequests";
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

        public static MethodInfo InnerMoveNext(Type type, string methodName)
        {
            var enumType = type.GetNestedTypes(AccessTools.allDeclared).Where(t => t.Name.Contains($"<{methodName}>")).Single();

            return AccessTools.Method(enumType, "MoveNext");
        }



        [HarmonyPatch]
        class Longnight_Patch
        {

            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return InnerMoveNext(typeof(StartBattleAction), "GetPhases");
            }



            static Card CheckAndCreateLongnight(Card pop)
            {
                Card card;
                switch (GameMaster.Instance.CurrentGameRun.CurrentStage.Level)
                {
                    case 2:
                        card = Library.CreateCard<NightMana1>();
                        break;
                    case 3:
                        card = Library.CreateCard<NightMana2>();
                        break;
                    case 4:
                        card = Library.CreateCard<NightMana2>();
                        break;
                    default:
                        card = Library.CreateCard<NightMana1>();
                        break;
                }
                return card;
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                int i = 0;
                CodeInstruction prevCi = null;
                foreach (var ci in instructions)
                {
                    if (ci.opcode == OpCodes.Stloc_0 && prevCi?.opcode == OpCodes.Ldloc_1)
                    {
                        log.LogDebug("inject");
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Longnight_Patch), nameof(Longnight_Patch.CheckAndCreateLongnight)));


                        yield return ci;

                    }
                    else
                    {
                        yield return ci;
                    }
                    prevCi = ci;
                    i++;
                }
            }
        }



        [HarmonyPatch]
        class Deadbattery_Patch
        {


            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.PropertyGetter(typeof(PlayerUnit), nameof(PlayerUnit.MaxPower));
            }

            static void Postfix(ref int __result, PlayerUnit __instance)
            {
                var gr = __instance.GameRun;
                if(gr.Puzzles.HasFlag(PuzzleFlag.LowPower))
                {
                    __result += __instance.PowerPerLevel / 2;
                }
            }
        }


    }
}
