using HarmonyLib;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoLEntitySideloader.CustomHandlers;
using LBoLEntitySideloader.ReflectionHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace CharSwapDemo.Core
{
    internal static class BattleManager
    {
        internal static AccessTools.FieldRef<BattleController, PlayerUnit> battlePlayerRef = AccessTools.FieldRefAccess<BattleController, PlayerUnit>(ConfigReflection.BackingWrap(nameof(BattleController.Player)));

        internal static void SetPlayer(this BattleController battle, PlayerUnit player) => battlePlayerRef(battle) = player;



        //[HarmonyPatch(typeof(BattleController), MethodType.Constructor, new Type[] { typeof(GameRunController), typeof(EnemyGroup), typeof(IEnumerable<Card>) })]
        class BattleController_Patch
        {
            static void Postfix(BattleController __instance)
            {
                __instance.SetPlayer(GRCharManager.GR.Player);
                
            }

            private static void OnRewardGenerating(StationEventArgs args)
            {
                throw new NotImplementedException();
            }
        }



    }
}
