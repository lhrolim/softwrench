using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate.Linq;
using softWrench.sW4.Util;
using Local = softWrench.sW4.Web.Util.StaticFileLoad.Bundles.Local;
using Prod = softWrench.sW4.Web.Util.StaticFileLoad.Bundles.Distribution;

namespace softWrench.sW4.Web.Util.StaticFileLoad {
    public static class StaticFileTagRenderer {

        public static ICollection<HtmlString> RenderScripts() {
            var scripts = ApplicationConfiguration.IsLocal() ? Local.Scripts : Prod.Scripts;
            var scriptTags = scripts.Select(path => RowStampScriptHelper.Render(path));
            return RenderDistinctTags(scriptTags, "</script>");
        }

        public static ICollection<HtmlString> RenderStyles() {
            var styles = ApplicationConfiguration.IsLocal() ? Local.Styles : Prod.Styles;
            var styleTags = styles.Select(path => RowStampScriptHelper.RenderCss(path));
            return RenderDistinctTags(styleTags, "/>");
        }

        /// <summary>
        /// Removes duplicate tags.
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="splitter"></param>
        /// <returns></returns>
        private static ICollection<HtmlString> RenderDistinctTags(IEnumerable<IHtmlString> tags, string splitter) {
            return tags.Select(t => t.ToHtmlString())
                .Select(t => 
                    t.Split(new[] { splitter }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => (s + splitter).Trim())
                    .ToList())
                .SelectMany(array => array)
                .Distinct()
                .Select(t => new HtmlString(t + "\n")).ToList();
        }
    }
}