﻿using System.Windows;
#if WPF
using System.Windows.Controls;
#elif Xamarin
using Xamarin.Forms;
#endif

namespace AdaptiveCards.Renderers
{
    public partial class XamlRenderer
        : AdaptiveRenderer<FrameworkElement, RenderContext>
    {
        /// <summary>
        /// TextInput
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected override FrameworkElement Render(InputToggle input, RenderContext context)
        {

            if (this.Options.SupportInteraction)
            {
#if WPF
                var uiToggle = new CheckBox();
                uiToggle.Content = input.Title;
                uiToggle.IsChecked = input.Value == (input.ValueOn ?? "true");
                uiToggle.Style = this.GetStyle($"Adaptive.Input.Toggle");
                uiToggle.DataContext = input;
                context.InputControls.Add(uiToggle);
                return uiToggle;
#elif Xamarin
                var uiToggle = new Switch();
                // TODO: Finish switch
                //uiToggle.Content = input.Title;
                uiToggle.IsToggled = input.Value == (input.ValueOn ?? "true");
                uiToggle.Style = this.GetStyle($"Adaptive.Input.Toggle");
                uiToggle.BindingContext = input;
                context.InputControls.Add(uiToggle);
                return uiToggle;
#endif
            }
            else
            {
                Container container = new Container() { Separation = input.Separation };
                container.Items.Add(new TextBlock() { Text = GetFallbackText(input)});
                if (input.Value != null)
                {
                    container.Items.Add(new TextBlock()
                    {
                        Text = (input.Value == (input.ValueOn ?? "true")) ? input.ValueOn ?? "selected" : input.ValueOff ?? "not selected",
                        Color = TextColor.Accent,
                        Wrap = true
                    });
                }
                return Render(container, context);
            }

        }
    }

}