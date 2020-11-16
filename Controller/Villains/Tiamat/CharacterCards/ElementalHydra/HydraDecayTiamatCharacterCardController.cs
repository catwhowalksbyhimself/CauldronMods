﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;

namespace Cauldron.Tiamat
{
    public class HydraDecayTiamatCharacterCardController : HydraTiamatCharacterCardController
    {
        public HydraDecayTiamatCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }

        protected override ITrigger[] AddFrontTriggers()
        {
            return new ITrigger[]
            { 
				//{Tiamat}, The Breath of Decay is immune to Toxic damage.
				base.AddImmuneToDamageTrigger((DealDamageAction dealDamage) => dealDamage.Target == base.Card && dealDamage.DamageType == DamageType.Toxic, false),
                //Increase damage dealt to hero targets by 1.
                base.AddIncreaseDamageTrigger((DealDamageAction action) => action.Target.IsHero, 1)
            };
        }
    }
}