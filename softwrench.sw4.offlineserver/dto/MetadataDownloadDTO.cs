using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.offlineserver.model;

namespace softwrench.sw4.offlineserver.dto {
    public class MetadataDownloadDto {

        public string ClientOperationId { get; set; }

        public DeviceData DeviceData { get; set; }

        public string Version { get; set; }
    }
}
