﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Malichae
{
    public class GrandEzaelCardController : DjinnOngoingController
    {
        public GrandEzaelCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController, "HighEzael", "Ezael")
        {
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            AddDestroyAtEndOfTurnTrigger();
        }

        public override Power GetGrantedPower(CardController cardController)
        {
            return new Power(cardController.HeroTurnTakerController, cardController, "All hero targets regain 3HP.", UseGrantedPower(), 0, null, GetCardSource());
        }

        private IEnumerator UseGrantedPower()
        {
            int hp = GetPowerNumeral(0, 3);

            var card = GetCardThisCardIsNextTo();
            var coroutine = base.GameController.GainHP(DecisionMaker, c => c.IsTarget && c.IsHero && c.IsInPlayAndHasGameText, hp, cardSource: GetCardSource());
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
