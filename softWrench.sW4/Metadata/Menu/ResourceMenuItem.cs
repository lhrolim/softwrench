using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using softwrench.sW4.Shared2.Util;

namespace softWrench.sW4.Metadata.Menu {
    public class ResourceMenuItem : MenuBaseDefinition {


        private readonly string _path;
        private readonly IDictionary<string, string> _params = new Dictionary<string, string>();

        public ResourceMenuItem(string id, string role, string path, string @params, string tooltip, string moduleName)
            : base(id, null, role, tooltip, null) {

            _path = path;
            _params = PropertyUtil.ConvertToDictionary(@params);
            Module = moduleName;
        }

        public string Path {
            get { return _path; }
        }

        public IDictionary<string, string> Params {
            get { return _params; }
        }
    }
}
