﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Cauldron.MagnificentMara;

namespace CauldronTests
{
    [TestFixture()]
    public class MagnificentMaraVariantTests : BaseTest
    {
        #region MaraHelperFunctions
        protected HeroTurnTakerController mara { get { return FindHero("MagnificentMara"); } }
        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(mara.CharacterCard, 1);
            DealDamage(villain, mara, 2, DamageType.Melee);
        }

        protected DamageType DTM => DamageType.Melee;

        protected Card MDP { get { return FindCardInPlay("MobileDefensePlatform"); } }
        #endregion maraHelperFunctions

        [Test]
        public void TestPastMaraLoads()
        {
            SetupGameController("BaronBlade", "Cauldron.MagnificentMara/PastMagnificentMaraCharacter", "Legacy", "Bunker", "TheScholar", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(mara);
            Assert.IsInstanceOf(typeof(PastMagnificentMaraCharacterCardController), mara.CharacterCardController);

            Assert.AreEqual(24, mara.CharacterCard.HitPoints);
        }
        
        [Test]
        public void TestMOSSMaraLoads()
        {
            SetupGameController("BaronBlade", "Cauldron.MagnificentMara/MinistryOfStrategicScienceMagnificentMaraCharacter", "Legacy", "Bunker", "TheScholar", "Megalopolis");

            Assert.AreEqual(6, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(mara);
            Assert.IsInstanceOf(typeof(MinistryOfStrategicScienceMagnificentMaraCharacterCardController), mara.CharacterCardController);

            Assert.AreEqual(25, mara.CharacterCard.HitPoints);
        }
        [Test]
        public void TestMOSSMaraPower()
        {
            SetupGameController("BaronBlade", "Cauldron.MagnificentMara/MinistryOfStrategicScienceMagnificentMaraCharacter", "Legacy", "Bunker", "TheScholar", "Megalopolis");
            StartGame();

            DecisionSelectTargets = new Card[] { bunker.CharacterCard, baron.CharacterCard };

            QuickHPStorage(baron.CharacterCard, bunker.CharacterCard, MDP);

            DealDamage(bunker, MDP, 1, DTM);
            QuickHPCheck(0, 0, -1);

            UsePower(mara);
            DealDamage(bunker, MDP, 1, DTM);
            //bunker takes 2 from power, MDP takes 1 + 1 boost
            QuickHPCheck(0, -2, -2);

            UsePower(mara);
            DealDamage(baron, bunker, 1, DTM);
            //should not require damage to go through for the boost to happen
            QuickHPCheck(0, -2, 0);

            //wears off on Mara's turn
            GoToStartOfTurn(mara);
            DealDamage(bunker, MDP, 1, DTM);
            DealDamage(baron, bunker, 1, DTM);
            QuickHPCheck(0, -1, -1);
        }
    }
}
