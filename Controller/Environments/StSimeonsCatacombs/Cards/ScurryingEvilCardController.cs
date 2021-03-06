﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.StSimeonsCatacombs
{
    public class ScurryingEvilCardController : StSimeonsBaseCardController
    {
        public static readonly string Identifier = "ScurryingEvil";

        public ScurryingEvilCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            base.AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            base.SpecialStringMaker.ShowSpecialString(() => $"{Card.Title} has not been dealt damage since it has entered play.", base.IsFirstOrOnlyCopyOfThisCardInPlay, null).Condition = (() => !this.HasBeenDealtDamageSinceEnteringPlay());
            base.SpecialStringMaker.ShowSpecialString(() => $"{Card.Title} is immune to damage as long as {this.FindCorrelatedRoom().Title} is in play.", base.IsFirstOrOnlyCopyOfThisCardInPlay, () => new Card[]
            {
                base.Card
            }).Condition = (() => this.IsImmuneToDamage());
        }

        public override void AddTriggers()
        {
            //At the end of the environment turn, play the top card of the environment deck.
            base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, new Func<PhaseChangeAction, IEnumerator>(base.PlayTheTopCardOfTheEnvironmentDeckWithMessageResponse), TriggerType.PlayCard);

            //Whenever this card is dealt damage, it becomes immune to damage until a different Room card enters play.

            //on room change trigger message to player
            base.AddTrigger<PlayCardAction>((PlayCardAction pca) => pca.CardToPlay.IsRoom && this.IsNewRoom(pca.CardToPlay), new Func<PlayCardAction, IEnumerator>(this.RoomChangeResponse), TriggerType.Other, TriggerTiming.After);

            //immune to damage trigger
            base.AddImmuneToDamageTrigger((DealDamageAction dd) => dd.Target == base.Card && this.IsImmuneToDamage());

        }

        private IEnumerator RoomChangeResponse(PlayCardAction pca)
        {
            IEnumerator coroutine = base.GameController.SendMessageAction($"{Card.Title} is no longer immune to damage", Priority.High, base.GetCardSource());
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

        public override bool AskIfCardIsIndestructible(Card card)
        {
            //This card is indestructible until it has 0 or fewer HP.
            return card == base.Card && base.Card.HitPoints > 0;
        }

        private bool IsImmuneToDamage()
        {
            //checks the journal for the last time Scurrying Evil was dealt damage
            DealDamageJournalEntry dealDamageJournalEntry = base.GameController.Game.Journal.MostRecentDealDamageEntry((DealDamageJournalEntry e) => e.TargetCard == base.Card && e.Amount > 0);
            int? damageEntryIndex;
            if (dealDamageJournalEntry != null)
            {
                damageEntryIndex = base.GameController.Game.Journal.GetEntryIndex(dealDamageJournalEntry);
            }
            else
            {
                return false;
            }

            //check the journal for the last room that was played before that damage
            IEnumerable<CardEntersPlayJournalEntry> roomEntries = from e in base.GameController.Game.Journal.CardEntersPlayEntries()
                                                                  where this.IsDefinitionRoom(e.Card) && base.GameController.Game.Journal.GetEntryIndex(e) < damageEntryIndex
                                                                  select e;
            int? latestCardEntersPlayIndex = new int?(0);
            int? roomEntryIndex;
            Dictionary<int?, CardEntersPlayJournalEntry> cardEntryDict = new Dictionary<int?, CardEntersPlayJournalEntry>();
            foreach (CardEntersPlayJournalEntry roomEntry in roomEntries)
            {
                roomEntryIndex = base.GameController.Game.Journal.GetEntryIndex(roomEntry);
                if (roomEntryIndex > latestCardEntersPlayIndex)
                {
                    latestCardEntersPlayIndex = roomEntryIndex;
                    cardEntryDict.Add(roomEntryIndex, roomEntry);
                }
            }

            CardEntersPlayJournalEntry latestRoomBeforeDamageJournalEntry = cardEntryDict[latestCardEntersPlayIndex];

            //check the journal for if there has been a room played since that point

            IEnumerable<CardEntersPlayJournalEntry> newRoomEntries = from e in base.GameController.Game.Journal.CardEntersPlayEntries()
                                                                     where this.IsDefinitionRoom(e.Card) && base.GameController.Game.Journal.GetEntryIndex(e) > damageEntryIndex
                                                                     select e;
            foreach (CardEntersPlayJournalEntry roomEntry in newRoomEntries)
            {
                //if there has been a room since then, check if its the same room as before
                if (roomEntry.Card != latestRoomBeforeDamageJournalEntry.Card)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsNewRoom(Card newRoom)
        {

            //check the journal for the last room that was played before this one
            IEnumerable<CardEntersPlayJournalEntry> roomEntries = from e in base.GameController.Game.Journal.CardEntersPlayEntries()
                                                                  where this.IsDefinitionRoom(e.Card)
                                                                  select e;
            int numEntries = roomEntries.Count();
            if (numEntries > 0)
            {
                Card previousRoom = roomEntries.ElementAt(numEntries - 1).Card;
                if (previousRoom != newRoom)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasBeenDealtDamageSinceEnteringPlay()
        {
            PlayCardJournalEntry playCardJournalEntry = base.GameController.Game.Journal.QueryJournalEntries<PlayCardJournalEntry>((PlayCardJournalEntry e) => e.CardPlayed == base.Card).LastOrDefault<PlayCardJournalEntry>();
            int? playCardIndex = base.GameController.Game.Journal.GetEntryIndex(playCardJournalEntry);
            IEnumerable<DealDamageJournalEntry> damageEntries = from e in base.GameController.Game.Journal.DealDamageEntries()
                                                                where e.TargetCard == base.Card && base.GameController.Game.Journal.GetEntryIndex(e) > playCardIndex
                                                                select e;
            if (damageEntries.Count() > 0)
            {
                return true;
            }

            return false;
        }

        private Card FindCorrelatedRoom()
        {
            IEnumerable<CardEntersPlayJournalEntry> roomEntries = from e in base.GameController.Game.Journal.CardEntersPlayEntries()
                                                                  where this.IsDefinitionRoom(e.Card)
                                                                  select e;
            int numEntries = roomEntries.Count();
            if (numEntries > 0)
            {
                Card currentRoom = roomEntries.ElementAt(numEntries - 1).Card;
                return currentRoom;
            }

            return null;
        }
    }
}