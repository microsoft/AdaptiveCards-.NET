using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// This class contais all proterties that are used for rendering and need to be passed down between parent and child elements 
namespace AdaptiveCards.Rendering
{
    public enum ColumnPositionEnum { Begin, Intermediate, End, Only };

    public class AdaptiveRenderArgs
    {
        public AdaptiveRenderArgs() { }

        public AdaptiveRenderArgs(AdaptiveRenderArgs previousRenderArgs)
        {
            ParentStyle = previousRenderArgs.ParentStyle;
            ForegroundColors = previousRenderArgs.ForegroundColors;
            ColumnRelativePosition = previousRenderArgs.ColumnRelativePosition;
            HasParentWithPadding = previousRenderArgs.HasParentWithPadding;
        }

        public AdaptiveContainerStyle ParentStyle { get; set; } = AdaptiveContainerStyle.Default;

        public ForegroundColorsConfig ForegroundColors { get; set; }
        
        public ColumnPositionEnum ColumnRelativePosition { get; set; }

        public bool HasParentWithPadding { get; set; } = true;

    }
}
