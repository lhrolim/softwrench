using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Optimization;

namespace softwrench.sw4.webcommons.classes.bundles {
    static class BundleHelper {

     
            public static Bundle IncludeDirectoryWithExclusion(this Bundle bundle, string directoryVirtualPath, string searchPattern, bool includeSubDirectories, params string[] excludePatterns)
            {
                string folderPath = HttpContext.Current.Server.MapPath(directoryVirtualPath);

                SearchOption searchOption = includeSubDirectories
                    ? SearchOption.AllDirectories
                    : SearchOption.TopDirectoryOnly;

                HashSet<string> excludedFiles = GetFilesToExclude(folderPath, searchOption, excludePatterns);
                IEnumerable<string> resultFiles = Directory.GetFiles(folderPath, searchPattern, searchOption)
                    .Where(file => !excludedFiles.Contains(file) && !file.Contains(".min."));

                foreach (string resultFile in resultFiles)
                {
                    bundle.Include(directoryVirtualPath + resultFile.Replace(folderPath, "")
                                       .Replace("\\", "/"));
                }

                return bundle;
            }

            private static HashSet<string> GetFilesToExclude(string path, SearchOption searchOptions, params string[] excludePatterns)
            {
                var result = new HashSet<string>();

                foreach (string pattern in excludePatterns)
                {
                    result.UnionWith(Directory.GetFiles(path, pattern, searchOptions));
                }

                return result;
            }
        

    }
}
