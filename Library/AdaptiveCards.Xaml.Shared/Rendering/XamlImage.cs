﻿using System;
using AdaptiveCards.Rendering;
#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UI = System.Windows.Controls;
#elif XAMARIN
using Xamarin.Forms;
using UI = Xamarin.Forms;
using Button = AdaptiveCards.Rendering.ContentButton;
#endif

namespace AdaptiveCards.Rendering
{
    public class XamlImage : Image, IRender<FrameworkElement, RenderContext>
    {
        /// <summary>
        /// Image
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public FrameworkElement Render(RenderContext context)
        {
            var uiImage = new UI.Image();
#if WPF
            uiImage.Source = context.ResolveImageSource(this.Url);
#elif XAMARIN
            uiImage.SetSource(new Uri(this.Url));
#endif
            uiImage.SetHorizontalAlignment(this.HorizontalAlignment);


            string style = $"Adaptive.{this.Type}";
            if (this.Style == ImageStyle.Person)
            {
                style += $".{this.Style}";
#if WPF
                var mask = new RadialGradientBrush()
                {
                    GradientOrigin = new Point(0.5, 0.5),
                    Center = new Point(0.5, 0.5),
                    RadiusX = 0.5,
                    RadiusY = 0.5,
                    GradientStops = new GradientStopCollection()
                };
                mask.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#ffffffff"), .9));
                mask.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#00ffffff"), 1.0));
                uiImage.OpacityMask = mask;
#elif XAMARIN
                //TODO
#endif 
            }
            uiImage.Style = context.GetStyle(style);
#if WPF
            switch (this.Size)
            {
                case ImageSize.Auto:
                    uiImage.Stretch = System.Windows.Media.Stretch.UniformToFill;
                    break;
                case ImageSize.Stretch:
                    uiImage.Stretch = System.Windows.Media.Stretch.Uniform;
                    break;
                case ImageSize.Small:
                    uiImage.Width = context.Options.Image.Size.Small;
                    uiImage.Height = context.Options.Image.Size.Small;
                    break;
                case ImageSize.Medium:
                    uiImage.Width = context.Options.Image.Size.Medium;
                    uiImage.Height = context.Options.Image.Size.Medium;
                    break;
                case ImageSize.Large:
                    uiImage.Width = context.Options.Image.Size.Large;
                    uiImage.Height = context.Options.Image.Size.Large;
                    break;
            }
#elif XAMARIN
            // TODO
#endif

            if (this.SelectAction != null)
            {
                var uiButton = (Button)context.Render(this.SelectAction);
                if (uiButton != null)
                {
                    uiButton.Content = uiImage;
                    uiButton.Style = context.GetStyle("Adaptive.Action.Tap");
                    return uiButton;
                }
            }
            return uiImage;
        }

    }
}