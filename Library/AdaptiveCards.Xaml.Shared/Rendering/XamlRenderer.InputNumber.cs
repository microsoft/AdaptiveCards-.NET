﻿using System.Windows;
using AdaptiveCards.Rendering;
#if WPF
using System.Windows.Controls;
#elif XAMARIN
using Xamarin.Forms;
#endif

namespace AdaptiveCards.Rendering
{
    public partial class XamlRenderer
        : AdaptiveRenderer<FrameworkElement, RenderContext>
    {

        /// <summary>
        /// Input.Number
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected override FrameworkElement Render(InputNumber input, RenderContext context)
        {
            if (this.Options.SupportInteraction)
            {
                var textBox = new TextBox() { Text = input.Value.ToString() };
                textBox.Text = input.Placeholder;
                textBox.Style = this.GetStyle($"Adaptive.Input.Text.Number");
                textBox.DataContext = input;
                context.InputControls.Add(textBox);
                return textBox;
            }
            else
            {
                Container container = new Container() { Separation = input.Separation };
                container.Items.Add(new TextBlock() { Text = GetFallbackText(input) ?? input.Placeholder });
                if (!double.IsNaN(input.Value))
                {
                    container.Items.Add(new TextBlock()
                    {
                        Text = input.Value.ToString(),
                        Color = TextColor.Accent,
                        Wrap = true
                    });
                }
                return Render(container, context);
            }
        }
    }
}