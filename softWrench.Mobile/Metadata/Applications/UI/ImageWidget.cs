using System;
using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.UI;

namespace softWrench.Mobile.Metadata.Applications.UI 
{
    public sealed class ImageWidget : ImageWidgetDefinition, IWidget
    {
        private readonly ImageWidgetDefinition _definition;

        public ImageWidget(ImageWidgetDefinition definition) : base()
        {
            _definition = definition;
        }

        public string Format(string value)
        {
            return value;
        }

        public bool Validate(string value, out string error)
        {
            error = null;
            return true;
        }
    }
}
