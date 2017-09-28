﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveCards.Rendering.Config
{
    /// <summary>
    /// Properties which control rendering of actions
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ActionsConfig 
    {
        public ActionsConfig() { }


        /// <summary>
        /// Arrange actions horizontal or vertical
        /// </summary>
        public ActionsOrientation ActionsOrientation { get; set; } = ActionsOrientation.Horizontal;

        /// <summary>
        /// should they be aligned Left, Center or Right
        /// </summary>
        public HorizontalAlignment ActionAlignment { get; set; } = HorizontalAlignment.Center;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BackgroundColor { get; set; } = "#FF5098FF";

        /// <summary>
        /// Space between actions
        /// </summary>
        public int ButtonSpacing { get; set; } = 8;

        /// <summary>
        /// Max number of actions to support on your Cards(e.g., 3)
        /// </summary>
        public int MaxActions { get; set; } = 5;

        /// <summary>
        /// ShowCard configuration
        /// </summary>
        public ShowCardConfig ShowCard { get; set; } = new ShowCardConfig();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BorderColor { get; set; } = "#FF000000";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string TextColor { get; set; } = "#FFFFFFFF";

        public int BorderThickness { get; set; } = 1;

        public int FontWeight { get; set; } = 400;

        public int FontSize { get; set; } = 12;


    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ShowCardConfig
    {
        public ShowCardConfig() { }

        public ShowCardActionMode ActionMode { get; set; } = ShowCardActionMode.Inline;

        /// <summary>
        /// If actionMode is inline this is the margin between the inline card and the actions
        /// </summary>
        public int InlineTopMargin { get; set; } = 8;
    }

    [JsonConverter(typeof(StringEnumConverter), true)]
    public enum ShowCardActionMode
    {
        /// <summary>
        /// Show the card inline 
        /// </summary>
        Inline,

        /// <summary>
        /// Popup the card 
        /// </summary>
        Popup
    }

    [JsonConverter(typeof(StringEnumConverter), true)]
    public enum ActionsOrientation
    {
        /// <summary>
        /// actions should be laid out horizontally
        /// </summary>
        Horizontal,

        /// <summary>
        /// Actions should be laid out vertically
        /// </summary>
        Vertical
    }

}
