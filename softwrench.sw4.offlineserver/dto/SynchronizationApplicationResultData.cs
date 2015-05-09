using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sw4.offlineserver.dto {
    public class SynchronizationApplicationResultData {

        private readonly string _applicationName;
        /// <summary>
        /// items that should be inserted at the client side, since their ids are not present there this allows for an optimization.
        /// </summary>
        private readonly IEnumerable<DataMap> _newdataMaps;

        /// <summary>
        /// items that should be updated at the client side.
        /// </summary>
        private readonly IEnumerable<DataMap> _updatedDataMaps;
        public string UpperLimitRowstamp { get; set; }
        private readonly string _lowerLimitRowstamp;
        public IEnumerable<string> DeletedRecordIds { get; set; }



        public SynchronizationApplicationResultData(ApplicationMetadata metadata,
            IEnumerable<DataMap> newdataMaps, IEnumerable<DataMap> updateDataMaps,
            Rowstamps rowstamps, List<Dictionary<string, string>> deletedRecords = null) {
            Metadata = metadata;
            _applicationName = metadata.Name;
            _newdataMaps = newdataMaps ?? new List<DataMap>();
            _updatedDataMaps = updateDataMaps ?? new List<DataMap>();
            UpperLimitRowstamp = rowstamps.Upperlimit;
            _lowerLimitRowstamp = rowstamps.Lowerlimit;
            //DeletedRecordIds = deletedRecords ?? new List<Dictionary<string, string>>();
        }


        public SynchronizationApplicationResultData(String applicationName, IEnumerable<DataMap> newdataMaps, IEnumerable<DataMap> updateDataMaps, IEnumerable<string> deletedRecords = null) {
            _applicationName = applicationName;
            _newdataMaps = newdataMaps;
            _updatedDataMaps = updateDataMaps ?? new List<DataMap>();
            DeletedRecordIds = deletedRecords ?? new List<string>();
        }


        public string ApplicationName {
            get { return _applicationName; }
        }

        public string LowerLimitRowstamp {
            get { return _lowerLimitRowstamp; }
        }

        public IEnumerable<DataMap> NewdataMaps {
            get { return _newdataMaps; }
        }

        public IEnumerable<DataMap> UpdatedDataMaps {
            get { return _updatedDataMaps; }
        }

        public Boolean IsEmpty {
            get { return !_updatedDataMaps.Any() && !_newdataMaps.Any() && !DeletedRecordIds.Any(); }
        }

        public ApplicationMetadata Metadata { get; set; }


        public static SynchronizationApplicationResultData NoRecords(ApplicationMetadata metadata) {
            return new SynchronizationApplicationResultData(metadata, new List<DataMap>(), null, new Rowstamps(), null);
        }

        public static SynchronizationApplicationResultData CompositionRecords(String applicationName, IEnumerable<DataMap> newdataMaps, long? maxRowstamp) {
            return new SynchronizationApplicationResultData(applicationName, newdataMaps, null, null) {
                UpperLimitRowstamp = maxRowstamp == null ? null : maxRowstamp.ToString()
            };
        }


    }
}
