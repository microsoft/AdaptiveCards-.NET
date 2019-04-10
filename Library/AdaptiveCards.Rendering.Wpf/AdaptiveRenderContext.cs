using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AdaptiveCards.Rendering.Wpf
{
    /// <summary>
    /// Context state for a render pass
    /// </summary>
    public class AdaptiveRenderContext
    {
        private readonly Dictionary<string, SolidColorBrush> _colors = new Dictionary<string, SolidColorBrush>();

        public List<Task> AssetTasks { get; } = new List<Task>();

        public AdaptiveRenderContext(Action<object, AdaptiveActionEventArgs> actionCallback,
            Action<object, MissingInputEventArgs> missingDataCallback,
            Action<object, AdaptiveMediaEventArgs> mediaClickCallback)
        {
            if (actionCallback != null)
                OnAction += (obj, args) => actionCallback(obj, args);

            if (missingDataCallback != null)
                OnMissingInput += (obj, args) => missingDataCallback(obj, args);

            if (mediaClickCallback != null)
                OnMediaClick += (obj, args) => mediaClickCallback(obj, args);
        }

        public AdaptiveHostConfig Config { get; set; } = new AdaptiveHostConfig();

        public IList<AdaptiveWarning> Warnings { get; } = new List<AdaptiveWarning>();

        public AdaptiveElementRenderers<FrameworkElement, AdaptiveRenderContext> ElementRenderers { get; set; }

        public ResourceDictionary Resources { get; set; }

        public AdaptiveActionHandlers ActionHandlers { get; set; }

        public ResourceResolver ResourceResolvers { get; set; }

        public bool IsRenderingSelectAction { get; set; }

        public IDictionary<Uri, MemoryStream> CardAssets { get; set; } = new Dictionary<Uri, MemoryStream>();

        public IDictionary<string, Func<string>> InputBindings = new Dictionary<string, Func<string>>();

        public event EventHandler<AdaptiveActionEventArgs> OnAction;

        public event EventHandler<AdaptiveMediaEventArgs> OnMediaClick;

        /// <summary>
        /// Event fires when missing input for submit/http actions
        /// </summary>
        public event EventHandler<MissingInputEventArgs> OnMissingInput;

        public void InvokeAction(FrameworkElement ui, AdaptiveActionEventArgs args)
        {
            // ToggleVisibility is a renderer-handled action
            if (args.Action is AdaptiveToggleVisibilityAction toggleVisibilityAction)
            {
                ToggleVisibility(toggleVisibilityAction.TargetElements);
                return;
            }
            else if (args.Action is AdaptiveShowCardAction
                && Config.Actions.ShowCard.ActionMode == ShowCardActionMode.Inline)
            {
                ToggleShowCardVisibility((Button)ui);
                return;
            }

            OnAction?.Invoke(ui, args);
        }

        public void MissingInput(AdaptiveAction sender, MissingInputEventArgs args)
        {
            OnMissingInput?.Invoke(sender, args);
        }

        public void ClickMedia(FrameworkElement ui, AdaptiveMediaEventArgs args)
        {
            OnMediaClick?.Invoke(ui, args);
        }

        /// <summary>
        /// All remote assets should be resolved through this method for tracking
        /// </summary>
        public async Task<BitmapImage> ResolveImageSource(Uri url)
        {
            var completeTask = new TaskCompletionSource<object>();
            AssetTasks.Add(completeTask.Task);

            try
            {
                // Load the stream from the pre-populated CardAssets or try to load from the ResourceResolver
                var streamTask = CardAssets.TryGetValue(url, out var s) ? Task.FromResult(s) : ResourceResolvers.LoadAssetAsync(url);

                Debug.WriteLine($"ASSETS: Starting asset down task for {url}");

                var source = new BitmapImage();

                var stream = await streamTask;
                if (stream != null)
                {
                    stream.Position = 0;
                    source.BeginInit();
                    source.CacheOption = BitmapCacheOption.OnLoad;
                    source.StreamSource = stream;
                    source.EndInit();
                    Debug.WriteLine($"ASSETS: Finished loading asset for {url} ({stream.Length} bytes)");
                }
                completeTask.SetResult(new object());
                return source;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"ASSETS: Failed to load asset for {url}. {e.Message}");
                completeTask.SetException(e);
                return null;
            }
        }

        public SolidColorBrush GetColorBrush(string color)
        {
            lock (_colors)
            {
                if (_colors.TryGetValue(color, out var brush))
                    return brush;
                brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
                _colors[color] = brush;
                return brush;
            }
        }

        // Flag to distinuish the main card and action show cards
        public int CardDepth = 0;

        public IDictionary<Button, FrameworkElement> ActionShowCards = new Dictionary<Button, FrameworkElement>();

        public virtual Style GetStyle(string styleName)
        {
            while (!String.IsNullOrEmpty(styleName))
            {
                Style style = this.Resources.TryGetValue<Style>(styleName);
                if (style != null)
                    return style;
                var iPos = styleName.LastIndexOf('.');
                if (iPos <= 0)
                    break;
                styleName = styleName.Substring(0, iPos);
            }

            // Debug.WriteLine($"Unable to find Style {styleName} from the supplied ResourceDictionary");
            return null;
        }

        /// <summary>
        /// Helper to deal with casting
        /// </summary>
        public FrameworkElement Render(AdaptiveTypedElement element)
        {
            // Inputs should render read-only if interactivity is false
            if (!Config.SupportsInteractivity && element is AdaptiveInput input)
            {
                var tb = AdaptiveTypedElementConverter.CreateElement<AdaptiveTextBlock>();
                tb.Text = input.GetNonInteractiveValue() ?? "*[Input]*";
                tb.Color = AdaptiveTextColor.Accent;
                tb.Wrap = true;
                Warnings.Add(new AdaptiveWarning(-1, $"Rendering non-interactive input element '{element.Type}'"));
                return Render(tb);
            }

            var renderer = ElementRenderers.Get(element.GetType());
            if (renderer != null)
            {
                // Increment card depth before rendering the inner card
                if (element is AdaptiveCard)
                {
                    CardDepth += 1;
                }

                var rendered = renderer.Invoke(element, this);

                if (!String.IsNullOrEmpty(element.Id))
                {
                    rendered.Name = element.Id;
                }

                // Decrement card depth after inner card is rendered
                if (element is AdaptiveCard)
                {
                    CardDepth -= 1;
                }

                return rendered;
            }

            Warnings.Add(new AdaptiveWarning(-1, $"No renderer for element '{element.Type}'"));
            return null;
        }

        public string Lang { get; set; }

        public FrameworkElement CardRoot { get; set; }

        public AdaptiveRenderArgs RenderArgs { get; set; }
        public FontColorConfig GetForegroundColors(AdaptiveTextColor textColor)
        {
            switch (textColor)
            {
                case AdaptiveTextColor.Accent:
                    return RenderArgs.ForegroundColors.Accent;
                case AdaptiveTextColor.Attention:
                    return RenderArgs.ForegroundColors.Attention;
                case AdaptiveTextColor.Dark:
                    return RenderArgs.ForegroundColors.Dark;
                case AdaptiveTextColor.Good:
                    return RenderArgs.ForegroundColors.Good;
                case AdaptiveTextColor.Light:
                    return RenderArgs.ForegroundColors.Light;
                case AdaptiveTextColor.Warning:
                    return RenderArgs.ForegroundColors.Warning;
                case AdaptiveTextColor.Default:
                default:
                    return RenderArgs.ForegroundColors.Default;
            }
        }

        public void ToggleVisibility(IEnumerable<AdaptiveTargetElement> targetElements)
        {
            foreach (AdaptiveTargetElement targetElement in targetElements)
            {
                var element = LogicalTreeHelper.FindLogicalNode(CardRoot, targetElement.ElementId);

                if (element != null && element is FrameworkElement elementFrameworkElement)
                {
                    Visibility visibility = elementFrameworkElement.Visibility;
                    // if we read something with the format {"elementId": <id>", "isVisible": true} or we just read the id and the element is not visible
                    if ((targetElement.IsVisible.HasValue && targetElement.IsVisible.Value) || (!targetElement.IsVisible.HasValue && visibility != Visibility.Visible))
                    {
                        elementFrameworkElement.Visibility = Visibility.Visible;
                    }
                    // otherwise if we read something with the format {"elementId": <id>", "isVisible": false} or we just read the id and the element is visible
                    else if ((targetElement.IsVisible.HasValue && !targetElement.IsVisible.Value) || (!targetElement.IsVisible.HasValue && visibility == Visibility.Visible))
                    {
                        elementFrameworkElement.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        public void ToggleShowCardVisibility(Button uiAction)
        {
            FrameworkElement card = ActionShowCards[uiAction];
            if (card != null)
            {
                if (card.Visibility != Visibility.Visible)
                {
                    card.Visibility = Visibility.Visible;
                }
                else
                {
                    card.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
