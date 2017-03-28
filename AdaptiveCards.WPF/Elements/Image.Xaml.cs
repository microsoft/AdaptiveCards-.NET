﻿using System;
#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using UI = System.Windows.Controls;
#elif Xamarin
using Xamarin.Forms;
using UI = Xamarin.Forms;
using Button = AdaptiveCards.XamarinForms.Renderer.ContentButton;
#endif

namespace AdaptiveCards.Renderers
{
    public partial class XamlRenderer
        : AdaptiveRenderer<FrameworkElement, RenderContext>
    {
        /// <summary>
        /// Image
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        protected override FrameworkElement Render(Image image, RenderContext context)
        {
            var uiImage = new UI.Image();
#if WPF
            uiImage.Source = context.ResolveImageSource(image.Url);
#elif Xamarin
            uiImage.SetSource(new Uri(image.Url));
#endif
            uiImage.SetHorizontalAlignment(image.HorizontalAlignment);

            string style = $"Adaptive.Image";
            if (image.Size != ImageSize.Auto)
                style += $".{image.Size}";

            if (image.Style == ImageStyle.Person)
                style += $".{image.Style}";
            uiImage.Style = this.GetStyle(style);

            if (image.SelectAction != null)
            {
                var uiButton = (Button)RenderAction(image.SelectAction, context);
                if (uiButton != null)
                {
                    uiButton.Content = uiImage;
                    uiButton.Style = this.GetStyle("Adaptive.Action.Tap");
                    return uiButton;
                }
            }
            return uiImage;
        }

    }
}