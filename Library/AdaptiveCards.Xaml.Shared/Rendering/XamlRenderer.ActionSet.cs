﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AdaptiveCards.Rendering;
using System;
#if WPF
using System.Windows.Shapes;
using System.Windows.Controls;
using WPF = System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#elif XAMARIN
using AsNum.XFControls;
using Xamarin.Forms;
using Button = AdaptiveCards.Rendering.ContentButton;
#endif

namespace AdaptiveCards.Rendering
{
    public partial class XamlRenderer
    {
        public static FrameworkElement RenderActionSet(TypedElement element, RenderContext context)
        {
            ActionSet actionSet = (ActionSet)element;
            var uiContainer = new Grid();
            uiContainer.Style = context.GetStyle("Adaptive.ActionSet");

            AddActions(uiContainer, actionSet.Actions, context, context.Options.ActionSet.SupportedActions, context.Options.ActionSet.MaxActions);

            return uiContainer;
        }

        public static void AddActions(Grid uiContainer, List<ActionBase> actions, RenderContext context, string[] supportedActions, int maxActions)
        {

            if (supportedActions != null)
            {
                var actionsToProcess = actions
                    .Where(a => supportedActions?.Contains(a.Type) == true)
                    .Take(maxActions).ToList();
                if (actionsToProcess.Any() == true)
                {
#if WPF
                    var uiActionBar = new UniformGrid();
                    if (context.Options.AdaptiveCard.ActionsOrientation == ActionsOrientation.Horizontal)
                        uiActionBar.Columns = actionsToProcess.Count();
                    else
                        uiActionBar.Rows = actionsToProcess.Count();
                    uiActionBar.HorizontalAlignment = (System.Windows.HorizontalAlignment)Enum.Parse(typeof(System.Windows.HorizontalAlignment), context.Options.AdaptiveCard.ActionAlignment.ToString());
                    uiActionBar.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

                    uiActionBar.Style = context.GetStyle("Adaptive.Actions");
                    uiContainer.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    Grid.SetRow(uiActionBar, uiContainer.RowDefinitions.Count - 1);
                    uiContainer.Children.Add(uiActionBar);

                    if (context.Options.Actions.ShowCard.ActionMode == ShowCardActionMode.Inline && actionsToProcess.Where(a => a is ActionShowCard).Any())
                    {
                        uiContainer.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    }

                    int iPos = 0;
                    List<FrameworkElement> actionBarCards = new List<FrameworkElement>();
                    foreach (var action in actionsToProcess)
                    {
                        // add actions
                        var uiAction = (Button)context.Render(action);
                        if (uiAction != null)
                        {
                            Grid.SetColumn(uiAction, iPos++);
                            uiActionBar.Children.Add(uiAction);

                            if (action is ActionShowCard)
                            {
                                ActionShowCard showCardAction = (ActionShowCard)action;
                                if (context.Options.Actions.ShowCard.ActionMode == ShowCardActionMode.Inline)
                                {
                                    Grid uiShowCardContainer = new Grid();
                                    uiShowCardContainer.Style = context.GetStyle("Adaptive.Actions.ShowCard");
                                    uiShowCardContainer.DataContext = showCardAction;
                                    if (context.Options.Actions.ShowCard.AutoMargin == true)
                                    {
                                        uiShowCardContainer.Margin = new Thickness(context.Options.AdaptiveCard.Padding.Left * -1,
                                            context.Options.Actions.ShowCard.Padding.Top,
                                            context.Options.AdaptiveCard.Padding.Right * -1,
                                            context.Options.AdaptiveCard.Padding.Bottom * -1);
                                    }
                                    else
                                    {
                                        uiShowCardContainer.Margin = new Thickness(context.Options.Actions.ShowCard.Padding.Left,
                                            context.Options.Actions.ShowCard.Padding.Top,
                                            context.Options.Actions.ShowCard.Padding.Right,
                                            context.Options.Actions.ShowCard.Padding.Bottom);
                                    }
                                    uiShowCardContainer.Background = context.GetColorBrush(context.Options.Actions.ShowCard.BackgroundColor);
                                    uiShowCardContainer.Visibility = Visibility.Collapsed;

                                    // render the card
                                    var uiShowCard = context.Render(showCardAction.Card);
                                    ((Grid)uiShowCard).Background = context.GetColorBrush("Transparent");
                                    uiShowCard.Margin = new Thickness(context.Options.Actions.ShowCard.Padding.Left,
                                        context.Options.Actions.ShowCard.Padding.Top,
                                        context.Options.Actions.ShowCard.Padding.Right,
                                        context.Options.Actions.ShowCard.Padding.Bottom);
                                    uiShowCard.DataContext = showCardAction;
                                    uiShowCardContainer.Children.Add(uiShowCard);

                                    actionBarCards.Add(uiShowCardContainer);
                                    Grid.SetRow(uiShowCardContainer, uiContainer.RowDefinitions.Count - 1);
                                    uiContainer.Children.Add(uiShowCardContainer);

                                    uiAction.Click += (sender, e) =>
                                    {
                                        bool showCard = (uiShowCardContainer.Visibility != Visibility.Visible);
                                        foreach (var actionBarCard in actionBarCards)
                                            actionBarCard.Visibility = Visibility.Collapsed;
                                        if (showCard)
                                            uiShowCardContainer.Visibility = Visibility.Visible;
                                    };
                                }
                            }
                        }
                    }

#elif XAMARIN
                    var uiActionBar = new UniformGrid();
                    //uiActionBar.Rows = 1;
                    //uiActionBar.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    //uiActionBar.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

                    int iCol = 0;
                    foreach (var action in actionsToProcess)
                    {
                        // add actions
                        var uiAction = context.Render(action);
                        if (uiAction != null)
                        {
                            Grid.SetColumn(uiAction, iCol++);
                            uiActionBar.Children.Add(uiAction);
                        }
                    }
                    uiActionBar.Style = context.GetStyle("Adaptive.Actions");
                    uiContainer.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    Grid.SetRow(uiActionBar, uiContainer.RowDefinitions.Count - 1);
                    uiContainer.Children.Add(uiActionBar);
#endif
                }
            }
        }
    }
}