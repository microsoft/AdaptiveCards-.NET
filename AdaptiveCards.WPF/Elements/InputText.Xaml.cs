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
        /// Input.Text
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected override FrameworkElement Render(InputText input, RenderContext context)
        {
            if (this.Options.SupportInteraction)
            {
                var textBox = new TextBox() { Text = input.Value };
                if (input.IsMultiline == true)
                {
                    textBox.AcceptsReturn = true;
#if WPF
                    textBox.TextWrapping = TextWrapping.Wrap;
                    textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
#elif Xamarin 
                    // TODO 
#endif
                }
#if WPF
                if (input.MaxLength > 0)
                    textBox.MaxLength = input.MaxLength;
#elif Xamarin 
                    // TODO 
#endif
                textBox.Text = input.Placeholder;
                textBox.Style = this.GetStyle($"Adaptive.Input.Text.{input.Style}");
                textBox.DataContext = input;
                context.InputControls.Add(textBox);
                return textBox;
            }
            else
            {
                Container container = new Container() { Separation = input.Separation };
                container.Items.Add(new TextBlock() { Text = GetFallbackText(input) ?? input.Placeholder });
                if (input.Value != null)
                {

                    container.Items.Add(new TextBlock()
                    {
                        Text = input.Value,
                        Color = TextColor.Accent,
                        Wrap = true
                    });
                }
                return Render(container, context);
            }

        }
    }
}