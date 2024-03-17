using HarmonyLib;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.EntityLib.Adventures;
using LBoL.Presentation;
using LBoL.Presentation.Effect;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using StSStuffMod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static LBoLEntitySideloader.Entities.CardTemplate;
using static StSJadebox.BepinexPlugin;
using static StSStuffMod.StSCoffeeDripperDef;
using static StSStuffMod.StSFusionHammerDef;
using static StSStuffMod.StSSozuDef;

namespace StSJadebox
{
    public sealed class StSTermsOfServiceJadeboxDef : JadeBoxTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(StSCursedManaJadebox);
        }

        public override LocalizationOption LoadLocalization()
        {
            return new DirectLocalization(new Dictionary<string, object>() {
                { "Name", "Terms of Service" },
                { "Description", "Lose your starting Exhibit. Gain |Fussion Hammer|, |Coffee Dripper| and |Sozu|."}
            });
        }

        public override JadeBoxConfig MakeConfig()
        {
            var config = DefaultConfig();
            return config;
        }


        


        [HarmonyPatch(typeof(Debut), nameof(Debut.ExchangeExhibit))]
        class BanExhibitSwap_Patch
        {
            static bool Prefix(ref IEnumerator __result, Debut __instance, int optionIndex)
            {

                var gr = GameMaster.Instance.CurrentGameRun;
                IReadOnlyList<JadeBox> jadeBox =  gr.JadeBoxes;
                

                if (jadeBox != null && jadeBox.Count > 0)
                {
                    if (gr.JadeBoxes.Any((JadeBox jb) => jb is StSCursedManaJadebox))
                    {

                        var Eirin = GameDirector.Enemies[0];


                        Eirin.Chat(StringDecorator.Decorate("Fufufu.. That's against the |Terms of Service|. You knew that, didn't you? Don't be cheeky now~"), time: 6f, ChatWidget.CloudType.RightTalk, delay: 0);




                        switch (GameDirector.Player.Unit.Id)
                        {
                            case VanillaCharNames.Reimu:
                                // do NOT use this Random function for anything game logic related
                                // or the whole concept of run seed will be meaningless
                                if (UnityEngine.Random.Range(0f, 1f) < 0.7f)
                                    GameDirector.Player.Chat("Tsk..", time: 2f, ChatWidget.CloudType.LeftTalk, delay: 2f);
                                else
                                    GameDirector.Player.Chat("Stingy hag..", time: 2.5f, ChatWidget.CloudType.LeftThink, delay: 2f);
                                break;

                            case VanillaCharNames.Marisa:
                                GameDirector.Player.Chat("You think I've read them? ¯\\_(ツ)_/¯", time: 3f, ChatWidget.CloudType.LeftTalk, delay: 2f);
                                break;
                            case VanillaCharNames.Sakuya:
                                if (UnityEngine.Random.Range(0f, 1f) < 0.97f)
                                    GameDirector.Player.Chat("My apologies.", time: 2.5f, ChatWidget.CloudType.LeftTalk, delay: 2f);
                                else
                                {
                                    GameDirector.Player.Chat("Time stop DEEZNUTS!!!", time: 2.5f, ChatWidget.CloudType.LeftTalk, delay: 2f);
                                    // recreates modified original method
                                    __result = DelayTimeEffect(3f, __instance, optionIndex);
                                }
                                break;
                            case VanillaCharNames.Cirno:
                                if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
                                    GameDirector.Player.Chat("???", time: 2f, ChatWidget.CloudType.LeftTalk, delay: 2f);
                                else
                                    GameDirector.Player.Chat("Cirno does not understand..", time: 2.5f, ChatWidget.CloudType.LeftTalk, delay: 2f);

                                break;
                            default:
                                GameDirector.Player.Chat("...", time: 2f, ChatWidget.CloudType.LeftTalk, delay: 2f);
                                break;
                        }


                        return false;
                    }
                }
                return true;
            }


            

            static IEnumerator DelayTimeEffect(float delay, Debut __instance, int optionIndex)
            {
                yield return new WaitForSeconds(delay);
                EffectManager.CreateEffect("ExtraTime", GameDirector.Player.transform, delay: 0f, startActive: true);
                AudioManager.PlaySfx("ExtraTurnLaunch");
                yield return new WaitForSeconds(0.7f);
                __instance.GameRun.LoseExhibit(__instance.GameRun.Player.Exhibits[0], false, true);
                yield return __instance.GameRun.GainExhibitRunner(__instance._exhibit, true, new VisualSourceData
                {
                    SourceType = VisualSourceType.Vn,
                    Index = optionIndex
                });
            }

        }


    }







    [EntityLogic(typeof(StSTermsOfServiceJadeboxDef))]
    public sealed class StSCursedManaJadebox : JadeBox
    {

        protected override void OnGain(GameRunController gameRun)
        {

            gameRun.LoseExhibit(base.GameRun.Player.Exhibits[0], false, true);

            GameMaster.Instance.StartCoroutine(GainExhibits(gameRun));
        }


        private IEnumerator GainExhibits(GameRunController gameRun)
        {
            var stsExhibits = new HashSet<Type> { typeof(StSFusionHammer), typeof(StSCoffeeDripper), typeof(StSSozu) };
            foreach (var et in stsExhibits)
            { 
                yield return gameRun.GainExhibitRunner(Library.CreateExhibit(et));
            }

            gameRun.ExhibitPool.RemoveAll(e => stsExhibits.Contains(e));
        }
    }


}
