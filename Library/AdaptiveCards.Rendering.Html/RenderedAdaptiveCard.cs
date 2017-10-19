﻿namespace AdaptiveCards.Rendering.Html
{
    /// <summary>
    /// The rendered Adaptive Card result.
    /// </summary>
    public class RenderedAdaptiveCard : RenderedAdaptiveCardBase
    {
        public RenderedAdaptiveCard(HtmlTag htmlTag, AdaptiveCard originatingCard)
            : base(originatingCard: originatingCard)
        {
            Html = htmlTag;
        }

        /// <summary>
        /// The rendered result. If there were errors present, this will be null.
        /// </summary>
        public HtmlTag Html { get; private set; }
    }
}
