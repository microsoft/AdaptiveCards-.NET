using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdaptiveCards.Rendering.Html.Test
{
    [TestClass]
    public class HtmlRendererTests
    {
        [TestMethod]
        public void TextBlockRender_ParagraphElementStylesAdded()
        {
            var renderContext = new AdaptiveRenderContext(
                new AdaptiveHostConfig(),
                new AdaptiveElementRenderers<HtmlTag, AdaptiveRenderContext>());

            var textBlock = new AdaptiveTextBlock
            {
                Text = "first line\n\nsecond line",
            };

            var generatedHtml = TestHtmlRenderer.CallTextBlockRender(textBlock, renderContext).ToString();

            // From String

            // Generated HTML should have two <p> tags, with appropriate styles set.
            Assert.AreEqual(
                "<div class='ac-textblock' style='box-sizing: border-box;text-align: left;color: rgba(0, 0, 0, 1.00);line-height: 18.62px;font-size: 14px;font-weight: 400;white-space: nowrap;'><p style='margin-top: 0px;margin-bottom: 0px;width: 100%;text-overflow: ellipsis;overflow: hidden;'>first line</p><p style='margin-top: 0px;margin-bottom: 0px;width: 100%;text-overflow: ellipsis;overflow: hidden;'>second line</p></div>",
                generatedHtml);
        }

        [TestMethod]
        public void ContainerStyleForegroundColors()
        {
            var hostConfig = new AdaptiveHostConfig();
            hostConfig.ContainerStyles.Emphasis.ForegroundColors = new ForegroundColorsConfig()
            {
                Default = new FontColorConfig("#FFcc3300")
            };

            var card = new AdaptiveCard("1.2")
            {
                Body = new System.Collections.Generic.List<AdaptiveElement>()
                {
                    new AdaptiveContainer()
                    {
                        Style = AdaptiveContainerStyle.Emphasis,
                        Items = new System.Collections.Generic.List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Text = "container 1 -- emphasis style text"
                            },
                            new AdaptiveContainer()
                            {
                                Style = AdaptiveContainerStyle.Accent,
                                Items = new System.Collections.Generic.List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text = "container 1.1 -- accent style text"
                                    }
                                }
                            },
                            new AdaptiveTextBlock()
                            {
                                Text = "container 1 -- emphasis style text"
                            }
                        }
                    },
                    new AdaptiveTextBlock()
                    {
                        Text = "default style text"
                    }
                }
            };

            var renderer = new AdaptiveCardRenderer(hostConfig);
            var result = renderer.RenderCard(card);
            var generatedHtml = result.Html.ToString();

            // Generated HTML should have two <p> tags, with appropriate styles set.
            Assert.AreEqual(
                "<div class='ac-adaptivecard' style='width: 100%;background-color: rgba(255, 255, 255, 1.00);padding: 15px;box-sizing: border-box;justify-content: flex-start;'><div class='ac-container' style='margin-right: 15px;margin-left: 15px;background-color: rgba(0, 0, 0, 0.03);justify-content: flex-start;'><div class='ac-textblock' style='box-sizing: border-box;text-align: left;color: rgba(204, 51, 0, 1.00);line-height: 18.62px;font-size: 14px;font-weight: 400;white-space: nowrap;'><p style='margin-top: 0px;margin-bottom: 0px;width: 100%;text-overflow: ellipsis;overflow: hidden;'>container 1 -- emphasis style text</p></div><div class='ac-separator' style='height: 8px;'></div><div class='ac-container' style='margin-right: 15px;margin-left: 15px;background-color: #dce5f7;justify-content: flex-start;'><div class='ac-textblock' style='box-sizing: border-box;text-align: left;color: rgba(0, 0, 0, 1.00);line-height: 18.62px;font-size: 14px;font-weight: 400;white-space: nowrap;'><p style='margin-top: 0px;margin-bottom: 0px;width: 100%;text-overflow: ellipsis;overflow: hidden;'>container 1.1 -- accent style text</p></div></div><div class='ac-separator' style='height: 8px;'></div><div class='ac-textblock' style='box-sizing: border-box;text-align: left;color: rgba(204, 51, 0, 1.00);line-height: 18.62px;font-size: 14px;font-weight: 400;white-space: nowrap;'><p style='margin-top: 0px;margin-bottom: 0px;width: 100%;text-overflow: ellipsis;overflow: hidden;'>container 1 -- emphasis style text</p></div></div><div class='ac-separator' style='height: 8px;'></div><div class='ac-textblock' style='box-sizing: border-box;text-align: left;color: rgba(0, 0, 0, 1.00);line-height: 18.62px;font-size: 14px;font-weight: 400;white-space: nowrap;'><p style='margin-top: 0px;margin-bottom: 0px;width: 100%;text-overflow: ellipsis;overflow: hidden;'>default style text</p></div></div>",
                generatedHtml);
        }

        private class TestHtmlRenderer : AdaptiveCardRenderer
        {
            public TestHtmlRenderer(AdaptiveHostConfig config)
                : base(config)
            {
            }

            public static HtmlTag CallTextBlockRender(AdaptiveTextBlock element, AdaptiveRenderContext context)
            {
                return TextBlockRender(element, context);
            }

            public static HtmlTag CallChoiceSetInputRender(AdaptiveChoiceSetInput element, AdaptiveRenderContext context)
            {
                return ChoiceSetRender(element, context);
            }
        }

        [TestMethod]
        public void ChoiceSetInput()
        {
            var renderContext = new AdaptiveRenderContext(
                new AdaptiveHostConfig(),
                new AdaptiveElementRenderers<HtmlTag, AdaptiveRenderContext>());

            var dropdownList = new AdaptiveChoiceSetInput()
            {
                Id = "1",
                Value = "1,3",
                Style = AdaptiveChoiceInputStyle.Compact,
                Choices =
                {
                    new AdaptiveChoice()
                    {
                        Title = "Value 1",
                        Value = "1"
                    },
                    new AdaptiveChoice()
                    {
                        Title = "Value 2",
                        Value = "2"
                    },
                    new AdaptiveChoice()
                    {
                        Title = "Value 3",
                        Value = "3"
                    }
                }
            };

            var dropdownGeneratedHtml = TestHtmlRenderer.CallChoiceSetInputRender(dropdownList, renderContext).ToString();

            // Generated HTML should have an additional disabled and hidden option which is selected.
            Assert.AreEqual(
                "<select class='ac-input ac-multichoiceInput' name='1' style='width: 100%;'><option disabled='' hidden='' selected=''/><option value='1'>Value 1</option><option value='2'>Value 2</option><option value='3'>Value 3</option></select>",
                dropdownGeneratedHtml);
        }
    }
}
