using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.offlineserver.model {


    [Component]
    public class DeviceData {

        [Property(Column = "device_platform")]
        public string Platform { get; set; }

        [Property(Column = "device_version")]
        public string Version { get; set; }

        [Property(Column = "device_model")]
        public string Model { get; set; }

        [Property]
        public string ClientVersion { get; set; }

    }
}
