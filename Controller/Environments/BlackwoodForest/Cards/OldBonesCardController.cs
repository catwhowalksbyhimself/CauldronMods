﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;


namespace Cauldron.BlackwoodForest
{
    public class OldBonesCardController : CardController
    {
        //==============================================================
        // When this card enters play, shuffle each trash pile other than the environment
        // and reveal a card from it. Put the revealed cards on top of each deck.
        // At the start of the environment turn destroy this card.
        //==============================================================

        public static readonly string Identifier = "OldBones";

        private const int CardsToMove = 1;

        public OldBonesCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            // Destroy self at start of next env. turn
            base.AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker,
                base.DestroyThisCardResponse,
                TriggerType.DestroySelf);

            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // Shuffle each trash pile other than environment
            var query = GameController.FindLocationsWhere(loc => !loc.IsEnvironment && loc.IsTrash && GameController.IsLocationVisibleToSource(loc, GetCardSource()));
            foreach (var loc in query)
            {
                IEnumerator shuffleRoutine = ShuffleTrashResponse(loc);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(shuffleRoutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(shuffleRoutine);
                }
            }
        }

        private IEnumerator ShuffleTrashResponse(Location trash)
        {
            // Shuffle trash pile
            IEnumerator shuffleTrashRoutine = base.GameController.ShuffleLocation(trash);

            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(shuffleTrashRoutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(shuffleTrashRoutine);
            }

            List<Card> revealedCards = new List<Card>();
            IEnumerator revealCardsRoutine = base.GameController.RevealCards(this.TurnTakerController, trash, CardsToMove, revealedCards,
                revealedCardDisplay: RevealedCardDisplay.ShowRevealedCards, cardSource: this.GetCardSource());

            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCardsRoutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCardsRoutine);
            }

            if (!revealedCards.Any())
            {
                yield break;
            }

            Card revealedCard = revealedCards.First();

            // Top deck the card 
            IEnumerator moveCardsRoutine = base.GameController.MoveCard(this.TurnTakerController, revealedCard, revealedCard.NativeDeck,
                cardSource: base.GetCardSource());

            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCardsRoutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCardsRoutine);
            }
        }
    }
}