﻿using NUnit.Framework;
using System;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Linq;
using System.Collections;
using Handelabra.Sentinels.UnitTest;
using System.Reflection;
using Cauldron.Anathema;
using Handelabra;
using System.Collections.Generic;
using Boomlagoon.JSON;
using System.IO;

namespace CauldronTests.Art
{
    abstract class ArtSourceBase : IEnumerable
    {
        //get the list of decks, and promo's for each, cards for each
        private readonly string _kind;
        protected ArtSourceBase(string kind)
        {
            _kind = kind;
        }

        public IEnumerator GetEnumerator()
        {
            Assembly assembly = typeof(Cauldron.Necro.NecroCharacterCardController).Assembly;

            List<object[]> results = new List<object[]>();

            var ress = assembly.GetManifestResourceNames();
            for (int index = 0; index < ress.Length; index++)
            {
                string res = ress[index];
                var stream = assembly.GetManifestResourceStream(res);

                JSONObject jsonObject;
                using (var sr = new System.IO.StreamReader(stream))
                {
                    string text = sr.ReadToEnd();
                    jsonObject = JSONObject.Parse(text);
                }

                string name = Path.GetFileNameWithoutExtension(res.Replace("Cauldron.DeckLists.", "").Replace("DeckList", ""));
                var kind = jsonObject.GetString("kind");
                if (_kind != null && kind != _kind)
                    continue;

                string initialCard = "<>";
                if (kind != "Environment")
                {
                    initialCard = jsonObject.GetArray("initialCardIdentifiers").First().Str;
                }

                var cards = jsonObject.GetArray("cards");

                List<string> cardIdentifiers = new List<string>();
                List<string> characterIdentifiers = new List<string>();
                List<string> startEndIdentifiers = new List<string>();

                foreach (JSONValue card in cards)
                {
                    var cardIdentifier = card.Obj.GetString("identifier");
                    bool isCharacter = card.Obj.GetBoolean("character");

                    cardIdentifiers.Add(cardIdentifier);

                    if (isCharacter)
                        characterIdentifiers.Add(cardIdentifier);

                    if (cardIdentifier == initialCard)
                        startEndIdentifiers.Add(cardIdentifier);
                }

                List<JSONValue> promos = new List<JSONValue>();
                if (jsonObject.ContainsKey("promoCards"))
                {
                    promos.AddRange(jsonObject.GetArray("promoCards"));
                }
                if (jsonObject.ContainsKey("notPromoCards"))
                {
                    promos.AddRange(jsonObject.GetArray("notPromoCards"));
                }

                foreach (JSONValue card in promos)
                {
                    var cardIdentifier = card.Obj.GetString("identifier");
                    var promoIdentifier = card.Obj.GetString("promoIdentifier");
                    bool isCharacter = card.Obj.GetBoolean("character");

                    cardIdentifiers.Add(promoIdentifier);

                    if (isCharacter)
                        characterIdentifiers.Add(promoIdentifier);

                    if (cardIdentifier == initialCard)
                        startEndIdentifiers.Add(promoIdentifier);
                }

                results.Add(new object[] { name, kind, cardIdentifiers, characterIdentifiers, startEndIdentifiers });

            }

            return results.GetEnumerator();
        }
    }

    public abstract class ArtTestBase
    {
        #region Data

        public static bool IsMicroArtRequired = false;

        protected readonly string ArtPath;

        protected readonly string _name;
        protected readonly string _kind;
        protected readonly List<string> _cardIdentifiers;
        protected readonly List<string> _characterIdentifiers;
        protected readonly List<string> _startEndIdentifiers;
        private int _numWarnings;


        protected ArtTestBase(string name, string kind, List<string> cardIdentifiers, List<string> characterIdentifiers, List<string> startEndIdentifiers)
        {
            ArtPath = Path.Combine(ProjectPath.Path, @"Art\");

            _name = name;
            _kind = kind;
            _cardIdentifiers = cardIdentifiers;
            _characterIdentifiers = characterIdentifiers;
            _startEndIdentifiers = startEndIdentifiers;
        }

        #endregion Data


        public static HashSet<string> ReadAtlasJson(string directoryPath, string atlasName)
        {
            var files = Directory.GetFiles(directoryPath, atlasName + "*.json").Select(s => Path.GetFileName(s)).ToArray();
            if (files.Length == 0)
                return null;
            HashSet<string> results = new HashSet<string>();
            foreach(var file in files)
            {
                JSONObject jsonObject;
                using (var sr = new System.IO.StreamReader(Path.Combine(directoryPath, file)))
                {
                    string text = sr.ReadToEnd();
                    jsonObject = JSONObject.Parse(text);
                }

                var frames = jsonObject.GetValue("frames");
                foreach(var thing in frames.Obj)
                {
                    if (!results.Add(Path.GetFileNameWithoutExtension(thing.Key)))
                    {
                        Assert.Warn($"In file: {file}, {thing.Key} was already present");
                    }
                }
            }

            return results;
        }

        [NUnit.Framework.SetUp]
        public void ResetWarnings()
        {
            _numWarnings = 0;
        }


        protected void Warn(string message)
        {
            Assert.Warn(message);
            _numWarnings++;
        }

        protected void AssertNoWarnings()
        {
            Assert.AreEqual(0, _numWarnings, $"There were {_numWarnings} warnings.");
        }

    }
}