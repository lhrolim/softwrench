using System.Collections.Generic;
using cts.commons.portable.Util;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;

namespace softWrench.sW4.Metadata.Menu {
    public class ResourceMenuItem : MenuBaseDefinition {
        public string ModuleName { get; set; }


        private readonly string _path;
        private readonly IDictionary<string, object> _params = new Dictionary<string, object>();

        public ResourceMenuItem(string id, string role, string path, string @params, string tooltip,string moduleName,string permissionExpression, string customizationPosition)
            : base(id, null, role, tooltip, null, customizationPosition) {
            ModuleName = moduleName;

            _path = path;
            _params = PropertyUtil.ConvertToDictionary(@params);
            PermissionExpresion = permissionExpression;
            }

        public string Path {
            get { return _path; }
        }

        public IDictionary<string, object> Params {
            get { return _params; }
        }
    }
}
