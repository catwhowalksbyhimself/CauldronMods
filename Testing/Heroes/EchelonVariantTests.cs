﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cauldron.Echelon;
using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;

namespace CauldronTests
{
    [TestFixture]
    public class EchelonVariantTests : BaseTest
    {
        #region echelonhelperfunctions
        protected HeroTurnTakerController echelon => FindHero("Echelon");

        private const string DeckNamespace = "Cauldron.Echelon";

        private readonly DamageType DTM = DamageType.Melee;
        private Card MDP => GetCardInPlay("MobileDefensePlatform");

        private void AssertHasKeyword(string keyword, IEnumerable<string> identifiers)
        {
            foreach (var id in identifiers)
            {
                AssertCardHasKeyword(GetCard(id), keyword, false);
            }
        }
        private void AddImmuneToNextDamageEffect(TurnTakerController ttc, bool villains, bool heroes)
        {
            ImmuneToDamageStatusEffect effect = new ImmuneToDamageStatusEffect();
            effect.TargetCriteria.IsVillain = villains;
            effect.TargetCriteria.IsHero = heroes;
            effect.NumberOfUses = 1;
            RunCoroutine(GameController.AddStatusEffect(effect, true, ttc.CharacterCardController.GetCardSource()));
        }

        #endregion

        [Test]
        public void TestFirstResponseEchelonLoads()
        {
            // Arrange & Act
            SetupGameController("BaronBlade", "Cauldron.Echelon/FirstResponseEchelonCharacter", "Megalopolis");

            // Assert
            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());
            Assert.IsNotNull(echelon);
            Assert.IsInstanceOf(typeof(FirstResponseEchelonCharacterCardController), echelon.CharacterCardController);

            Assert.AreEqual(28, echelon.CharacterCard.HitPoints);
        }
        [Test]
        public void TestFirstResponsePower()
        {
            SetupGameController("BaronBlade", "Cauldron.Echelon/FirstResponseEchelonCharacter", "Ra", "Tachyon", "Megalopolis");
            StartGame();

            Card kestrel = PutInHand("TheKestrelMarkII");
            DecisionSelectCards = new Card[] { null, kestrel };

            QuickHandStorage(echelon);

            //draw a card, Kestral play is optional
            UsePower(echelon);
            AssertInHand(kestrel);
            QuickHandCheck(1);

            //may play kestrel
            UsePower(echelon);
            AssertIsInPlay(kestrel);
            QuickHandCheck(0);
        }
        [Test]
        public void TestFirstResponseIncap1()
        {
            SetupGameController("BaronBlade", "Cauldron.Echelon/FirstResponseEchelonCharacter", "Ra", "Tachyon", "TheWraith", "Megalopolis");
            StartGame();
            DestroyCard(MDP);
            DealDamage(baron, echelon, 50, DTM);

            Card staff = PutInHand("TheStaffOfRa");
            Card hud = PutInHand("HUDGoggles");
            Card belt = PutInHand("UtilityBelt");
            Card stun = PutInHand("StunBolt");
            DecisionSelectCards = new Card[] { staff, hud, belt, null, null };

            //play 2 equipment
            UseIncapacitatedAbility(echelon, 0);
            AssertIsInPlay(staff, hud);
            AssertInHand(belt, stun);

            //play only 1
            UseIncapacitatedAbility(echelon, 0);
            AssertIsInPlay(belt);
            AssertInHand(stun);

            //play 0
            UseIncapacitatedAbility(echelon, 0);
            AssertInHand(stun);
        }
        [Test]
        public void TestFirstResponseIncap2()
        {
            SetupGameController("BaronBlade", "Cauldron.Echelon/FirstResponseEchelonCharacter", "Ra", "Tachyon", "Megalopolis");
            StartGame();
            DestroyCard(MDP);
            DealDamage(baron, echelon, 50, DTM);

            Card flesh = PutOnDeck("FleshOfTheSunGod");
            Card blast = PutOnDeck("FireBlast");
            Card barrier = PutOnDeck("FlameBarrier");

            DecisionSelectCards = new Card[] { flesh, blast };

            //starts, from top down, barrier-blast-flesh
            //should end up flesh-blast-barrier
            AssertNextDecisionChoices(included: new TurnTaker[] { ra.TurnTaker, tachyon.TurnTaker }, notIncluded: new TurnTaker[] { echelon.TurnTaker, baron.TurnTaker, FindEnvironment().TurnTaker });
            UseIncapacitatedAbility(echelon, 1);

            AssertOnTopOfDeck(flesh);
            DrawCard(ra, 2);

            AssertInHand(blast);
            AssertOnTopOfDeck(barrier);
        }
        [Test]
        public void TestFirstResponseIncap3()
        {
            SetupGameController("BaronBlade", "Cauldron.Echelon/FirstResponseEchelonCharacter", "Ra", "Tachyon", "Megalopolis");
            StartGame();
            DealDamage(baron, echelon, 50, DTM);

            Card police = PutIntoPlay("PoliceBackup");
            Card traffic = PutIntoPlay("TrafficPileup");
            Card lash = PutIntoPlay("BacklashField");

            AssertNextDecisionChoices(new Card[] { traffic, police }, new Card[] { lash, MDP });
            UseIncapacitatedAbility(echelon, 2);
            AssertInTrash(police);
        }
        [Test]
        public void TestFutureEchelonLoads()
        {
            // Arrange & Act
            SetupGameController("BaronBlade", "Cauldron.Echelon/FutureEchelonCharacter", "Megalopolis");

            // Assert
            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());
            Assert.IsNotNull(echelon);
            Assert.IsInstanceOf(typeof(FutureEchelonCharacterCardController), echelon.CharacterCardController);

            Assert.AreEqual(26, echelon.CharacterCard.HitPoints);
        }
    }
}