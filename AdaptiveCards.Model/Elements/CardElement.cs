﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AdaptiveCards
{
    /// <summary>
    /// Base class for all elements in a container
    /// </summary>
    public partial class CardElement : TypedElement
    {
        public CardElement() : base()
        {
        }

        /// <summary>
        /// SSML fragment for spoken interaction
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#if DESKTOP
        [XmlElement]
#endif
        public string Speak { get; set; }

        /// <summary>
        /// How should this element be emphasized relative to previous element
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#if DESKTOP
        [XmlAttribute]
#endif
        public SeparationStyle Separation { get; set; }

        public bool ShouldSerializeSeparation() { return this.Separation != SeparationStyle.Default; }
    }

}



