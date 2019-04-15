using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Windows;
using System.Windows.Controls;

namespace AdaptiveCards.Rendering.Wpf
{
    public static class AdaptiveChoiceSetRenderer
    {
        public static FrameworkElement Render(AdaptiveChoiceSetInput input, AdaptiveRenderContext context)
        {
            var chosen = input.Value?.Split(',').Select(p => p.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>();

            var uiGrid = new Grid();
            uiGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            uiGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            var uiComboBox = new ComboBox();
            uiComboBox.Style = context.GetStyle("Adaptive.Input.AdaptiveChoiceSetInput.ComboBox");
            uiComboBox.DataContext = input;

            var uiChoices = new StackPanel();
            uiChoices.DataContext = input;
            uiChoices.Style = context.GetStyle("Adaptive.Input.AdaptiveChoiceSetInput");

            foreach (var choice in input.Choices)
            {

                if (input.IsMultiSelect == true)
                {
                    var uiCheckbox = new CheckBox();
                    SetContent(uiCheckbox, choice.Title, input.Wrap);
                    uiCheckbox.IsChecked = chosen.Contains(choice.Value);
                    uiCheckbox.DataContext = choice;
                    uiCheckbox.Style = context.GetStyle("Adaptive.Input.AdaptiveChoiceSetInput.CheckBox");
                    uiChoices.Children.Add(uiCheckbox);
                }
                else
                {
                    if (input.Style == AdaptiveChoiceInputStyle.Compact)
                    {
                        var uiComboItem = new ComboBoxItem();
                        uiComboItem.Style = context.GetStyle("Adaptive.Input.AdaptiveChoiceSetInput.ComboBoxItem");
                        SetContent(uiComboItem, choice.Title, input.Wrap);
                        uiComboItem.DataContext = choice;
                        uiComboBox.Items.Add(uiComboItem);

                        // If multiple values are specified, no option is selected
                        if (chosen.Contains(choice.Value) && chosen.Count == 1)
                        {
                            uiComboBox.SelectedItem = uiComboItem;
                        }
                    }
                    else
                    {
                        var uiRadio = new RadioButton();
                        SetContent(uiRadio, choice.Title, input.Wrap);

                        // When isMultiSelect is false, only 1 specified value is accepted.
                        // Otherwise, don't set any option
                        if (chosen.Count == 1)
                        {
                            uiRadio.IsChecked = chosen.Contains(choice.Value);
                        }
                        uiRadio.GroupName = input.Id;
                        uiRadio.DataContext = choice;
                        uiRadio.Style = context.GetStyle("Adaptive.Input.AdaptiveChoiceSetInput.Radio");
                        uiChoices.Children.Add(uiRadio);
                    }
                }
            }
            context.InputBindings.Add(input.Id, () =>
            {
                if (input.IsMultiSelect == true)
                {
                    string values = string.Empty;
                    foreach (var item in uiChoices.Children)
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
                        foreach (var item in uiChoices.Children)
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
            
            if (!input.IsMultiSelect && input.Style == AdaptiveChoiceInputStyle.Compact)
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
        public static void SetContent(ContentControl uiControl, string text, bool wrap)
        { 
            if (wrap)
            {
                uiControl.Content = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap };
            }
            else
            {
                uiControl.Content = text;
            }
        }
    }
}
