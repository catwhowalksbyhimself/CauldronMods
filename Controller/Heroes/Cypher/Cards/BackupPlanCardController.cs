﻿using System.Collections;
using System.Collections.Generic;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.Cypher
{
    public class BackupPlanCardController : CypherBaseCardController
    {
        //==============================================================
        // When a non-hero card enters play, you may destroy this card.
        // If you do, select any number of Augments in play and move
        // each one next to a new hero. Then, each augmented hero regains 2HP.
        //==============================================================

        public static string Identifier = "BackupPlan";

        private const int HpGain = 2;


        public BackupPlanCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {
            // When a non-hero card enters play, you may destroy this card.
            base.AddTrigger<CardEntersPlayAction>(p => !p.CardEnteringPlay.IsHero && GameController.IsCardVisibleToCardSource(p.CardEnteringPlay, GetCardSource()),
                DestroySelfResponse, TriggerType.DestroySelf, TriggerTiming.After);

            base.AddTriggers();
        }

        private IEnumerator DestroySelfResponse(CardEntersPlayAction cepa)
        {
            // Ask player if they want to destroy this card
            List<YesNoCardDecision> storedResults = new List<YesNoCardDecision>();
            IEnumerator routine = base.GameController.MakeYesNoCardDecision(base.HeroTurnTakerController,
                SelectionType.DestroySelf, base.Card, storedResults: storedResults, cardSource: base.GetCardSource());

            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(routine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(routine);
            }

            if (!base.DidPlayerAnswerYes(storedResults))
            {
                yield break;
            }

            if (!base.GameController.IsCardIndestructible(base.Card))
            {
                // If you do, select any number of Augments in play and move
                // each one next to a new hero. Then, each augmented hero regains 2HP.

                
                SelectCardDecision scd = new SelectCardDecision(GameController, DecisionMaker,
                    SelectionType.MoveCardNextToCard, GetAugmentsInPlay(), isOptional: true, cardSource: GetCardSource());

                IEnumerable<Function> FunctionsBasedOnCard(Card c) => new[]
                {
                    new Function(this.HeroTurnTakerController, $"Move {c.Title}", SelectionType.MoveCardNextToCard, () => MoveAugment(c) )
                };

                routine = base.GameController.SelectCardsAndPerformFunction(this.HeroTurnTakerController,
                    new LinqCardCriteria(c => GetAugmentsInPlay().Contains(c)), FunctionsBasedOnCard, true, GetCardSource());

                

                /*
                List<SelectCardsDecision> scds = new List<SelectCardsDecision>();
                 routine = base.GameController.SelectCardsAndStoreResults(base.HeroTurnTakerController,
                    SelectionType.MoveCard, c => GetAugmentsInPlay().Contains(c), 5, scds, false,
                    cardSource: GetCardSource());
                */

                
                //List<MoveCardDestination> list = new List<MoveCardDestination>();
                //list.Add(new MoveCardDestination(base.HeroTurnTaker.Hand));
                //routine = base.GameController.SelectCardsFromLocationAndMoveThem(base.HeroTurnTakerController,
                    //base.TurnTaker., 0, 5, new LinqCardCriteria(IsAugment), list);

                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(routine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(routine);
                }


                // Then, each augmented hero regains 2HP.
                routine = this.GameController.GainHP(this.HeroTurnTakerController, IsAugmented, HpGain,
                    cardSource: GetCardSource());

                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(routine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(routine);
                }


                // Destroy card
                List<DestroyCardAction> dcas = new List<DestroyCardAction>();
                routine = this.GameController.DestroyCard(this.DecisionMaker, this.Card, storedResults: dcas, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(routine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(routine);
                }
            }
            else
            {
                routine = base.GameController.SendMessageAction(base.Card.Title + " is indestructible, so it cannot be destroyed for an extra actions.", 
                    Priority.Medium, base.GetCardSource(), null, true);

                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(routine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(routine);
                }
            }
        }
    }
}