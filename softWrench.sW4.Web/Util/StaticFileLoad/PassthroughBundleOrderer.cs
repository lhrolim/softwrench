using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Optimization;

namespace softWrench.sW4.Web.Util.StaticFileLoad {
    public class PassthroughBundleOrderer : IBundleOrderer {
        /// <summary>
        /// Returns the same file list back to the caller without doing any ordering on it whatsoever
        /// </summary>
        /// <param name="context"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        public IEnumerable<FileInfo> OrderFiles(BundleContext context, IEnumerable<FileInfo> files) {
            return files;
        }
    }
}
