﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace AdaptiveCards.Rendering.Config
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ContainerStyleConfig
    {
        public ContainerStyleConfig() { }

        /// <summary>
        /// The background color to use for this container
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BackgroundColor { get; set; } = "#00FFFFFF";

        /// <summary>
        /// The font colors to use for this container
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public FontColorsConfig FontColors { get; set; } = new FontColorsConfig();
    }
}
