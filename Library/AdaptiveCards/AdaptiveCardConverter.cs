// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdaptiveCards
{
    public class AdaptiveCardConverter : JsonConverter, ILogWarnings
    {
        public List<AdaptiveWarning> Warnings { get; set; } = new List<AdaptiveWarning>();

        // TODO #2749: temporary warning code for fallback card. Remove when common set of error codes created and integrated.
        private enum WarningStatusCode { UnsupportedSchemaVersion = 7, InvalidLanguage = 12 };

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            if (jObject.Value<string>("type") != AdaptiveCard.TypeName)
                throw new AdaptiveSerializationException($"Property 'type' must be '{AdaptiveCard.TypeName}'");

            if (reader.Depth == 0)
            {
                // Needed for ID collision detection after fallback was introduced
                ParseContext.Initialize();

                if (jObject.Value<string>("version") == null)
                {
                    throw new AdaptiveSerializationException("Could not parse required key: version. It was not found.");
                }

                // If this is the root AdaptiveCard and missing a version we fail parsing.
                // The depth checks that cards within a Action.ShowCard don't require the version
                if (jObject.Value<string>("version") == "")
                {
                    throw new AdaptiveSerializationException("Property is required but was found empty: version");
                }

                if (new AdaptiveSchemaVersion(jObject.Value<string>("version")) > AdaptiveCard.KnownSchemaVersion)
                {
                    return MakeFallbackTextCard(jObject);
                }
            }
            var typedElementConverter = serializer.ContractResolver.ResolveContract(typeof(AdaptiveTypedElement)).Converter;

            var card = (AdaptiveCard)typedElementConverter.ReadJson(jObject.CreateReader(), objectType, existingValue, serializer);
            card.Lang = ValidateLang(jObject.Value<string>("lang"));

            return card;
        }

        // Checks if lang is valid. Creates warning if not.
        private string ValidateLang(string val)
        {
            if (!string.IsNullOrEmpty(val))
            {
                try
                {
                    if (val.Length == 2 || val.Length == 3)
                    {
                        new CultureInfo(val);
                    }
                    else
                    {
                        Warnings.Add(new AdaptiveWarning((int)WarningStatusCode.InvalidLanguage, "Invalid language identifier: " + val));
                    }
                }
                catch (CultureNotFoundException)
                {
                    Warnings.Add(new AdaptiveWarning((int)WarningStatusCode.InvalidLanguage, "Invalid language identifier: " + val));
                }
            }
            return val;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(AdaptiveCard).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        private AdaptiveCard MakeFallbackTextCard(JObject jObject)
        {
            // Retrieve values defined by parsed json
            string fallbackText = jObject.Value<string>("fallbackText");
            string speak = jObject.Value<string>("speak");
            string language = jObject.Value<string>("lang");

            // Replace undefined values by default values
            if (string.IsNullOrEmpty(fallbackText))
            {
                fallbackText = "We're sorry, this card couldn't be displayed";
            }
            if (string.IsNullOrEmpty(speak))
            {
                speak = fallbackText;
            }
            if (string.IsNullOrEmpty(language))
            {
                language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            }

            // Define AdaptiveCard to return
            AdaptiveCard fallbackCard = new AdaptiveCard("1.0")
            {
                Speak = speak,
                Lang = language
            };
            fallbackCard.Body.Add(new AdaptiveTextBlock
            {
                Text = fallbackText
            });

            // Add relevant warning
            Warnings.Add(new AdaptiveWarning((int) WarningStatusCode.UnsupportedSchemaVersion, "Schema version is not supported"));

            return fallbackCard;
        }
    }
}
