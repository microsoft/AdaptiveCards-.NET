// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace AdaptiveCards.Rendering.Wpf
{
    public static class XceedTextInput
    {
        public static FrameworkElement Render(AdaptiveTextInput input, AdaptiveRenderContext context)
        {
            if (context.Config.SupportsInteractivity)
            {
                var textBox = new WatermarkTextBox() { Text = input.Value };
                if (input.IsMultiline == true)
                {
                    textBox.AcceptsReturn = true;
                    textBox.TextWrapping = TextWrapping.Wrap;
                    textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                }
                if (input.MaxLength > 0)
                {
                    textBox.MaxLength = input.MaxLength;
                }

                textBox.Watermark = input.Placeholder;
                textBox.Style = context.GetStyle($"Adaptive.Input.Text.{input.Style}");
                textBox.DataContext = input;
                context.InputBindings.Add(input.Id, () => textBox.Text);
                if (input.InlineAction != null)
                {
                    if (context.Config.Actions.ShowCard.ActionMode == ShowCardActionMode.Inline &&
                        input.InlineAction.GetType() == typeof(AdaptiveShowCardAction))
                    {
                        context.Warnings.Add(new AdaptiveWarning(-1, "Inline ShowCard not supported for InlineAction"));
                    }
                    else
                    {
                        if (context.Config.SupportsInteractivity && context.ActionHandlers.IsSupported(input.InlineAction.GetType()))
                        {
                            // Set up a parent view that holds textbox, separator and button
                            var parentView = new Grid();

                            // grid config for textbox
                            parentView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                            Grid.SetColumn(textBox, 0);
                            parentView.Children.Add(textBox);

                            // grid config for spacing
                            int spacing = context.Config.GetSpacing(AdaptiveSpacing.Default);
                            var uiSep = new Grid
                            {
                                Style = context.GetStyle($"Adaptive.Input.Text.InlineAction.Separator"),
                                VerticalAlignment = VerticalAlignment.Stretch,
                                Width = spacing,
                            };
                            parentView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(spacing, GridUnitType.Pixel) });
                            Grid.SetColumn(uiSep, 1);

                            // adding button
                            var uiButton = new Button();
                            Style style = context.GetStyle($"Adaptive.Input.Text.InlineAction.Button");
                            if(style != null)
                            {
                                uiButton.Style = style;
                            }

                            // this textblock becomes tooltip if icon url exists else becomes the tile for the button
                            var uiTitle = new TextBlock
                            {
                                Text = input.InlineAction.Title,
                            };

                            if (input.InlineAction.IconUrl != null)
                            {
                                var actionsConfig = context.Config.Actions;

                                var image = new AdaptiveImage(input.InlineAction.IconUrl)
                                {
                                    HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                                    Type = "Adaptive.Input.Text.InlineAction.Image",
                                };

                                FrameworkElement uiIcon = null;
                                uiIcon = AdaptiveImageRenderer.Render(image, context);
                                uiButton.Content = uiIcon;

                                // adjust height
                                textBox.Loaded += (sender, e) =>
                                {
                                    uiIcon.Height = textBox.ActualHeight;
                                };

                                uiButton.ToolTip = uiTitle;
                            }
                            else
                            {
                                uiTitle.FontSize = context.Config.GetFontSize(AdaptiveFontStyle.Default, AdaptiveTextSize.Default);
                                uiTitle.Style = context.GetStyle($"Adaptive.Input.Text.InlineAction.Title");
                                uiButton.Content = uiTitle;
                            }

                            uiButton.Click += (sender, e) =>
                            {
                                context.InvokeAction(uiButton, new AdaptiveActionEventArgs(input.InlineAction));

                                // Prevent nested events from triggering
                                e.Handled = true;
                            };

                            parentView.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                            Grid.SetColumn(uiButton, 2);
                            parentView.Children.Add(uiButton);
                            uiButton.VerticalAlignment = VerticalAlignment.Bottom;

                            textBox.KeyDown += (sender, e) =>
                            {
                                if (e.Key == System.Windows.Input.Key.Enter)
                                {
                                    context.InvokeAction(uiButton, new AdaptiveActionEventArgs(input.InlineAction));
                                    e.Handled = true;
                                }
                            };
                            return parentView;
                        }
                    }
                }
                return textBox;
            }
            else
            {
                var textBlock = AdaptiveTypedElementConverter.CreateElement<AdaptiveTextBlock>();
                textBlock.Text = XamlUtilities.GetFallbackText(input) ?? input.Placeholder;
                return context.Render(textBlock);
            }
        }
    }
}
