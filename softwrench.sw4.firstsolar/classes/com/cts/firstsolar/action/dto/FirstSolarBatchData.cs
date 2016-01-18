using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.Shared2.Data.Association;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action.dto {

    public class BatchSharedData {

        public string Summary {
            get; set;
        }
        public string Details {
            get; set;
        }
        public string SiteId {
            get; set;
        }

        public AssociationOption Classification {
            get; set;
        }
    }

    public class BatchStartingData :BatchSharedData {

        public List<MultiValueAssociationOption> Items {
            get; set;
        }

    }


    public class BatchSpecificData : BatchSharedData {
        
    }

    public class AssetBatchSpecificData : BatchSpecificData {

        //for asset batches, the Location might be passed as the asset´s location
        public string Location {
            get; set;
        }

    }

    public class BatchSubmissionData {

        public BatchSharedData SharedData {
            get; set;
        }

        public IDictionary<string, BatchSpecificData> SpecificData {
            get; set;
        }
    }
}
