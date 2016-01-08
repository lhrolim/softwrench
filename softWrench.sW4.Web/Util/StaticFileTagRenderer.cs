using System.Collections.Generic;
using System.Linq;
using System.Web;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Util {
    public static class StaticFileTagRenderer {

        private static readonly string[] LocalStyles = {
            "~/Content/vendor/css",
            "~/Content/customVendor/css",
            "~/Content/fonts",
            "~/Content/styles/client/client-css",
            "~/Content/styles/shared"
        };

        private static readonly string[] DistributionStyles = {
            "~/Content/dist/css"
        };

        private static readonly string[] LocalScripts = {
            "~/Content/vendor/scripts",
            "~/Content/customVendor/scripts",
            "~/Content/Scripts/client/application",
            "~/Content/Scripts/client/application/shared"
        };

        private static readonly string[] DistributionScripts = {
            "~/Content/dist/scripts"
        };

        public static ICollection<IHtmlString> RenderScripts() {
            var styles = ApplicationConfiguration.IsLocal() ? LocalStyles : DistributionStyles;
            return styles.Select(path => RowStampScriptHelper.RenderCss(path)).ToList();
        }

        public static ICollection<IHtmlString> RenderStyles() {
            var styles = ApplicationConfiguration.IsLocal() ? LocalScripts : DistributionScripts;
            return styles.Select(path => RowStampScriptHelper.Render(path)).ToList();
        }

    }
}