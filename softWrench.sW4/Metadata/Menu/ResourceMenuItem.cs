using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using softwrench.sW4.Shared2.Util;

namespace softWrench.sW4.Metadata.Menu {
    public class ResourceMenuItem : MenuBaseDefinition {
        public string ModuleName { get; set; }


        private readonly string _path;
        private readonly IDictionary<string, object> _params = new Dictionary<string, object>();

        public ResourceMenuItem(string id, string role, string path, string @params, string tooltip,string moduleName)
            : base(id, null, role, tooltip, null) {
            ModuleName = moduleName;

            _path = path;
            _params = PropertyUtil.ConvertToDictionary(@params);

        }

        public string Path {
            get { return _path; }
        }

        public IDictionary<string, object> Params {
            get { return _params; }
        }
    }
}
