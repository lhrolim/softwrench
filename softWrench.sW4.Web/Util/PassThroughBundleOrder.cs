using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web.Optimization;

namespace softWrench.sW4.Web.Util {
    public class PassthroughBundleOrderer : IBundleOrderer {

        public IEnumerable<FileInfo> OrderFiles(BundleContext context, IEnumerable<FileInfo> files) {
            //            yield break;
            return files;
        }
    }
}
