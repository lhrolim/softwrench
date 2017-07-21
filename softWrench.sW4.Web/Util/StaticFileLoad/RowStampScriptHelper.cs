using System.Web;
using System.Web.Optimization;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Util.StaticFileLoad {
    public static class RowStampScriptHelper {

        public static IHtmlString Render(params string[] paths) {
            var defaultValue = Scripts.Render(paths);
            if (ApplicationConfiguration.IsLocal()) {
                return defaultValue;
            }
            var totalMillis = ApplicationConfiguration.GetStartTimeInMillis();
            return new HtmlString(defaultValue.ToHtmlString().Replace(".js", ".js?" + totalMillis));
        }

        public static IHtmlString RenderDefer(params string[] paths) {
            var defaultValue = Scripts.RenderFormat(@"<script src='{0}' defer></script>", paths);
            if (ApplicationConfiguration.IsLocal()) {
                return defaultValue;
            }
            var totalMillis = ApplicationConfiguration.GetStartTimeInMillis();
            return new HtmlString(defaultValue.ToHtmlString().Replace(".js", ".js?" + totalMillis));
        }

        public static IHtmlString RenderCss(params string[] paths) {
            var defaultValue = Styles.Render(paths);
            if (ApplicationConfiguration.IsLocal()) {
                return defaultValue;
            }
            var totalMillis = ApplicationConfiguration.GetStartTimeInMillis();
            return new HtmlString(defaultValue.ToHtmlString().Replace(".css", ".css?" + totalMillis));
        }
    }
}