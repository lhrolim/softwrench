using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data;

namespace softwrench.sw4.offlineserver.dto.association {

    public class AssociationSynchronizationResultDto {

        public IDictionary<string, List<DataMap>> AssociationData { get; set; }

        public AssociationSynchronizationResultDto() {
            AssociationData = new ConcurrentDictionary<string, List<DataMap>>();
        }

        public Boolean IsEmpty {
            get { return AssociationData.All(data => !data.Value.Any()); }
        }

    }
}
