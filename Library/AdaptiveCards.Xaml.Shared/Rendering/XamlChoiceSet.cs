﻿using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Windows;
using AdaptiveCards.Rendering;
#if WPF
using System.Windows.Controls;
#elif XAMARIN
using Xamarin.Forms;
#endif

namespace AdaptiveCards.Rendering
{
    public class XamlChoiceSet : ChoiceSet, IRender<FrameworkElement, RenderContext>
    {
        /// <summary>
        /// Input.ChoiceSet
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public FrameworkElement Render(RenderContext context)
        {
#if WPF
            if (context.Options.AdaptiveCard.SupportsInteractivity)
            {
                var uiGrid = new Grid();
                uiGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                uiGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

                var uiComboBox = new ComboBox();
                uiComboBox.Style = context.GetStyle("Adaptive.Input.ChoiceSet.ComboBox");
                uiComboBox.DataContext = this;

                var uiChoices = new ListBox();
                ScrollViewer.SetHorizontalScrollBarVisibility(uiChoices, ScrollBarVisibility.Disabled);
                var itemsPanelTemplate = new ItemsPanelTemplate();
                var factory = new FrameworkElementFactory(typeof(WrapPanel));
                itemsPanelTemplate.VisualTree = factory;
                uiChoices.ItemsPanel = itemsPanelTemplate;
                uiChoices.DataContext = this;
                uiChoices.Style = context.GetStyle("Adaptive.Input.ChoiceSet");

                foreach (var choice in this.Choices)
                {
                    if (this.IsMultiSelect == true)
                    {
                        var uiCheckbox = new CheckBox();
                        uiCheckbox.Content = choice.Title;
                        uiCheckbox.IsChecked = choice.IsSelected;
                        uiCheckbox.DataContext = choice;
                        uiCheckbox.Style = context.GetStyle("Adaptive.Input.ChoiceSet.CheckBox");
                        uiChoices.Items.Add(uiCheckbox);
                    }
                    else
                    {
                        if (this.Style == ChoiceInputStyle.Compact)
                        {
                            var uiComboItem = new ComboBoxItem();
                            uiComboItem.Style = context.GetStyle("Adaptive.Input.ChoiceSet.ComboBoxItem");
                            uiComboItem.Content = choice.Title;
                            uiComboItem.DataContext = choice;
                            uiComboBox.Items.Add(uiComboItem);
                            if (choice.IsSelected)
                                uiComboBox.SelectedItem = uiComboItem;
                        }
                        else
                        {
                            var uiRadio = new RadioButton();
                            uiRadio.Content = choice.Title;
                            uiRadio.IsChecked = choice.IsSelected;
                            uiRadio.GroupName = this.Id;
                            uiRadio.DataContext = choice;
                            uiRadio.Style = context.GetStyle("Adaptive.Input.ChoiceSet.Radio");
                            uiChoices.Items.Add(uiRadio);
                        }
                    }
                }
                context.InputBindings.Add(this.Id, () =>
                {
                    if (this.IsMultiSelect == true)
                    {
                        List<string> values = new List<string>();
                        foreach (var item in uiChoices.Items)
                        {
                            CheckBox checkBox = (CheckBox)item;
                            Choice choice = checkBox.DataContext as Choice;
                            if (checkBox.IsChecked == true)
                                values.Add(choice.Value);
                        }
                        return values;
                    }
                    else
                    {
                        if (this.Style == ChoiceInputStyle.Compact)
                        {
                            ComboBoxItem item = uiComboBox.SelectedItem as ComboBoxItem;
                            if (item != null)
                            {
                                Choice choice = item.DataContext as Choice;
                                return choice.Value;
                            }
                            return null;
                        }
                        else
                        {
                            foreach (var item in uiChoices.Items)
                            {
                                RadioButton radioBox = (RadioButton)item;
                                Choice choice = radioBox.DataContext as Choice;
                                if (radioBox.IsChecked == true)
                                    return choice.Value;
                            }
                            return null;
                        }
                    }
                });
                if (this.Style == ChoiceInputStyle.Compact)
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

#endif

            string choiceText = XamlUtilities.GetFallbackText(this);
            if (choiceText == null)
            {
                List<string> choices = this.Choices.Select(choice => choice.Title).ToList();
                if (this.Style == ChoiceInputStyle.Compact)
                {
                    if (this.IsMultiSelect)
                    {
                        choiceText = $"Choices: {RendererUtilities.JoinString(choices, ", ", " and ")}";
                    }
                    else
                    {
                        choiceText = $"Choices: {RendererUtilities.JoinString(choices, ", ", " or ")}";
                    }
                }
                else // if (this.Style == ChoiceInputStyle.Expanded)
                {
                    choiceText = $"* {RendererUtilities.JoinString(choices, "\n* ", "\n* ")}";
                }
            }
            Container container = TypedElementConverter.CreateElement<Container>();
            container.Separation = this.Separation;
            TextBlock textBlock = TypedElementConverter.CreateElement<TextBlock>();
            textBlock.Text = choiceText;
            textBlock.Wrap = true;
            container.Items.Add(textBlock);

            textBlock = TypedElementConverter.CreateElement<TextBlock>();
            textBlock.Text = RendererUtilities.JoinString(this.Choices.Where(c => c.IsSelected).Select(c => c.Title).ToList(), ", ", " and ");
            textBlock.Color = TextColor.Accent;
            textBlock.Wrap = true;
            container.Items.Add(textBlock);
            return context.Render(container);
        }
    }
}