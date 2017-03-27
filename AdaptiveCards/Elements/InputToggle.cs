﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace AdaptiveCards
{
    /// <summary>
    /// Input which collects a choice between two options from the user
    /// </summary>
    public partial class InputToggle: Input
    {
        public InputToggle()
        {
            this.Type = "Input.Toggle";
        }

        /// <summary>
        /// Title text for toggle
        /// </summary>
        [JsonRequired]
#if DESKTOP
        [XmlAttribute]
#endif
        public string Title { get; set; }

        /// <summary>
        /// Value to use for on (Default: true)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#if DESKTOP
        [XmlAttribute]
#endif
        public string ValueOn { get; set; } = "true";

        /// <summary>
        /// Value to use for off (Default: false)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#if DESKTOP
        [XmlAttribute]
#endif
        public string ValueOff { get; set; } = "false";

        /// <summary>
        /// The initial value for the field
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#if DESKTOP
        [XmlAttribute]
#endif
        public string Value { get; set; }
    }
}