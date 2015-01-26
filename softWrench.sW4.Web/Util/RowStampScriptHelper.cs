using System;
using System.Web;
using System.Web.Optimization;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Util {
    public static class RowStampScriptHelper {

        public static IHtmlString Render(params string[] paths) {


            var defaultValue = Scripts.Render(paths);
            if (false &&ApplicationConfiguration.IsLocal()) {
                return defaultValue;
            }

            var totalMillis = ApplicationConfiguration.GetStartTimeInMillis();

            return new HtmlString(defaultValue.ToHtmlString().Replace(".js", ".js?" + totalMillis));
        }

        public static IHtmlString RenderCss(params string[] paths) {
            var defaultValue = Styles.Render(paths);
            if (false && ApplicationConfiguration.IsLocal()) {
                return defaultValue;
            }
            var totalMillis = ApplicationConfiguration.GetStartTimeInMillis();

            return new HtmlString(defaultValue.ToHtmlString().Replace(".css", ".css?" + totalMillis));
        }
    }
}