﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.StSimeonsCatacombs
{
    public class BreathStealerCardController : GhostCardController
    {
        #region Constructors

        public BreathStealerCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController, new string[] { "Aqueducts" })
        {
        }

        #endregion Constructors

        #region Methods
        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            //Play this card next to the hero with the lowest HP
            List<Card> foundTarget = new List<Card>();
            IEnumerator coroutine = base.GameController.FindTargetWithLowestHitPoints(1, (Card c) => c.IsHero && (overridePlayArea == null || c.IsAtLocationRecursive(overridePlayArea)), foundTarget, cardSource: base.GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            Card lowestHero = foundTarget.FirstOrDefault<Card>();
            if (lowestHero != null && storedResults != null)
            {
                //Play this card next to the hero with the lowest HP
                storedResults.Add(new MoveCardDestination(lowestHero.NextToLocation, false, false, false));
            }
            yield break;
        }

        public override IEnumerator Play()
        {
            if (base.Card.Location.IsNextToCard)
            {
                //That hero cannot regain HP.
                CannotGainHPStatusEffect cannotGainHPStatusEffect = new CannotGainHPStatusEffect();
                cannotGainHPStatusEffect.TargetCriteria.IsSpecificCard = base.GetCardThisCardIsNextTo();
                cannotGainHPStatusEffect.UntilTargetLeavesPlay(base.Card);
                IEnumerator coroutine = base.AddStatusEffect(cannotGainHPStatusEffect);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }
            yield break;
        }

        public override void AddTriggers()
        {
            //At the end of the environment turn, this card deals that hero 1 toxic damage
            IEnumerator dealDamage = base.DealDamage(base.Card, base.GetCardThisCardIsNextTo(), 1, DamageType.Toxic, cardSource: base.GetCardSource());
            base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => dealDamage, TriggerType.DealDamage);

            //add unaffected triggers from GhostCardControllers
            base.AddTriggers();
        }

        #endregion Methods
    }
}