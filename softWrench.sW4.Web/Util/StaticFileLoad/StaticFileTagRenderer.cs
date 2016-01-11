using System.Collections.Generic;
using System.Linq;
using System.Web;
using softWrench.sW4.Util;
using Local = softWrench.sW4.Web.Util.StaticFileLoad.Bundles.Local;
using Prod = softWrench.sW4.Web.Util.StaticFileLoad.Bundles.Distribution;

namespace softWrench.sW4.Web.Util.StaticFileLoad {
    public static class StaticFileTagRenderer {

        public static ICollection<IHtmlString> RenderScripts() {
            var styles = ApplicationConfiguration.IsLocal() ? Local.Scripts : Prod.Scripts;
            return styles.Select(path => RowStampScriptHelper.Render(path)).ToList();
        }

        public static ICollection<IHtmlString> RenderStyles() {
            var styles = ApplicationConfiguration.IsLocal() ? Local.Styles : Prod.Styles;
            return styles.Select(path => RowStampScriptHelper.RenderCss(path)).ToList();
        }

    }
}