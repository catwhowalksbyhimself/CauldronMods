﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cauldron.Dendron;
using Cauldron.Vector;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;

using NUnit.Framework;

namespace CauldronTests
{
    [TestFixture]
    public class VectorTests : BaseTest
    {
        protected TurnTakerController Vector => FindVillain("Vector");

        private const string DeckNamespace = "Cauldron.Vector";

        [Test]
        public void TestVectorDeckList()
        {
            SetupGameController(DeckNamespace, "Legacy", "Ra", "Haka", "Megalopolis");

            Card anticoagulant = GetCard(AnticoagulantCardController.Identifier);
            Assert.IsTrue(anticoagulant.DoKeywordsContain(new []{ "virus", "ongoing" }));

            Card assassinsSignature = GetCard(AssassinsSignatureCardController.Identifier);
            Assert.IsTrue(assassinsSignature.DoKeywordsContain("one-shot"));

            Card bioterrorSquad = GetCard(BioterrorSquadCardController.Identifier);
            Assert.IsTrue(bioterrorSquad.DoKeywordsContain(new []{ "pawn", "virus" }));

            Card bloodSample = GetCard(BloodSampleCardController.Identifier);
            Assert.IsTrue(bloodSample.DoKeywordsContain(new []{ "ongoing", "virus" }));

            Card delayedSymptoms = GetCard(DelayedSymptomsCardController.Identifier);
            Assert.IsTrue(delayedSymptoms.DoKeywordsContain(new [] { "one-shot", "virus" }));

            Card eliteTraining = GetCard(EliteTrainingCardController.Identifier);
            Assert.IsTrue(eliteTraining.DoKeywordsContain("ongoing"));

            Card hostageShield = GetCard(HostageShieldCardController.Identifier);
            Assert.IsTrue(hostageShield.DoKeywordsContain("ongoing"));

            Card hotZone = GetCard(HotZoneCardController.Identifier);
            Assert.IsTrue(hotZone.DoKeywordsContain("one-shot"));

            Card hyperactiveImmuneSystem = GetCard(HyperactiveImmuneSystemCardController.Identifier);
            Assert.IsTrue(hyperactiveImmuneSystem.DoKeywordsContain("ongoing"));

            Card lethalForce = GetCard(LethalForceCardController.Identifier);
            Assert.IsTrue(lethalForce.DoKeywordsContain("one-shot"));

            Card quarantineProtocols = GetCard(QuarantineProtocolsCardController.Identifier);
            Assert.IsTrue(quarantineProtocols.DoKeywordsContain("device"));

            Card supervirus = GetCard(SupervirusCardController.Identifier);
            Assert.IsTrue(supervirus.DoKeywordsContain(new [] { "ongoing", "virus" }));

            Card undiagnosedSubject = GetCard(UndiagnosedSubjectCardController.Identifier);
            Assert.IsTrue(undiagnosedSubject.DoKeywordsContain("pawn"));

            Card vendetta = GetCard(VendettaCardController.Identifier);
            Assert.IsTrue(vendetta.DoKeywordsContain("one-shot"));

            Card virulentBlade = GetCard(VirulentBladeCardController.Identifier);
            Assert.IsTrue(virulentBlade.DoKeywordsContain(new[] { "device", "virus" }));

            Card vrRazortail = GetCard(VrRazortailCardController.Identifier);
            Assert.IsTrue(vrRazortail.DoKeywordsContain("vehicle"));
        }

        [Test]
        public void TestVectorLoads()
        {
            SetupGameController(DeckNamespace, "Legacy", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(Vector);
            Assert.IsInstanceOf(typeof(VectorCharacterCardController), Vector.CharacterCardController);

            Assert.AreEqual(55, Vector.CharacterCard.HitPoints);
        }

        [Test]
        public void TestAnticoagulant()
        {
            // Arrange
            SetupGameController(DeckNamespace, "Legacy", "Ra", "Haka", "Megalopolis");
            StartGame();

            Card antiC = GetCard(AnticoagulantCardController.Identifier);

            PlayCard(antiC);

            // Act
            GoToPlayCardPhase(legacy);
            DealDamage(legacy, Vector, 5, DamageType.Melee);


            // Assert
            Assert.True(false, "TODO");
        }

        [Test]
        public void TestAssassinsSignature()
        {
            // Arrange
            SetupGameController(DeckNamespace, "Legacy", "Ra", "Haka", "Megalopolis");
            StartGame();

            Card mere = GetCard("Mere");
            Card dominion = GetCard("Dominion");
            Card savageMana = GetCard("SavageMana");

            PutIntoPlay(mere.Identifier);
            PutIntoPlay(dominion.Identifier);
            PutIntoPlay(savageMana.Identifier);

            DecisionSelectCard = dominion;
            QuickHPStorage(haka);

            // Act
            Card aSig = GetCard(AssassinsSignatureCardController.Identifier);
            PlayCard(aSig);


            // Assert
            QuickHPCheck(-3);
            AssertInPlayArea(haka, new []{ mere, savageMana});
            AssertInTrash(haka, dominion);
        }

        [Test]
        public void TestBioTerrorSquad()
        {
            // Arrange
            SetupGameController(DeckNamespace, "Legacy", "Ra", "Haka", "Megalopolis");
            
            Card bloodSample = GetCard(BloodSampleCardController.Identifier); // Virus
            Card delayedSymptoms = GetCard(DelayedSymptomsCardController.Identifier); // Virus
            Card eliteTraining = GetCard(EliteTrainingCardController.Identifier);

            Card bioTerror = GetCard(BioterrorSquadCardController.Identifier);

            PutInTrash(bloodSample);
            PutInTrash(delayedSymptoms);
            PutInTrash(eliteTraining);

            StartGame();
            DecisionSelectCard = GetCard(DelayedSymptomsCardController.Identifier);

            GoToPlayCardPhase(Vector);
            QuickHPStorage(bioTerror, haka.CharacterCard);
            PlayCard(bioTerror);

            QuickShuffleStorage(Vector);

            // Act
            GoToStartOfTurn(Vector);

            // Assert
            QuickShuffleCheck(1);
            QuickHPCheck(-1, -3);
            AssertNotInTrash(delayedSymptoms);
        }
    }
}