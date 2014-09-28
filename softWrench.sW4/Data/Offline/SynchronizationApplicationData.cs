using System.Collections.Generic;
using JetBrains.Annotations;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Entity.Association;

namespace softWrench.sW4.Data.Offline {
    public class SynchronizationApplicationData {

        private readonly string _applicationName;
        private readonly ICollection<DataMap> _dataMaps;
        private readonly string _upperLimitRowstamp;
        private readonly string _lowerLimitRowstamp;
        public IList<object> DeletedRecordIds { get; set; }


        public SynchronizationApplicationData(ApplicationMetadata metadata, ICollection<DataMap> dataMaps, ICollection<EntityAssociation> unresolvedAssociations, [NotNull]Rowstamps rowstamps, IList<object> deletedRecords = null) {
            Metadata = metadata;
            _applicationName = metadata.Name;
            _dataMaps = dataMaps;
            _upperLimitRowstamp = rowstamps.Upperlimit;
            _lowerLimitRowstamp = rowstamps.Lowerlimit;
            DeletedRecordIds = deletedRecords ?? new List<object>();
        }

        public string UpperLimitRowstamp {
            get { return _upperLimitRowstamp; }
        }

        public string ApplicationName {
            get { return _applicationName; }
        }

        public string LowerLimitRowstamp {
            get { return _lowerLimitRowstamp; }
        }

        public ICollection<DataMap> DataMaps {
            get { return _dataMaps; }
        }

        public ApplicationMetadata Metadata { get; set; }


        public static SynchronizationApplicationData NoRecords(ApplicationMetadata metadata) {
            return new SynchronizationApplicationData(metadata, new List<DataMap>(), null, new Rowstamps(), null);
        }
    }
}
