﻿using System.Windows.Controls;
using System.Windows.Media;

namespace AdaptiveCards.Rendering.Wpf
{
    public static class ImageExtensions
    {
        public static void SetSource(this Image image, string url, AdaptiveRenderContext context)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;
            image.Source = context.ResolveImageSource(url);
        }

        public static void SetBackgroundSource(this Grid grid, string url, AdaptiveRenderContext context)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;
            grid.Background = new ImageBrush(context.ResolveImageSource(url));
        }

        public static void SetImageProperties(this Image imageview, AdaptiveImage image, AdaptiveRenderContext context)
        {
            switch (image.Size)
            {
                case AdaptiveImageSize.Auto:
                    imageview.Stretch = System.Windows.Media.Stretch.UniformToFill;
                    break;
                case AdaptiveImageSize.Stretch:
                    imageview.Stretch = System.Windows.Media.Stretch.Uniform;
                    break;
                case AdaptiveImageSize.Small:
                    imageview.Width = context.Config.ImageSizes.Small;
                    imageview.Height = context.Config.ImageSizes.Small;
                    break;
                case AdaptiveImageSize.Medium:
                    imageview.Width = context.Config.ImageSizes.Medium;
                    imageview.Height = context.Config.ImageSizes.Medium;
                    break;
                case AdaptiveImageSize.Large:
                    imageview.Width = context.Config.ImageSizes.Large;
                    imageview.Height = context.Config.ImageSizes.Large;
                    break;
            }
        }
    }
}
