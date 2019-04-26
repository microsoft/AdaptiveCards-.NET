// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using AdaptiveCards;
using AdaptiveCards.Rendering;
using AdaptiveCards.Rendering.Wpf;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Speech.Synthesis;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Windows.Media;

namespace WpfVisualizer
{
    public partial class MainWindow : Window
    {
        private bool _dirty;
        private readonly SpeechSynthesizer _synth;
        private DocumentLine _errorLine;

        public MainWindow()
        {
            foreach (var type in typeof(AdaptiveHostConfig).Assembly.GetExportedTypes()
                .Where(t => t.Namespace == typeof(AdaptiveHostConfig).Namespace))
                TypeDescriptor.AddAttributes(type, new ExpandableObjectAttribute());

            InitializeComponent();

            _synth = new SpeechSynthesizer();
            _synth.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
            _synth.SetOutputToDefaultAudioDevice();
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += Timer_Tick;
            timer.Start();

            foreach (var config in Directory.GetFiles(@"..\..\..\..\..\..\samples\HostConfig", "*.json"))
            {
                hostConfigs.Items.Add(new ComboBoxItem
                {
                    Content = Path.GetFileNameWithoutExtension(config),
                    Tag = config
                });
            }


            Renderer = new AdaptiveCardRenderer()
            {
                Resources = Resources
            };

            Renderer.FeatureRegistration.Set("acTest", "1.0");

            // Use the Xceed rich input controls
            Renderer.UseXceedElementRenderers();

            // Register custom elements and actions
            // TODO: Change to instance property? Change to UWP parser registration
            AdaptiveTypedElementConverter.RegisterTypedElement<MyCustomRating>();
            AdaptiveTypedElementConverter.RegisterTypedElement<MyCustomAction>();

            Renderer.ElementRenderers.Set<MyCustomRating>(MyCustomRating.Render);

            // This seems unecessary?
            Renderer.ActionHandlers.AddSupportedAction<MyCustomAction>();
        }

        public AdaptiveCardRenderer Renderer { get; set; }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_dirty)
            {
                _dirty = false;
                RenderCard();
            }
        }

        private void RenderCard()
        {
            cardError.Children.Clear();
            cardGrid.Children.Clear();

            try
            {

                AdaptiveCardParseResult parseResult = AdaptiveCard.FromJson(textBox.Text);

                AdaptiveCard card = parseResult.Card;

                /*
                // Example on how to override the Action Positive and Destructive styles
                Style positiveStyle = new Style(typeof(Button));
                positiveStyle.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.Green));
                Style otherStyle = new Style(typeof(Button));
                otherStyle.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.Yellow));
                otherStyle.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.Red));

                Renderer.Resources.Add("Adaptive.Action.Submit.positive", positiveStyle);
                Renderer.Resources.Add("Adaptive.Action.Submit.other", otherStyle);
                */

                RenderedAdaptiveCard renderedCard = Renderer.RenderCard(card);
                // TODO: should we have an option to render fallback card instead of exception?

                // Wire up click handler
                renderedCard.OnAction += OnAction;

                renderedCard.OnMediaClicked += OnMediaClick;

                cardGrid.Children.Add(renderedCard.FrameworkElement);

                // Report any warnings
                var allWarnings = parseResult.Warnings.Union(renderedCard.Warnings);
                foreach (var warning in allWarnings)
                {
                    ShowWarning(warning.Message);
                }
            }
            catch (AdaptiveRenderException ex)
            {
                var fallbackCard = new TextBlock
                {
                    Text = ex.CardFallbackText ?? "Sorry, we couldn't render the card"
                };

                cardGrid.Children.Add(fallbackCard);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void OnAction(RenderedAdaptiveCard sender, AdaptiveActionEventArgs e)
        {
            if (e.Action is AdaptiveOpenUrlAction openUrlAction)
            {
                Process.Start(openUrlAction.Url.AbsoluteUri);
            }
            else if (e.Action is AdaptiveShowCardAction showCardAction)
            {
                // Action.ShowCard can be rendered inline automatically, or in "popup" mode
                // If the Host Config is set to Popup mode, then the app needs to show it
                if (Renderer.HostConfig.Actions.ShowCard.ActionMode == ShowCardActionMode.Popup)
                {
                    var dialog = new ShowCardWindow(showCardAction.Title, showCardAction, Resources);
                    dialog.Owner = this;
                    dialog.ShowDialog();
                }
            }
            else if (e.Action is AdaptiveSubmitAction submitAction)
            {
                var inputs = sender.UserInputs.AsJson();

                // Merge the Action.Submit Data property with the inputs
                inputs.Merge(submitAction.Data);

                MessageBox.Show(this, JsonConvert.SerializeObject(inputs, Formatting.Indented), "SubmitAction");
            }
        }

        private void OnMediaClick(RenderedAdaptiveCard sender, AdaptiveMediaEventArgs e)
        {
            MessageBox.Show(this, JsonConvert.SerializeObject(e.Media), "Host received a Media");
        }

        private void ShowWarning(string message)
        {
            var textBlock = new TextBlock
            {
                Text = "WARNING: " + message,
                TextWrapping = TextWrapping.Wrap,
                Style = Resources["Warning"] as Style
            };
            var button = new Button { Content = textBlock };
            cardError.Children.Add(button);
        }

        private void ShowError(Exception err)
        {
            var textBlock = new TextBlock
            {
                Text = "ERROR: " + err.Message,
                TextWrapping = TextWrapping.Wrap,
                Style = Resources["Error"] as Style
            };
            var button = new Button { Content = textBlock };
            button.Click += Button_Click;
            cardError.Children.Add(button);

            var iPos = err.Message.IndexOf("line ");
            if (iPos > 0)
            {
                iPos += 5;
                var iEnd = err.Message.IndexOf(",", iPos);

                var line = 1;
                if (int.TryParse(err.Message.Substring(iPos, iEnd - iPos), out line))
                {
                    if (line == 0) line = 1;
                    iPos = err.Message.IndexOf("position ");
                    if (iPos > 0)
                    {
                        iPos += 9;
                        iEnd = err.Message.IndexOf(".", iPos);
                        var position = 0;
                        if (int.TryParse(err.Message.Substring(iPos, iEnd - iPos), out position))
                            _errorLine = textBox.Document.GetLineByNumber(Math.Min(line, textBox.Document.LineCount));
                    }
                }
            }
        }

        private void _OnMissingInput(object sender, MissingInputEventArgs args)
        {
            MessageBox.Show("Required input is missing.");
            args.FrameworkElement.Focus();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_errorLine != null)
                textBox.Select(_errorLine.Offset, _errorLine.Length);
        }

        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.DefaultExt = ".json";
            dlg.Filter = "Json documents (*.json)|*.json";
            var result = dlg.ShowDialog();
            if (result == true)
            {
                textBox.Text = File.ReadAllText(dlg.FileName).Replace("\t", "  ");
                _dirty = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var binding = new CommandBinding(NavigationCommands.GoToPage, GoToPage, CanGoToPage);
            // Register CommandBinding for all windows.
            CommandManager.RegisterClassCommandBinding(typeof(Window), binding);
        }


        private void GoToPage(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is string)
            {
                var name = e.Parameter as string;
                if (!string.IsNullOrWhiteSpace(name))
                    Process.Start(name);
            }
        }

        private void CanGoToPage(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private async void viewImage_Click(object sender, RoutedEventArgs e)
        {
            //Disable interactivity to remove inputs and actions from the image
            var supportsInteractivity = Renderer.HostConfig.SupportsInteractivity;
            Renderer.HostConfig.SupportsInteractivity = false;

            var renderedCard = await Renderer.RenderCardToImageAsync(AdaptiveCard.FromJson(textBox.Text).Card, false);
            Renderer.HostConfig.SupportsInteractivity = supportsInteractivity;

            var path = Path.GetRandomFileName() + ".png";
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await renderedCard.ImageStream.CopyToAsync(fileStream);
            }
            Process.Start(path);
        }

        private void speak_Click(object sender, RoutedEventArgs e)
        {
            var result = AdaptiveCard.FromJson(textBox.Text);
            var card = result.Card;

            _synth.SpeakAsyncCancelAll();
            if (card.Speak != null)
            {
                _synth.SpeakSsmlAsync(FixSSML(card.Speak));
            }
        }

        private string FixSSML(string speak)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<speak version=\"1.0\"");
            sb.AppendLine(" xmlns =\"http://www.w3.org/2001/10/synthesis\"");
            sb.AppendLine(" xml:lang=\"en-US\">");
            sb.AppendLine(speak);
            sb.AppendLine("</speak>");
            return sb.ToString();
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            _dirty = true;
        }

        private void toggleOptions_Click(object sender, RoutedEventArgs e)
        {
            hostConfigEditor.Visibility = hostConfigEditor.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        public AdaptiveHostConfig HostConfig
        {
            get => Renderer.HostConfig;
            set
            {
                hostConfigerror.Children.Clear();
                Renderer.HostConfig = value;
                _dirty = true;
                if (value != null)
                {
                    var props = value.GetType()
                        .GetRuntimeProperties()
                        .Where(p => typeof(AdaptiveConfigBase).IsAssignableFrom(p.PropertyType));

                    foreach (var x in value.AdditionalData)
                    {
                        var textBlock = new TextBlock
                        {
                            Text = $"Unknown property {x.Key}",
                            TextWrapping = TextWrapping.Wrap,
                            Style = Resources["Warning"] as Style
                        };
                        hostConfigerror.Children.Add(textBlock);
                    }
                }
            }
        }

        private void hostConfigs_Selected(object sender, RoutedEventArgs e)
        {
            HostConfig = AdaptiveHostConfig.FromJson(File.ReadAllText((string)((ComboBoxItem)hostConfigs.SelectedItem).Tag));
            hostConfigEditor.SelectedObject = Renderer.HostConfig;
        }

        private void loadConfig_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.DefaultExt = ".json";
            dlg.Filter = "Json documents (*.json)|*.json";
            var result = dlg.ShowDialog();
            if (result == true)
            {
                HostConfig = AdaptiveHostConfig.FromJson(File.ReadAllText(dlg.FileName));
            }
        }

        private void saveConfig_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.DefaultExt = ".json";
            dlg.Filter = "Json documents (*.json)|*.json";
            var result = dlg.ShowDialog();
            if (result == true)
            {
                var json = JsonConvert.SerializeObject(Renderer.HostConfig, Formatting.Indented);
                File.WriteAllText(dlg.FileName, json);
            }
        }

        private void HostConfigEditor_OnPropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            _dirty = true;
        }
    }
}
