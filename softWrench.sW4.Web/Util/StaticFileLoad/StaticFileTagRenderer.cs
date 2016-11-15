using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using log4net;
using softWrench.sW4.Util;
using Local = softWrench.sW4.Web.Util.StaticFileLoad.Bundles.Local;
using Prod = softWrench.sW4.Web.Util.StaticFileLoad.Bundles.Distribution;

namespace softWrench.sW4.Web.Util.StaticFileLoad {
    public static class StaticFileTagRenderer
    {

        private static ILog Log = LogManager.GetLogger(typeof (StaticFileTagRenderer));

        public static ICollection<IHtmlString> RenderScripts()
        {
            
            var scripts = ApplicationConfiguration.IsLocal() ? Local.Scripts : Prod.Scripts;
            var scriptTags = scripts.Select(path => RowStampScriptHelper.Render(path));
            var watch = Stopwatch.StartNew();
//            var renderScripts = RenderDistinctTags(scriptTags, "</script>");
            watch.Stop();
            Log.DebugOrInfoFormat("static rendering scripts, took {0} ms", watch.ElapsedMilliseconds);
            return scriptTags.ToList();
        }

        public static ICollection<HtmlString> RenderStyles() {
            var watch = Stopwatch.StartNew();
            var styles = ApplicationConfiguration.IsLocal() ? Local.Styles : Prod.Styles;
            var styleTags = styles.Select(path => RowStampScriptHelper.RenderCss(path));
            var renderDistinctTags = RenderDistinctTags(styleTags, "/>");
            watch.Stop();
            Log.DebugOrInfoFormat("static rendering styles, took {0} ms", watch.ElapsedMilliseconds);
            return renderDistinctTags;
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