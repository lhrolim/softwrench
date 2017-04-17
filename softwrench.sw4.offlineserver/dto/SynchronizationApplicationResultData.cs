using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sw4.offlineserver.dto {
    public class SynchronizationApplicationResultData {

        public string ApplicationName {
            get; set;
        }


        public ICollection<JSONConvertedDatamap> InsertOrUpdateDataMaps {
            get; set;
        }

        /// <summary>
        /// items that should be inserted at the client side, since their ids are not present there this allows for an optimization.
        /// </summary>
        public ICollection<JSONConvertedDatamap> NewdataMaps {
            get; set;
        }

        /// <summary>
        /// items that should be updated at the client side.
        /// </summary>
        public IList<JSONConvertedDatamap> UpdatedDataMaps {
            get; set;
        }
        public ICollection<string> DeletedRecordIds {
            get; set;
        }

        [JsonIgnore]
        public ICollection<DataMap> AllData {
            get; set;
        }

        [JsonIgnore]
        public ICollection<DataMap> AlreadyExistingDatamaps {
            get; set;
        }

        public IList<string> TextIndexes {
            get; set;
        }

        public IList<string> NumericIndexes {
            get; set;
        }

        public IList<string> DateIndexes {
            get; set;
        }

        public SynchronizationApplicationResultData(string applicationName) {
            NewdataMaps = new List<JSONConvertedDatamap>();
            UpdatedDataMaps = new List<JSONConvertedDatamap>();
            DeletedRecordIds = new List<string>();
            AlreadyExistingDatamaps = new List<DataMap>();
            InsertOrUpdateDataMaps = new List<JSONConvertedDatamap>();
            TextIndexes = new List<string>();
            NumericIndexes = new List<string>();
            DateIndexes = new List<string>();
            ApplicationName = applicationName;
        }

        public SynchronizationApplicationResultData(String applicationName, IList<JSONConvertedDatamap> newdataMaps, IList<JSONConvertedDatamap> updateDataMaps, IList<string> deletedRecords = null) {
            ApplicationName = applicationName;
            NewdataMaps = newdataMaps;
            UpdatedDataMaps = updateDataMaps ?? new List<JSONConvertedDatamap>();
            DeletedRecordIds = deletedRecords ?? new List<string>();
            InsertOrUpdateDataMaps = new List<JSONConvertedDatamap>();
            TextIndexes = new List<string>();
            NumericIndexes = new List<string>();
            DateIndexes = new List<string>();
        }





        public Boolean IsEmpty => !UpdatedDataMaps.Any() && !NewdataMaps.Any() && !DeletedRecordIds.Any() &&
                                  !InsertOrUpdateDataMaps.Any();

        public Boolean IsEmptyExceptDeletion => !UpdatedDataMaps.Any() && !NewdataMaps.Any() && !InsertOrUpdateDataMaps.Any();


        public ApplicationMetadata Metadata {
            get; set;
        }

        public bool HasNewEntries => NewdataMaps.Count > 0;

        public int NewCount => NewdataMaps.Count;
    }
}
