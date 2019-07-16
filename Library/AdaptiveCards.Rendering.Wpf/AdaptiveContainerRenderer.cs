// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AdaptiveCards.Rendering.Wpf
{
    public static class AdaptiveContainerRenderer
    {
        public static FrameworkElement Render(AdaptiveContainer container, AdaptiveRenderContext context)
        {
            var uiContainer = new Grid();
            uiContainer.Style = context.GetStyle("Adaptive.Container");
            uiContainer.SetBackgroundSource(container.BackgroundImage, context);

            // Keep track of ContainerStyle.ForegroundColors before Container is rendered
            var parentRenderArgs = context.RenderArgs;
            // This is the renderArgs that will be passed down to the children
            var childRenderArgs = new AdaptiveRenderArgs(parentRenderArgs);

            Grid uiOuterContainer = new Grid();

            uiOuterContainer.Children.Add(uiContainer);
            Border border = new Border();
            border.Child = uiOuterContainer;

            RendererUtil.ApplyVerticalContentAlignment(uiContainer, container);
            uiContainer.MinHeight = container.PixelMinHeight;

            bool inheritsStyleFromParent = !container.Style.HasValue;
            bool hasPadding = false;
            if (!inheritsStyleFromParent)
            {
                hasPadding = ApplyPadding(border, uiOuterContainer, container, parentRenderArgs, context);

                // Apply background color
                ContainerStyleConfig containerStyle = context.Config.ContainerStyles.GetContainerStyleConfig(container.Style);
                border.Background = context.GetColorBrush(containerStyle.BackgroundColor);

                childRenderArgs.ForegroundColors = containerStyle.ForegroundColors;
            }

            switch (container.VerticalContentAlignment)
            {
                case AdaptiveVerticalContentAlignment.Center:
                    uiContainer.VerticalAlignment = VerticalAlignment.Center;
                    break;
                case AdaptiveVerticalContentAlignment.Bottom:
                    uiContainer.VerticalAlignment = VerticalAlignment.Bottom;
                    break;
                case AdaptiveVerticalContentAlignment.Top:
                default:
                    break;
            }

            if (hasPadding)
            {
                childRenderArgs.BleedDirection = BleedDirection.BleedAll;
            }

            // Modify context outer parent style so padding necessity can be determined
            childRenderArgs.ParentStyle = (inheritsStyleFromParent) ? parentRenderArgs.ParentStyle : container.Style.Value;
            childRenderArgs.HasParentWithPadding = (hasPadding || parentRenderArgs.HasParentWithPadding);
            context.RenderArgs = childRenderArgs;

            AddContainerElements(uiContainer, container.Items, context);

            // Revert context's value to that of outside the Container
            context.RenderArgs = parentRenderArgs;

            return RendererUtil.ApplySelectAction(border, container, context);
        }

        public static void AddContainerElements(Grid uiContainer, IList<AdaptiveElement> elements, AdaptiveRenderContext context)
        {
            // Keeping track of the index so we don't have to call IndexOf function on every iteration
            int index = 0;
            foreach (var cardElement in elements)
            {
                if (index != 0)
                {
                    // Only the first element can bleed to the top
                    context.RenderArgs.BleedDirection &= ~BleedDirection.BleedUp;
                }

                if (index != elements.Count - 1)
                {
                    // Only the last element can bleed to the bottom
                    context.RenderArgs.BleedDirection &= ~BleedDirection.BleedDown;
                }

                index++;

                // each element has a row
                FrameworkElement uiElement = context.Render(cardElement);
                if (uiElement != null)
                {
                    TagContent tag = null;
                    Grid separator = null;
                    if (cardElement.Separator && uiContainer.Children.Count > 0)
                    {
                        separator = AddSeparator(context, cardElement, uiContainer);
                    }
                    else if (uiContainer.Children.Count > 0)
                    {
                        separator = AddSpacing(context, cardElement, uiContainer);
                    }

                    tag = new TagContent(separator, uiContainer);

                    uiElement.Tag = tag;

                    // Sets the minHeight property for Container and ColumnSet
                    if (cardElement.Type == "Container" || cardElement.Type == "ColumnSet")
                    {
                        AdaptiveCollectionElement collectionElement = (AdaptiveCollectionElement)cardElement;
                        uiElement.MinHeight = collectionElement.PixelMinHeight;
                    }

                    int rowDefinitionIndex = uiContainer.RowDefinitions.Count;
                    RowDefinition rowDefinition = null;
                    if (cardElement.Height != AdaptiveHeight.Stretch)
                    {
                        rowDefinition = new RowDefinition() { Height = GridLength.Auto };

                        uiContainer.RowDefinitions.Add(rowDefinition);
                        Grid.SetRow(uiElement, rowDefinitionIndex);
                        uiContainer.Children.Add(uiElement);

                        // Row definition is stored in the tag for containers and elements that stretch
                        // so when the elements are shown, the row can have it's original definition,
                        // while when the element is hidden, the extra space is not reserved in the layout
                        tag.RowDefinition = rowDefinition;
                        tag.ViewIndex = rowDefinitionIndex;

                        context.SetVisibility(uiElement, cardElement.IsVisible, tag);
                    }
                    else
                    {
                        rowDefinition = new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) };
                        
                        uiContainer.RowDefinitions.Add(rowDefinition);

                        // Row definition is stored in the tag for containers and elements that stretch
                        // so when the elements are shown, the row can have it's original definition,
                        // while when the element is hidden, the extra space is not reserved in the layout
                        tag.RowDefinition = rowDefinition;
                        tag.ViewIndex = rowDefinitionIndex;

                        if (cardElement.Type == "Container")
                        {
                            Grid.SetRow(uiElement, rowDefinitionIndex);
                            uiContainer.Children.Add(uiElement);
                            context.SetVisibility(uiElement, cardElement.IsVisible, tag);
                        }
                        else
                        {
                            StackPanel panel = new StackPanel();

                            if (!String.IsNullOrEmpty(cardElement.Id))
                            {
                                panel.Name = cardElement.Id;
                            }

                            panel.Children.Add(uiElement);
                            panel.Tag = tag;

                            Grid.SetRow(panel, rowDefinitionIndex);
                            uiContainer.Children.Add(panel);
                            context.SetVisibility(panel, cardElement.IsVisible, tag);
                        }
                    }

                    if (cardElement.Type == "ActionSet")
                    {
                        AdaptiveActionSetRenderer.AddShowCardsViewsToRoot(context);
                    }

                }
            }

            context.ResetSeparatorVisibilityInsideContainer(uiContainer);
        }
        
        /// <summary>
        /// Adds spacing as a grid element to the container
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="element">Element to render spacing for</param>
        /// <param name="uiContainer">Container of rendered elements</param>
        /// <returns>Spacing grid</returns>
        public static Grid AddSpacing(AdaptiveRenderContext context, AdaptiveElement element, Grid uiContainer)
        {
            if (element.Spacing == AdaptiveSpacing.None)
            {
                return null;
            }

            var uiSpa = new Grid();
            uiSpa.Style = context.GetStyle($"Adaptive.Spacing");
            int spacing = context.Config.GetSpacing(element.Spacing);

            uiSpa.SetHeight(spacing);

            uiContainer.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            Grid.SetRow(uiSpa, uiContainer.RowDefinitions.Count - 1);
            uiContainer.Children.Add(uiSpa);

            return uiSpa;
        }

        public static Grid AddSeparator(AdaptiveRenderContext context, AdaptiveElement element, Grid uiContainer)
        {
            if (element.Spacing == AdaptiveSpacing.None && !element.Separator)
            {
                return null;
            }

            var uiSep = new Grid();
            uiSep.Style = context.GetStyle($"Adaptive.Separator");
            int spacing = context.Config.GetSpacing(element.Spacing);

            SeparatorConfig sepStyle = context.Config.Separator;

            var margin = (spacing - sepStyle.LineThickness) / 2;
            uiSep.Margin = new Thickness(0, margin, 0, margin);
            uiSep.SetHeight(sepStyle.LineThickness);
            if (!string.IsNullOrWhiteSpace(sepStyle.LineColor))
            {
                uiSep.SetBackgroundColor(sepStyle.LineColor, context);
            }
            uiContainer.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            Grid.SetRow(uiSep, uiContainer.RowDefinitions.Count - 1);
            uiContainer.Children.Add(uiSep);

            return uiSep;
        }

        private static Thickness GetBleedMargin(AdaptiveRenderArgs parentRenderArgs, int padding)
        {
            Thickness bleedMargin = new Thickness(0);

            if ((parentRenderArgs.BleedDirection & BleedDirection.BleedLeft) != BleedDirection.BleedNone)
            {
                bleedMargin.Left = padding;
            }

            if ((parentRenderArgs.BleedDirection & BleedDirection.BleedRight) != BleedDirection.BleedNone)
            {
                bleedMargin.Right = padding;
            }

            if ((parentRenderArgs.BleedDirection & BleedDirection.BleedUp) != BleedDirection.BleedNone)
            {
                bleedMargin.Top = padding;
            }

            if ((parentRenderArgs.BleedDirection & BleedDirection.BleedDown) != BleedDirection.BleedNone)
            {
                bleedMargin.Bottom = padding;
            }

            return bleedMargin;
        }

        // For applying bleeding, we must know if the element has padding, so both properties are applied in the same method
        public static bool ApplyPadding(Border border, Grid uiElement, AdaptiveCollectionElement element, AdaptiveRenderArgs parentRenderArgs, AdaptiveRenderContext context)
        {
            bool canApplyPadding = false;

            // AdaptiveColumn inherits from AdaptiveContainer so only one check is required for both
            if (element is AdaptiveContainer container)
            {
                canApplyPadding = ((container.BackgroundImage != null) || (container.Style.HasValue && (container.Style != parentRenderArgs.ParentStyle)));
            }
            else if (element is AdaptiveColumnSet columnSet)
            {
                canApplyPadding = (columnSet.Style.HasValue && (columnSet.Style != parentRenderArgs.ParentStyle));
            }

            int padding = context.Config.Spacing.Padding;

            if (canApplyPadding)
            {
                uiElement.Margin = new Thickness(padding);

                if (element.Bleed)
                {
                    border.Margin = GetBleedMargin(parentRenderArgs, -padding);
                }
            }

            return canApplyPadding;
        }
    }
}
