﻿using System.Windows;
#if WPF
using System.Windows.Controls;
#elif Xamarin
using Xamarin.Forms;
using Button = AdaptiveCards.XamarinForms.Renderer.ContentButton;
#endif

namespace AdaptiveCards.Renderers
{

    public partial class XamlRenderer
        : AdaptiveRenderer<FrameworkElement, RenderContext>
    {
        /// <summary>
        /// OpenUrl
        /// </summary>
        /// <param name="openUrlAction"></param>
        /// <returns></returns>
        protected override FrameworkElement Render(ActionOpenUrl action, RenderContext context)
        {
            if (this.Options.SupportInteraction)
            {
                Button uiButton = this.CreateActionButton(action, context); // content);
                uiButton.Click += (sender, e) =>
                {
                    context.Action(uiButton, new ActionEventArgs() { Action = action });
                };
                return uiButton;
            }
            return null;
        }
    }
}