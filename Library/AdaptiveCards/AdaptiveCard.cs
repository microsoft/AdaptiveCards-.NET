﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AdaptiveCards
{
    /// <summary>
    ///     Adaptive card which has flexible container
    /// </summary>
    public class AdaptiveCard : AdaptiveTypedElement
#if WINDOWS_UWP
      // TODO: uncomment when I figure out the 
       //   , Windows.UI.Shell.IAdaptiveCard
#endif
    {
        public const string TypeName = "AdaptiveCard";

        public AdaptiveCard()
        {
            Type = TypeName;
            Version = new AdaptiveSchemaVersion(1, 0);
        }

        public static AdaptiveCardParseResult FromJson(string json)
        {
            AdaptiveCard card = null;

            try
            {
                card = JsonConvert.DeserializeObject<AdaptiveCard>(json);

                // Version must be specified
                if (card.Version == null)
                {
                    card = null;
                }
            }

            catch
            {
                // TODO: Return errors here
            }

            return new AdaptiveCardParseResult(card);
        }

        public const string ContentType = "application/vnd.microsoft.card.adaptive";

        [JsonProperty(Order = -3)]
        public List<AdaptiveElement> Body { get; set; } = new List<AdaptiveElement>();

        /// <summary>
        ///     Actions for this container
        /// </summary>
        [JsonProperty(Order = -2, NullValueHandling = NullValueHandling.Ignore)]
        public List<AdaptiveActionBase> Actions { get; set; } = new List<AdaptiveActionBase>();

        /// <summary>
        ///     Speak annotation for the card
        /// </summary>
        [JsonProperty(Order = -6, NullValueHandling = NullValueHandling.Ignore)]
        public string Speak { get; set; }

        /// <summary>
        ///     Title for the card (used when displayed in a dialog)
        /// </summary>
        [JsonProperty(Order = -5, NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        /// <summary>
        ///     Background image for card
        /// </summary>
        [JsonProperty(Order = -4, NullValueHandling = NullValueHandling.Ignore)]
        public string BackgroundImage { get; set; }

        /// <summary>
        ///     Version of schema that this card was authored. Defaults to the latest Adaptive Card schema version that this library supports.
        /// </summary>
        [JsonProperty(Order = -9, Required = Required.Always)]
        [JsonRequired]
        public AdaptiveSchemaVersion Version { get; set; }

        /// <summary>
        ///     if a client doesn't support the minVersion the card should be rejected.  If it does, then the elements that are not
        ///     supported are safe to ignore
        /// </summary>
        [JsonProperty(Order = -8, NullValueHandling = NullValueHandling.Ignore)]
        public AdaptiveSchemaVersion MinVersion { get; set; }

        /// <summary>
        ///     if a client is not able to show the card, show fallbackText to the user. This can be in markdown format.
        /// </summary>
        [JsonProperty(Order = -7, NullValueHandling = NullValueHandling.Ignore)]
        public string FallbackText { get; set; }

        public bool ShouldSerializeActions()
        {
            return Actions.Any();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}