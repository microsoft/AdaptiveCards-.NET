﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
#if WPF
using System.Windows.Controls;
using System.Windows.Shapes;
#elif Xamarin
using Xamarin.Forms;
#endif

namespace AdaptiveCards.Renderers
{
    public partial class XamlRenderer
        : AdaptiveRenderer<FrameworkElement, RenderContext>
    {

        /// <summary>
        /// ColumnSet
        /// </summary>
        /// <param name="columnSet"></param>
        /// <returns></returns>
        protected override FrameworkElement Render(ColumnSet columnSet, RenderContext context)
        {
            var uiColumnSet = new Grid();
            uiColumnSet.Style = this.GetStyle("Adaptive.ColumnSet");

            foreach (var column in columnSet.Columns)
            {
                // Add vertical Seperator
                if (uiColumnSet.ColumnDefinitions.Count > 0)
                {
                    switch (column.Separation)
                    {
                        case SeparationStyle.None:
                            break;

                        case SeparationStyle.Default:
                            {
                                var sep = new Grid();
                                sep.Style = this.GetStyle($"Adaptive.VerticalSeparator");
                                uiColumnSet.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                                Grid.SetColumn(sep, uiColumnSet.ColumnDefinitions.Count - 1);
                                uiColumnSet.Children.Add(sep);
                            }
                            break;

                        case SeparationStyle.Strong:
                            {
                                var sep = new Grid();
#if WPF
                                sep.VerticalAlignment = VerticalAlignment.Stretch;
#elif Xamarin
                                // TOOD: check xamarin separator visual
                                //sep.VerticalAlignment = VerticalAlignment.Stretch;
#endif
                                sep.Style = this.GetStyle($"Adaptive.VerticalSeparator.Strong");
                                uiColumnSet.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                                Grid.SetColumn(sep, uiColumnSet.ColumnDefinitions.Count - 1);
                                uiColumnSet.Children.Add(sep);
                            }
                            break;
                    }
                }

                FrameworkElement uiElement = this.Render(column, context);

                // do some sizing magic using the magic GridUnitType.Star
                var size = column.Size?.ToLower();
                if (size == null || size == ColumnSize.Stretch.ToLower())
                    uiColumnSet.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                else if (size == ColumnSize.Auto.ToLower())
                    uiColumnSet.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                else
                {
                    double val;
                    if (double.TryParse(size, out val))
                        uiColumnSet.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(val, GridUnitType.Star) });
                    else
                        uiColumnSet.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                }

                Grid.SetColumn(uiElement, uiColumnSet.ColumnDefinitions.Count - 1);
                uiColumnSet.Children.Add(uiElement);
            }

            return uiColumnSet;
        }

    }
}