using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.Cards.Character.Cirno;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using LBoL.Core.Units;
using System.Linq;
using HarmonyLib;
using static VanillaTweaks.Plugin;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace VanillaTweaks
{

    [OverwriteVanilla]
    public sealed class FairyTreeCardDefinition : CardTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(LBoL.EntityLib.Cards.Character.Cirno.FairyTree);

        }

        [DontOverwrite]
        public override CardImages LoadCardImages()
        {
            return null;
        }

        public override LocalizationOption LoadLocalization()
        {
            return new GlobalLocalization();
        }

        [DontOverwrite]
        public override CardConfig MakeConfig()
        {
            return null;
        }

        [EntityLogic(typeof(FairyTreeCardDefinition))]
        public sealed class FairyTree : Card
        {
            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {
                // make sure the status effect is referring to the custom effect rather than the original 
                yield return BuffAction<FairyTreeSe>(Value1, 0, 0, 0, 0.2f);
            }
        }
    }


    [OverwriteVanilla]
    public sealed class FairyTreeSeDefinition : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(LBoL.EntityLib.StatusEffects.Cirno.FairyTreeSe);
        }

        [DontOverwrite]
        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("FairyTreeSe.png", embeddedSource);
        }

        public override LocalizationOption LoadLocalization()
        {
            var globalLoc = new GlobalLocalization(embeddedSource);

            globalLoc.LocalizationFiles.AddLocaleFile(Locale.En, "StatusEffectsEn.yaml");
            return globalLoc;
        }

        [DontOverwrite]
        public override StatusEffectConfig MakeConfig()
        {
            return null;
        }
    }


    [EntityLogic(typeof(FairyTreeSeDefinition))]
    public sealed class FairyTreeSe : StatusEffect
    {
        public ManaGroup Mana
        {
            get
            {
                return ManaGroup.Philosophies((Level > 0) ? Level : 1);
            }
        }

        protected override void OnAdded(Unit unit)
        {
            _active = true;
            HandleOwnerEvent(Battle.Player.TurnStarting, delegate (UnitEventArgs _)
            {
                _active = true;
            });


            // reactors on all possible ways a card can enter the hand
            ReactOwnerEvent(Battle.CardsAddedToHand, new EventSequencedReactor<CardsEventArgs>(OnCardsCreatedToHand));

            ReactOwnerEvent(Battle.CardDrawn, new EventSequencedReactor<CardEventArgs>(OnCardDrawn));

            ReactOwnerEvent(Battle.CardMoved, new EventSequencedReactor<CardMovingEventArgs>(OnCardMovedToHand));
        }

        private IEnumerable<BattleAction> OnCardsCreatedToHand(CardsEventArgs args)
        {
            foreach (var action in CheckTrigger(args.Cards)) { yield return action; }
        }

        private IEnumerable<BattleAction> OnCardDrawn(CardEventArgs args)
        {
            foreach (var action in CheckTrigger(new Card[] { args.Card })) { yield return action; }
        }
        private IEnumerable<BattleAction> OnCardMovedToHand(CardMovingEventArgs args)
        {
            if (args.DestinationZone == CardZone.Hand)
                foreach (var action in CheckTrigger(new Card[] { args.Card })) { yield return action; }
        }



        private IEnumerable<BattleAction> CheckTrigger(Card[] cards)
        {
            if (_active && cards.Any((Card c) => c.CardType == CardType.Skill))
            {
                NotifyActivating();
                yield return new GainManaAction(Mana);
                yield return new DrawManyCardAction(Level);
                _active = false;
            }
        }


        private bool _active;
    }
}
