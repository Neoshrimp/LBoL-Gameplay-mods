using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.SaveData;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Adventures;
using LBoL.EntityLib.Adventures.Shared23;
using LBoL.EntityLib.Adventures.Stage3;
using LBoL.EntityLib.Cards.Character.Marisa;
using LBoL.EntityLib.Cards.Character.Sakuya;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.StatusEffects.Sakuya;
using LBoL.Presentation;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using static AchievementEnabler.BepinexPlugin;
using static HarmonyLib.AccessTools;
using static LBoL.EntityLib.Cards.Character.Marisa.Potion;

namespace AchievementEnabler
{

    [HarmonyPatch]
    //[HarmonyDebug]
    class DisableJadeboxCheck_Patch
    {


        static IEnumerable<MethodBase> TargetMethods()
        {

            yield return Method(typeof(UseCardAction).GetNestedTypes(AccessTools.allDeclared).Single(t => t.Name.Contains("DisplayClass17_0")), "<GetPhases>b__4");
            yield return Method(typeof(BattleController), "GainMana");
            yield return Method(typeof(BattleController), nameof(BattleController.RecordCardUsage));
            yield return Method(typeof(Firepower), "CheckAchievement");
            yield return Method(typeof(Unit), nameof(Unit.GainBlockShield));
            yield return Method(typeof(Unit), nameof(Unit.TryAddStatusEffect));
            yield return Method(typeof(RinnosukeTrade), nameof(RinnosukeTrade.SellExhibit));
            yield return EnumeratorMoveNext(Method(typeof(NarumiOfferCard), nameof(NarumiOfferCard.OfferDeckCard)));
            yield return EnumeratorMoveNext(Method(typeof(BackgroundDancers), nameof(BackgroundDancers.ForExhibit)));
            yield return Method(typeof(Potion.PotionAchievementCounter), "Increase");
            yield return Method(typeof(Knife.KnifeAchievementCounter), "Increase");
            yield return Method(typeof(Cold), nameof(Cold.Stack));

            yield return Method(typeof(TimeAuraSe), nameof(TimeAuraSe.CheckAchievement));
            yield return EnumeratorMoveNext(Method(typeof(GameMaster), nameof(GameMaster.BattleFlow)));
            yield return Method(typeof(GameMaster), nameof(GameMaster.SaveProfileWithEndingGameRun));

            yield return EnumeratorMoveNext(Method(typeof(GameRunVisualPanel), nameof(GameRunVisualPanel.ViewGainMoney)));
            yield return EnumeratorMoveNext(Method(typeof(PlayBoard), nameof(PlayBoard.ViewStartPlayerTurn)));

            yield return EnumeratorMoveNext(Method(typeof(GameDirector), nameof(GameDirector.InternalDieViewer)));
            yield return EnumeratorMoveNext(Method(typeof(GameDirector), nameof(GameDirector.StatisticalTotalDamageViewer)));

        }

        public static int totalACount = 0;


        static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);
            matcher = matcher.MatchForward(true, new CodeMatch[] { new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(GameRunController), nameof(GameRunController.IsAutoSeed))), OpCodes.Brfalse });

            Action<CodeMatch[]> findJadeboxCheck = (lookFor) => {
                matcher = matcher
                    .MatchForward(true, lookFor
                        )
                    .MatchForward(false, new CodeMatch(OpCodes.Brfalse))
                    .Set(OpCodes.Pop, null);
            };


            var usualCase = new CodeMatch[] {
                        new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(GameRunController), nameof(GameRunController.JadeBoxes))),
                        new CodeInstruction(OpCodes.Call, Method(typeof(CollectionsExtensions), "Empty").MakeGenericMethod(new Type[] { typeof(JadeBox) }))
                        };

            var specialCase = new CodeMatch[] {
                        new CodeInstruction(OpCodes.Ldfld, Field(typeof(GameRunRecordSaveData), nameof(GameRunRecordSaveData.JadeBoxes))),
                        new CodeInstruction(OpCodes.Call, Method(typeof(CollectionsExtensions), "Empty").MakeGenericMethod(new Type[] { typeof(string) }))
                        };

            try
            {
                if (original == Method(typeof(GameMaster), nameof(GameMaster.SaveProfileWithEndingGameRun)))
                    findJadeboxCheck(specialCase);
                else
                    findJadeboxCheck(usualCase);
                
            }
            catch (Exception ex)
            {
                log.LogWarning($"While patching {original.FullDescription()}: {ex}");
            }
            int aCount = 0;
            bool loop = true;
            if (isDebugConfig.Value)
            { 
                while (loop)
                {
                    try
                    {
                        matcher = matcher.SearchForward((CodeInstruction ci) =>
                        ci.Is(OpCodes.Callvirt, Method(typeof(IGameRunAchievementHandler), nameof(IGameRunAchievementHandler.UnlockAchievement)))
                        || ci.Is(OpCodes.Call, Method(typeof(GameMaster), nameof(GameMaster.UnlockAchievement), new Type[] { typeof(AchievementKey) }))
                        || ci.Is(OpCodes.Call, Method(typeof(GameMaster), nameof(GameMaster.UnlockAchievement), new Type[] { typeof(string) }))
                        )
                            .Advance(1)
                            .ThrowIfInvalid("deez");
                        aCount++;
                    }
                    catch (Exception ex)
                    {
                        loop = false;
                    }                
                }
                if (isExtraDebugConfig.Value)
                    log.LogDebug($"{original.FullDescription()} achievement count: {aCount}");
                totalACount += aCount;
                // v1.4 -3*4 char achievs - 1 rumia
            }

            return matcher.InstructionEnumeration();
        }
    }



    [HarmonyPatch(typeof(StartGamePanel), nameof(StartGamePanel.SetNoClearHint))]
    class StartGamePanel_Hint_Patch
    {

        static void Postfix(StartGamePanel __instance)
        {
            __instance.noClearHint.gameObject.SetActive(__instance._setSeed);
            __instance.noClearHint.text = __instance.noClearHint.text.Replace(" or Jade Box", "");
        }
    }




}
