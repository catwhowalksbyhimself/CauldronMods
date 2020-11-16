﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;

namespace Cauldron.Tiamat
{
    public class HydraFrigidEarthTiamatInstructionsCardController : HydraTiamatInstructionsCardController
    {
        public HydraFrigidEarthTiamatInstructionsCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            this.firstHead = base.GameController.FindCardController("HydraWinterTiamatCharacter");
            this.secondHead = base.GameController.FindCardController("HydraEarthTiamatCharacter");
            this.element = "ElementOfIce";
            //Whenever Element of Ice enters play and {WinterTiamatCharacter} is decapitated, if {EarthTiamatCharacter} is active she deals the hero target with the highest HP X melee damage, where X = {H} plus the number of Sky Breaker cards in the villain trash.
            this.alternateElementCoroutine = base.DealDamageToHighestHP(this.secondHead.Card, 1, (Card c) => c.IsHero, (Card c) => this.PlusNumberOfACardInTrash(Game.H, "SkyBreaker"), DamageType.Melee);
        }

        protected override ITrigger[] AddFrontTriggers()
        {
            return new ITrigger[]
            {
                //At the end of the villain turn, 1 player must discard 1 card.
                base.AddEndOfTurnTrigger((TurnTaker turnTaker) => turnTaker == base.TurnTaker, this.DiscardResponse, TriggerType.DiscardCard)
            };
        }

        protected override ITrigger[] AddFrontAdvancedTriggers()
        {
            return new ITrigger[]
            {
                //At the end of the villain turn, 1 player must discard 1 card.
                base.AddEndOfTurnTrigger((TurnTaker turnTaker) => turnTaker == base.TurnTaker, this.DiscardResponse, TriggerType.DiscardCard)
            };
        }

        protected override ITrigger[] AddBackTriggers()
        {
            return new ITrigger[]
            {
                //At the end of the villain turn, if {WinterTiamatCharacter} is active, she deals the hero target with the lowest HP 1 cold damage.
                base.AddEndOfTurnTrigger((TurnTaker turnTaker) => turnTaker == base.TurnTaker, this.DealDamageResponse, TriggerType.DealDamage, (PhaseChangeAction action) => !firstHead.Card.IsFlipped)
            };
        }

        private IEnumerator DiscardResponse(PhaseChangeAction action)
        {
            //At the end of the villain turn, 1 player must discard 1 card.
            IEnumerator coroutine = base.SelectAndDiscardCards(this.DecisionMaker, 1);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            yield break;
        }

        private IEnumerator DealDamageResponse(PhaseChangeAction action)
        {
            //...if {WinterTiamatCharacter} is active, she deals the hero target with the lowest HP 1 cold damage.
            IEnumerator coroutine = base.DealDamageToLowestHP(this.firstHead.Card, 1, (Card c) => c.IsHero, (Card c) => new int?(1), DamageType.Cold);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            yield break;
        }
    }
}