using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Attributes;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActionRecord;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Dialogs;
using LBoL.Core.GapOptions;
using LBoL.Core.Helpers;
using LBoL.Core.Intentions;
using LBoL.Core.JadeBoxes;
using LBoL.Core.PlatformHandlers;
using LBoL.Core.Randoms;
using LBoL.Core.SaveData;
using LBoL.Core.Stations;
using LBoL.Core.Stats;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Adventures;
using LBoL.EntityLib.Adventures.Common;
using LBoL.EntityLib.Adventures.FirstPlace;
using LBoL.EntityLib.Adventures.Shared12;
using LBoL.EntityLib.Adventures.Shared23;
using LBoL.EntityLib.Adventures.Stage1;
using LBoL.EntityLib.Adventures.Stage2;
using LBoL.EntityLib.Adventures.Stage3;
using LBoL.EntityLib.Cards.Character.Cirno;
using LBoL.EntityLib.Cards.Character.Koishi;
using LBoL.EntityLib.Cards.Character.Marisa;
using LBoL.EntityLib.Cards.Character.Reimu;
using LBoL.EntityLib.Cards.Character.Sakuya;
using LBoL.EntityLib.Cards.Neutral;
using LBoL.EntityLib.Cards.Neutral.Black;
using LBoL.EntityLib.Cards.Neutral.Blue;
using LBoL.EntityLib.Cards.Neutral.Green;
using LBoL.EntityLib.Cards.Neutral.MultiColor;
using LBoL.EntityLib.Cards.Neutral.NoColor;
using LBoL.EntityLib.Cards.Neutral.Red;
using LBoL.EntityLib.Cards.Neutral.TwoColor;
using LBoL.EntityLib.Cards.Neutral.White;
using LBoL.EntityLib.Cards.Other.Adventure;
using LBoL.EntityLib.Cards.Other.Enemy;
using LBoL.EntityLib.Cards.Other.Misfortune;
using LBoL.EntityLib.Cards.Other.Tool;
using LBoL.EntityLib.Dolls;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.EnemyUnits.Character.DreamServants;
using LBoL.EntityLib.EnemyUnits.Lore;
using LBoL.EntityLib.EnemyUnits.Normal;
using LBoL.EntityLib.EnemyUnits.Normal.Bats;
using LBoL.EntityLib.EnemyUnits.Normal.Drones;
using LBoL.EntityLib.EnemyUnits.Normal.Guihuos;
using LBoL.EntityLib.EnemyUnits.Normal.Maoyus;
using LBoL.EntityLib.EnemyUnits.Normal.Ravens;
using LBoL.EntityLib.EnemyUnits.Opponent;
using LBoL.EntityLib.Exhibits;
using LBoL.EntityLib.Exhibits.Adventure;
using LBoL.EntityLib.Exhibits.Common;
using LBoL.EntityLib.Exhibits.Mythic;
using LBoL.EntityLib.Exhibits.Seija;
using LBoL.EntityLib.Exhibits.Shining;
using LBoL.EntityLib.JadeBoxes;
using LBoL.EntityLib.Mixins;
using LBoL.EntityLib.PlayerUnits;
using LBoL.EntityLib.Stages;
using LBoL.EntityLib.Stages.NormalStages;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoL.EntityLib.StatusEffects.Enemy.SeijaItems;
using LBoL.EntityLib.StatusEffects.Marisa;
using LBoL.EntityLib.StatusEffects.Neutral;
using LBoL.EntityLib.StatusEffects.Neutral.Black;
using LBoL.EntityLib.StatusEffects.Neutral.Blue;
using LBoL.EntityLib.StatusEffects.Neutral.Green;
using LBoL.EntityLib.StatusEffects.Neutral.Red;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;
using LBoL.EntityLib.StatusEffects.Neutral.White;
using LBoL.EntityLib.StatusEffects.Others;
using LBoL.EntityLib.StatusEffects.Reimu;
using LBoL.EntityLib.StatusEffects.Sakuya;
using LBoL.EntityLib.UltimateSkills;
using LBoL.Presentation;
using LBoL.Presentation.Animations;
using LBoL.Presentation.Bullet;
using LBoL.Presentation.Effect;
using LBoL.Presentation.I10N;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Transitions;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;
using Untitled;
using Untitled.ConfigDataBuilder;
using Untitled.ConfigDataBuilder.Base;
using Debug = UnityEngine.Debug;


namespace CommonOnly
{
    [BepInPlugin(CommonOnly.PInfo.GUID, CommonOnly.PInfo.Name, CommonOnly.PInfo.version)]
    [BepInDependency(LBoLEntitySideloader.PluginInfo.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(AddWatermark.API.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("LBoL.exe")]
    public class BepinexPlugin : BaseUnityPlugin
    {

        private static readonly Harmony harmony = CommonOnly.PInfo.harmony;

        internal static BepInEx.Logging.ManualLogSource log;

        internal static TemplateSequenceTable sequenceTable = new TemplateSequenceTable();

        internal static IResourceSource embeddedSource = new EmbeddedSource(Assembly.GetExecutingAssembly());

        // add this for audio loading
        internal static DirectorySource directorySource = new DirectorySource(CommonOnly.PInfo.GUID, "");


        private void Awake()
        {
            log = Logger;

            // very important. Without this the entry point MonoBehaviour gets destroyed
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            EntityManager.RegisterSelf();

            harmony.PatchAll();

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(AddWatermark.API.GUID))
                WatermarkWrapper.ActivateWatermark();
        }

        private void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }


        [HarmonyPatch]

        class GameRunController_Patch
        {

            static IEnumerable<MethodBase> TargetMethods()
            {
                return AccessTools.GetDeclaredMethods(typeof(GameRunController)).Where(m => m.Name == "RollCards").ToList();
            }


            static void Prefix(ref CardWeightTable weightTable)
            {
                weightTable = new CardWeightTable(RarityWeightTable.OnlyCommon, weightTable.OwnerTable, weightTable.CardTypeTable);
            }
        }





        [HarmonyPatch(typeof(ShopStation), nameof(ShopStation.OnEnter))]
        class ShopStation_Patch
        {
            static bool Prefix(ShopStation __instance)
            {
                List<ShopItem<Card>> list = new List<ShopItem<Card>>();
                foreach (Card card in __instance.Stage.GetShopNormalCards())
                {
                    list.Add(new ShopItem<Card>(__instance.GameRun, card, __instance.GetPrice(card, list.Count == __instance.DiscountCardNo), false, false));
                }
                __instance.DiscountCardNo = __instance.GameRun.ShopRng.NextInt(0, list.Count);
                list[__instance.DiscountCardNo].IsDiscounted = true;
                foreach (Card card2 in __instance.Stage.GetShopToolCards(2))
                {
                    list.Add(new ShopItem<Card>(__instance.GameRun, card2, __instance.GetPrice(card2, false), false, false));
                }
                __instance.ShopCards = list;
                List<ShopItem<Exhibit>> list2 = new List<ShopItem<Exhibit>>();
                for (int j = 0; j < 3; j++)
                {
                    Exhibit shopExhibit = __instance.Stage.GetShopExhibit(j == 2);
                    list2.Add(new ShopItem<Exhibit>(__instance.GameRun, shopExhibit, __instance.GetPrice(shopExhibit), false, false));
                }
                __instance.ShopExhibits = list2;

                return false;
            }

        }



        [HarmonyPatch(typeof(ShopPanel), nameof(ShopPanel.SetShop))]
        class ShopPanel_Patch
        {
            static bool Prefix(ShopPanel __instance)
            {
                int money = __instance.ShopStation.GameRun.Money;
                if (__instance.ShopStation.ShopCards.Count > ShopPanel.MaxCardCount)
                {
                    Debug.LogWarning("there's too many shopCards to show.");
                }
                for (int i = 0; i < Math.Min(__instance.ShopStation.ShopCards.Count, ShopPanel.MaxCardCount); i++)
                {
                    ShopItem<Card> shopItem = __instance.ShopStation.ShopCards[i];
                    if (shopItem != null)
                    {
                        if (shopItem.IsSoldOut)
                        {
                            __instance.shopCardList[i].Close();
                        }
                        else
                        {
                            __instance.shopCardList[i].SetCard(shopItem.Content, shopItem.Price, money >= shopItem.Price, shopItem.IsDiscounted);
                        }
                    }
                    else
                    {
                        __instance.shopCardList[i].Close();
                    }
                }
                if (__instance.ShopStation.ShopExhibits.Count > ShopPanel.MaxExhibitCount)
                {
                    Debug.LogWarning("there's too many shopExhibits to show.");
                }
                for (int j = 0; j < Math.Min(__instance.ShopStation.ShopExhibits.Count, 3); j++)
                {
                    ShopItem<Exhibit> shopItem2 = __instance.ShopStation.ShopExhibits[j];
                    if (shopItem2 != null)
                    {
                        if (shopItem2.Content != __instance.shopExhibitList[j].Exhibit)
                        {
                            __instance.shopExhibitList[j].SetExhibit(shopItem2.Content, shopItem2.Price, money >= shopItem2.Price);
                        }
                        else if (shopItem2.IsSoldOut)
                        {
                            __instance.shopExhibitList[j].Close();
                        }
                        else
                        {
                            __instance.shopExhibitList[j].SetExhibit(shopItem2.Content, shopItem2.Price, money >= shopItem2.Price);
                        }
                    }
                    else
                    {
                        __instance.shopExhibitList[j].Close();
                    }
                }
                __instance.ShowDetailCardService = false;
                __instance.SetCardService();
                return false;
            }
        }




    }
}
