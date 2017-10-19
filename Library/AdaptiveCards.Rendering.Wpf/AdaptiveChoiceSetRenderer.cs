﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AdaptiveCards.Rendering.Wpf
{
    public static class AdaptiveChoiceSetRenderer
    {
        public static FrameworkElement Render(AdaptiveChoiceSetInput input, AdaptiveRenderContext context)
        {
            var chosen = input.Value?.Split(',').Select(p => p.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>();

            if (context.Config.SupportsInteractivity)
            {
                var uiGrid = new Grid();
                uiGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                uiGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

                var uiComboBox = new ComboBox();
                uiComboBox.Style = context.GetStyle("Adaptive.Input.AdaptiveChoiceSetInput.ComboBox");
                uiComboBox.DataContext = input;

                var uiChoices = new ListBox();
                ScrollViewer.SetHorizontalScrollBarVisibility(uiChoices, ScrollBarVisibility.Disabled);
                var itemsPanelTemplate = new ItemsPanelTemplate();
                var factory = new FrameworkElementFactory(typeof(WrapPanel));
                itemsPanelTemplate.VisualTree = factory;
                uiChoices.ItemsPanel = itemsPanelTemplate;
                uiChoices.DataContext = input;
                uiChoices.Style = context.GetStyle("Adaptive.Input.AdaptiveChoiceSetInput");

                foreach (var choice in input.Choices)
                {
                    if (input.IsMultiSelect == true)
                    {
                        var uiCheckbox = new CheckBox();
                        uiCheckbox.Content = choice.Title;
                        uiCheckbox.IsChecked = chosen.Contains(choice.Value);
                        uiCheckbox.DataContext = choice;
                        uiCheckbox.Style = context.GetStyle("Adaptive.Input.AdaptiveChoiceSetInput.CheckBox");
                        uiChoices.Items.Add(uiCheckbox);
                    }
                    else
                    {
                        if (input.Style == AdaptiveChoiceInputStyle.Compact)
                        {
                            var uiComboItem = new ComboBoxItem();
                            uiComboItem.Style = context.GetStyle("Adaptive.Input.AdaptiveChoiceSetInput.ComboBoxItem");
                            uiComboItem.Content = choice.Title;
                            uiComboItem.DataContext = choice;
                            uiComboBox.Items.Add(uiComboItem);
                            if (chosen.Contains(choice.Value))
                                uiComboBox.SelectedItem = uiComboItem;
                        }
                        else
                        {
                            var uiRadio = new RadioButton();
                            uiRadio.Content = choice.Title;
                            uiRadio.IsChecked = chosen.Contains(choice.Value);
                            uiRadio.GroupName = input.Id;
                            uiRadio.DataContext = choice;
                            uiRadio.Style = context.GetStyle("Adaptive.Input.AdaptiveChoiceSetInput.Radio");
                            uiChoices.Items.Add(uiRadio);
                        }
                    }
                }
                context.InputBindings.Add(input.Id, () =>
                {
                    if (input.IsMultiSelect == true)
                    {
                        string values = string.Empty;
                        foreach (var item in uiChoices.Items)
                        {
                            CheckBox checkBox = (CheckBox)item;
                            AdaptiveChoice adaptiveChoice = checkBox.DataContext as AdaptiveChoice;
                            if (checkBox.IsChecked == true)
                                values += (values == string.Empty ? "" : ",") + adaptiveChoice.Value;
                        }
                        return values;
                    }
                    else
                    {
                        if (input.Style == AdaptiveChoiceInputStyle.Compact)
                        {
                            ComboBoxItem item = uiComboBox.SelectedItem as ComboBoxItem;
                            if (item != null)
                            {
                                AdaptiveChoice adaptiveChoice = item.DataContext as AdaptiveChoice;
                                return adaptiveChoice.Value;
                            }
                            return null;
                        }
                        else
                        {
                            foreach (var item in uiChoices.Items)
                            {
                                RadioButton radioBox = (RadioButton)item;
                                AdaptiveChoice adaptiveChoice = radioBox.DataContext as AdaptiveChoice;
                                if (radioBox.IsChecked == true)
                                    return adaptiveChoice.Value;
                            }
                            return null;
                        }
                    }
                });
                if (input.Style == AdaptiveChoiceInputStyle.Compact)
                {
                    Grid.SetRow(uiComboBox, 1);
                    uiGrid.Children.Add(uiComboBox);
                    return uiGrid;
                }
                else
                {
                    Grid.SetRow(uiChoices, 1);
                    uiGrid.Children.Add(uiChoices);
                    return uiGrid;
                }
            }

            string choiceText = XamlUtilities.GetFallbackText(input);
            if (choiceText == null)
            {
                List<string> choices = input.Choices.Select(choice => choice.Title).ToList();
                if (input.Style == AdaptiveChoiceInputStyle.Compact)
                {
                    if (input.IsMultiSelect)
                    {
                        choiceText = $"Choices: {RendererUtilities.JoinString(choices, ", ", " and ")}";
                    }
                    else
                    {
                        choiceText = $"Choices: {RendererUtilities.JoinString(choices, ", ", " or ")}";
                    }
                }
                else // if (adaptiveChoiceSetInput.Style == ChoiceInputStyle.Expanded)
                {
                    choiceText = $"* {RendererUtilities.JoinString(choices, "\n* ", "\n* ")}";
                }
            }
            AdaptiveContainer container = AdaptiveTypedElementConverter.CreateElement<AdaptiveContainer>();
            container.Spacing = input.Spacing;
            container.Separator = input.Separator;
            AdaptiveTextBlock textBlock = AdaptiveTypedElementConverter.CreateElement<AdaptiveTextBlock>();
            textBlock.Text = choiceText;
            textBlock.Wrap = true;
            container.Items.Add(textBlock);

            textBlock = AdaptiveTypedElementConverter.CreateElement<AdaptiveTextBlock>();
            textBlock.Text = RendererUtilities.JoinString(input.Choices.Where(c => chosen.Contains(c.Value)).Select(c => c.Title).ToList(), ", ", " and ");
            textBlock.Color = AdaptiveTextColor.Accent;
            textBlock.Wrap = true;
            container.Items.Add(textBlock);
            return context.Render(container);
        }
    }
}