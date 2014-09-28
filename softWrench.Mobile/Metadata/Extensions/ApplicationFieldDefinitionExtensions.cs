using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.Mobile.Metadata.Applications.UI;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.UI;

namespace softWrench.Mobile.Metadata.Extensions {
    internal static class ApplicationFieldDefinitionExtensions {
        private const string WidgetConst = "widget";

        public static IWidget Widget(this ApplicationFieldDefinition definition) {
            var widget = definition.WidgetDefinition;
            var extensionParam = definition.ExtensionParameter(WidgetConst);
            if (extensionParam != null) {
                return (IWidget)extensionParam;
            }
            IWidget iWidget = CreateIWidget(widget, definition.RendererType);
            definition.ExtensionParameter(WidgetConst, iWidget);
            return iWidget;
        }

        private static IWidget CreateIWidget(IWidgetDefinition widget, string rendererType) {
            if (widget is DateWidgetDefinition) {
                return new DateWidget((DateWidgetDefinition)widget);
            }
            if (widget is HiddenWidgetDefinition) {
                return new HiddenWidget((HiddenWidgetDefinition)widget);
            } //TODO: implement <image> widget parsing
            if (rendererType == "image") {
                return new ImageWidget(new ImageWidgetDefinition());
            }
            if (widget is TextWidgetDefinition) {
                return new TextWidget((TextWidgetDefinition)widget);
            }
            if (widget is NumberWidgetDefinition) {
                return new NumberWidget((NumberWidgetDefinition)widget);
            }
            if (widget is LookupWidgetDefinition) {
                return new LookupWidget((LookupWidgetDefinition)widget);
            }
            if (widget is ImageWidgetDefinition) {
                return new ImageWidget((ImageWidgetDefinition)widget);
            } 
            return null;
        }
    }
}
