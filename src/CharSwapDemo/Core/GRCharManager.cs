
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.SaveData;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits.Shining;
using LBoL.EntityLib.PlayerUnits;
using LBoL.Presentation;
using LBoL.Presentation.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using static UnityEngine.Random;

namespace CharSwapDemo.Core
{
    // 2do persist
    public static class GRCharManager
    {
        static ConditionalWeakTable<GameRunController, CharsData> cwt_charsData = new ConditionalWeakTable<GameRunController, CharsData>();

        static WeakReference<GameRunController> gr_ref;

        [MaybeNull]
        public static GameRunController GR
        {
            get
            {
                var rez = GameMaster.Instance?.CurrentGameRun;
                if (rez == null && gr_ref != null)
                    gr_ref.TryGetTarget(out rez);
                return rez;
            }
        }

        internal static CharsData GetCharsData(GameRunController gr) => cwt_charsData.GetOrCreateValue(gr);

        [HarmonyPatch(typeof(GameRunController), MethodType.Constructor)]
        [HarmonyPriority(Priority.High)]
        class GameRunController_Patch
        {
            static void Prefix(GameRunController __instance)
            {
                GetCharsData(__instance);
                gr_ref = new WeakReference<GameRunController>(__instance);
            }
        }



        public static void AttachCharData(this GameRunController gr, IEnumerable<string> subIds)
        {
            var mainPlayer = gr.Player;
            if (gr.Player == null)
            {
                Log.LogError($"{nameof(GameRunController)} doesn't have Player set.");
                return;
            }
            if (cwt_charsData.TryGetValue(gr, out var charData))
            {
                
                charData.mainChar = gr.Player;

                foreach (var id in subIds)
                {
                    var subP = Library.CreatePlayerUnit(id);

                    // 2do maybe more initialization
                    if (subP.Us == null)
                    {
                        subP.SetUs(Library.CreateUs(subP.Config.UltimateSkillA));
                        subP.Us.GameRun = gr;
                    }
                    subP.EnterGameRun(gr);
                    if (gr.Puzzles.HasFlag(PuzzleFlag.LowMaxHp))
                    {
                        int maxHp = Math.Max(1, subP.MaxHp - 10);
                        subP.SetMaxHp(maxHp, maxHp);
                    }

                    charData.subChars.Add(subP);
                }
            }
            else
            {
                Log.LogWarning($"{nameof(GameRunController)} doesn't have character data attached.");
            }
        }


        // called too early
        //[HarmonyPatch(typeof(GameDirector), nameof(GameDirector.PlayerUnitView), MethodType.Getter)]
        class GameDirector_Patch
        {
            static bool Prefix(ref UnitView __result)
            {
                // 2do jank
                if (GR?.Player != null)
                {
                    //Log.LogDebug("deez");
                    __result = GR.Player.View as IUnitView as UnitView;
                    return false;
                }

                return true;
            }
        }



        internal static void ProcessGrInitDemo(GameRunController gr)
        {
            var subs = new string[] { nameof(Marisa) };
            gr.AttachCharData(subs);

        }

        internal static IEnumerator ProcessVisualInitDemo(GameRunController gr)
        {
            foreach (var pu in GetCharsData(gr).subChars)
            {
                yield return GameDirector.Instance.InternalLoadPlayerAsync(pu, false).ToCoroutine();
            }
        }


    }


    [HarmonyPatch(typeof(GameMaster), nameof(GameMaster.CoSetupGameRun))]
    class GameMaster_Patch
    {
        static IEnumerator Postfix(IEnumerator routine, GameRunController gameRun)
        {
            while (routine.MoveNext())
                yield return routine.Current;

            var subCharRoutine = GRCharManager.ProcessVisualInitDemo(gameRun);
            while (subCharRoutine.MoveNext())
                yield return subCharRoutine.Current;
            // 2DO
            gameRun.Player = GameDirector.Player.Unit as PlayerUnit;
        }
    }




    [HarmonyPatch(typeof(GameRunController), MethodType.Constructor, new Type[] {typeof(GameRunStartupParameters)})]
    [HarmonyPriority(Priority.Low)]
    class AttachDemo_Patch
    {
        static void Postfix(GameRunController __instance)
        {
            GRCharManager.ProcessGrInitDemo(__instance);
        }
    }


    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.Restore))]
    class RestoreAttachDemo_Patch
    {
        static void Postfix(GameRunController __result)
        {
            GRCharManager.ProcessGrInitDemo(__result);
        }
    }


    public class CharsData
    {
        public PlayerUnit mainChar;

        public List<PlayerUnit> subChars = new List<PlayerUnit>();
        // sub position
        // sub Us, could be shared
    }
}
