using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.Shared2.Data.Association;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action.dto {

    public class BatchData {

        public string Summary {
            get; set;
        }
        public string Details {
            get; set;
        }
        public string SiteId {
            get; set;
        }

        public string Classification {
            get; set;
        }


    }

    public class LocationBatchData : BatchData {
        public List<AssociationOption> Locations {
            get; set;
        }
    }

    public class AssetBatchData : BatchData {
        public List<AssociationOption> Assets {
            get; set;
        }
    }

    public class AssetBatchSubmissionData {

        public BatchData SharedData {
            get; set;
        }

        public IDictionary<string, BatchData> LocationSpecificData {
            get; set;
        }
    }

    public class LocationBatchSubmissionData {

        public BatchData SharedData {
            get; set;
        }

        public IDictionary<string, BatchData> SpecificData {
            get; set;
        }
    }
}
