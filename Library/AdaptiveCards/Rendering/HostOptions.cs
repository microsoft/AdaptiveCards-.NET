﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdaptiveCards.Rendering
{
    public class HostOptions
    {
        public HostOptions() { }

        //  ------ AdaptiveCard -------
        public AdaptiveCardOptions AdaptiveCard { get; set; } = new AdaptiveCardOptions();

        /// <summary>
        /// Color settings for the TextBlock
        /// </summary>
        public TextColorOptions Colors { get; set; } = new TextColorOptions();

        // ------ Basic ------
        public TextBlockOptions TextBlock { get; set; } = new TextBlockOptions();

        public ImageOptions Image { get; set; } = new ImageOptions();

        // ------ Containers ------
        public ContainerOptions Container { get; set; } = new ContainerOptions();

        public ContainerSetOptions ContainerSet { get; set; } = new ContainerSetOptions();

        public ImageSetOptions ImageSet { get; set; } = new ImageSetOptions();

        public FactSetOptions FactSet { get; set; } = new FactSetOptions();

        public ActionSetOptions ActionSet { get; set; } = new ActionSetOptions();

        // ------ Input ------
        public InputOptions Input { get; set; } = new InputOptions();

        // ------ Actions------
        public ActionOptions Actions { get; set; } = new ActionOptions();

        public CardElementOptions GetElementStyling(object obj)
        {
            if (obj is TextBlock)
                return this.TextBlock;
            if (obj is Image)
                return this.Image;
            if (obj is Container)
                return this.Container;
            if (obj is ContainerSet)
                return this.ContainerSet;
            if (obj is ActionSet)
                return this.ActionSet;
            if (obj is ImageSet)
                return this.ImageSet;
            if (obj is ImageSet)
                return this.ImageSet;
            if (obj is FactSet)
                return this.FactSet;
            if (obj is InputText)
                return this.Input;
            if (obj is InputNumber)
                return this.Input;
            if (obj is InputDate)
                return this.Input;
            if (obj is InputTime)
                return this.Input;
            if (obj is InputChoiceSet)
                return this.Input;
            if (obj is InputToggle)
                return this.Input;
            throw new ArgumentException($"Unknown type {obj.GetType().Name}");
        }
    }

    public class BoundaryOptions
    {
        public BoundaryOptions() { }

        public BoundaryOptions(int allMargin)
        {
            Left = Right = Top = Bottom = allMargin;
        }
        public BoundaryOptions(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }


        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
    }

    public class AdaptiveCardOptions
    {
        public AdaptiveCardOptions() { }

        /// <summary>
        ///  Margin for the card
        /// </summary>
        public BoundaryOptions Margin { get; set; } = new BoundaryOptions(8);

        /// <summary>
        /// Background color for card
        /// </summary>
        public string BackgroundColor { get; set; } = "#FFFFFFFF";

        /// <summary>
        /// Font family for the card
        /// </summary>
        public string FontFamily { get; set; } = "Calibri";

        /// <summary>
        /// Arrange actions horizontal or vertical
        /// </summary>
        public ActionsOrientation ActionsOrientation { get; set; } = ActionsOrientation.Horizontal;

        /// <summary>
        /// should they be aligned Left, Center or Right
        /// </summary>
        public HorizontalAlignment ActionAlignment { get; set; } = HorizontalAlignment.Center;

        /// <summary>
        /// Toggles whether or not to render inputs and actions
        /// </summary>
        public bool SupportsInteractivity { get; set; } = true;

        /// <summary>
        /// The types of Actions that you support(null for no actions)
        /// </summary>
        public string[] SupportedActionTypes { get; set; } = new string[]
        {
            ActionOpenUrl.TYPE,
            ActionSubmit.TYPE,
            ActionHttp.TYPE,
            ActionShowCard.TYPE
        };

        /// <summary>
        /// Max number of actions to support on your Cards(e.g., 3)
        /// </summary>
        public int MaxActions { get; set; } = 5;
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


    /// <summary>
    /// Shared properties for elements
    /// </summary>
    public class CardElementOptions
    {
        public CardElementOptions()
        { }

        /// <summary>
        /// Separation settings 
        /// </summary>
        public SeparationOptions Separation { get; set; } = new SeparationOptions();
    }

    /// <summary>
    /// Properties which control spacing and visual between elements
    /// </summary>
    public class SeparationOptions
    {
        public SeparationOptions() { }

        /// <summary>
        /// Separation settings when Separation:default
        /// </summary>
        public SeparationOption Default { get; set; } = new SeparationOption() { Spacing = 10, LineThickness = 0 };

        /// <summary>
        /// Separation settings when Separation:Strong
        /// </summary>
        public SeparationOption Strong { get; set; } = new SeparationOption() { Spacing = 20, LineThickness = 1, LineColor = "#FF707070" };
    }

    public class SeparationOption
    {
        public SeparationOption() { }

        /// <summary>
        /// How much space between this element and previous should be used
        /// </summary>
        public int Spacing { get; set; }

        /// <summary>
        /// If there is a visible line, how thick should the line be
        /// </summary>
        public int LineThickness { get; set; }

        /// <summary>
        /// If there is a visible color, what color to use
        /// </summary>
        public string LineColor { get; set; }

    }

    /// <summary>
    /// Properties which control rendering of TextBlock 
    /// </summary>
    public class TextBlockOptions : CardElementOptions
    {
        public TextBlockOptions() { }

        /// <summary>
        /// FontSize
        /// </summary>
        public FontSizeOptions FontSize { get; set; } = new FontSizeOptions();
    }

    public class FontSizeOptions
    {
        public FontSizeOptions() { }

        public int Small { get; set; } = 10;

        public int Normal { get; set; } = 12;

        public int Medium { get; set; } = 14;

        public int Large { get; set; } = 17;

        public int ExtraLarge { get; set; } = 20;

    }

    public class TextColorOptions
    {
        public TextColorOptions() { }

        public ColorOption Default { get; set; } = new ColorOption("#FF000000");

        public ColorOption Accent { get; set; } = new ColorOption("#FF0000FF");

        public ColorOption Dark { get; set; } = new ColorOption("#FF101010");

        public ColorOption Light { get; set; } = new ColorOption("#FFFFFFFF");

        public ColorOption Good { get; set; } = new ColorOption("#FF008000");

        public ColorOption Warning { get; set; } = new ColorOption("#FFFFD700");

        public ColorOption Attention { get; set; } = new ColorOption("#FF8B0000");
    }

    public class ColorOption
    {
        public ColorOption(string color, double opacity = .7)
        {
            this.Color = color;
            this.IsSubtleOpacity = opacity;
        }

        public string Color { get; set; }

        public double IsSubtleOpacity { get; set; } = .7;
    }

    /// <summary>
    /// properties which control rendering of Images
    /// </summary>
    public class ImageOptions : CardElementOptions
    {
        public ImageOptions() { }

        public ImageSizeOptions Size { get; set; } = new ImageSizeOptions();
    }

    public class ImageSizeOptions

    {
        public ImageSizeOptions() { }

        public int Small { get; set; } = 60;

        public int Medium { get; set; } = 120;

        public int Large { get; set; } = 180;
    }

    /// <summary>
    /// Properties which control rendering of actions
    /// </summary>
    public class ActionOptions
    {
        public ActionOptions() { }

        public ShowCardOptions ShowCard { get; set; } = new ShowCardOptions();

        public string BackgroundColor { get; set; } = "#FF5098FF";

        public string BorderColor { get; set; } = "#FF000000";

        public string TextColor { get; set; } = "#FFFFFFFF";

        public int BorderThickness { get; set; } = 1;

        public int FontWeight { get; set; } = 400;

        public int FontSize { get; set; } = 12;

        /// <summary>
        /// Space between actions
        /// </summary>
        public BoundaryOptions Margin { get; set; } = new BoundaryOptions(4, 10, 4, 0);

        /// <summary>
        /// space between title and button edge
        /// </summary>

        public BoundaryOptions Padding { get; set; } = new BoundaryOptions(4);

    }

    public class ShowCardOptions
    {
        public ShowCardOptions() { }

        public ShowCardActionMode ActionMode { get; set; } = ShowCardActionMode.Popup;

        /// <summary>
        /// Background color for showcard area
        /// </summary>
        public string BackgroundColor { get; set; } = "#FFF8F8F8";

        /// <summary>
        /// margins for showcard when inline
        /// </summary>
        public BoundaryOptions Margin { get; set; } = new BoundaryOptions(10);

        /// <summary>
        /// Padding for showcard when inline
        /// </summary>
        public BoundaryOptions Padding { get; set; } = new BoundaryOptions(10);
    }

    [JsonConverter(typeof(StringEnumConverter), true)]
    public enum ShowCardActionMode
    {
        Inline,
        Popup
    }


    public class ImageSetOptions : CardElementOptions
    {
        public ImageSetOptions() { }

        public bool Wrap { get; set; } = true;
    }

    public class FactSetOptions : CardElementOptions
    {
        public FactSetOptions() { }

        /// <summary>
        /// TextBlock to use for Titles in factsets
        /// </summary>
        public TextBlock Title { get; set; } = new TextBlock() { Weight = TextWeight.Bolder };

        /// <summary>
        /// TextBlock to use for Values in fact sets
        /// </summary>
        public TextBlock Value { get; set; } = new TextBlock() { };
    }

    public class InputOptions : CardElementOptions
    {
        public InputOptions() { }
    }

    public class ContainerSetOptions : CardElementOptions
    {
        public ContainerSetOptions() { }
    }

    public class ContainerOptions : CardElementOptions
    {
        public ContainerOptions() { }
    }

    public class ActionSetOptions : CardElementOptions
    {
        public ActionSetOptions() { }

        public int MaxActions { get; set; } = 5;

        /// <summary>
        /// The types of Actions that you support(null for no actions)
        /// </summary>
        public string[] SupportedActions { get; set; } = new string[]
        {
            ActionOpenUrl.TYPE,
            ActionSubmit.TYPE,
            ActionHttp.TYPE,
            ActionShowCard.TYPE
        };
    }
}
