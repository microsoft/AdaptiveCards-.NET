// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace AdaptiveCards
{
    /// <summary>
    ///     Container for a collection of elements
    /// </summary>
#if !NETSTANDARD1_3
    [XmlType(TypeName = AdaptiveContainer.TypeName)]
#endif
    public class AdaptiveContainer : AdaptiveCollectionElement
    {
        public const string TypeName = "Container";

#if !NETSTANDARD1_3
        [XmlIgnore]
#endif
        public override string Type { get; set; } = TypeName;

        [JsonConverter(typeof(AdaptiveBackgroundImageConverter))]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(null)]
        public AdaptiveBackgroundImage BackgroundImage { get; set; }

        /// <summary>
        ///     Elements of the container
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(IgnoreEmptyItemsConverter<AdaptiveElement>))]
#if !NETSTANDARD1_3
        [XmlElement(typeof(AdaptiveTextBlock))]
        [XmlElement(typeof(AdaptiveImage))]
        [XmlElement(typeof(AdaptiveContainer))]
        [XmlElement(typeof(AdaptiveColumnSet))]
        [XmlElement(typeof(AdaptiveImageSet))]
        [XmlElement(typeof(AdaptiveFactSet))]
        [XmlElement(typeof(AdaptiveTextInput))]
        [XmlElement(typeof(AdaptiveDateInput))]
        [XmlElement(typeof(AdaptiveTimeInput))]
        [XmlElement(typeof(AdaptiveNumberInput))]
        [XmlElement(typeof(AdaptiveChoiceSetInput))]
        [XmlElement(typeof(AdaptiveToggleInput))]
        [XmlElement(typeof(AdaptiveMedia))]
        [XmlElement(typeof(AdaptiveActionSet))]
#endif
        public List<AdaptiveElement> Items { get; set; } = new List<AdaptiveElement>();

    }
}
