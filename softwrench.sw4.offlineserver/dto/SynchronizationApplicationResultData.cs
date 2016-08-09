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


        public ICollection<DataMap> InsertOrUpdateDataMaps {
            get; set;
        }

        /// <summary>
        /// items that should be inserted at the client side, since their ids are not present there this allows for an optimization.
        /// </summary>
        public ICollection<DataMap> NewdataMaps {
            get; set;
        }

        /// <summary>
        /// items that should be updated at the client side.
        /// </summary>
        public IList<DataMap> UpdatedDataMaps {
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
            NewdataMaps = new List<DataMap>();
            UpdatedDataMaps = new List<DataMap>();
            DeletedRecordIds = new List<string>();
            AlreadyExistingDatamaps = new List<DataMap>();
            InsertOrUpdateDataMaps = new List<DataMap>();
            TextIndexes = new List<string>();
            NumericIndexes = new List<string>();
            DateIndexes = new List<string>();
            ApplicationName = applicationName;
        }

        public SynchronizationApplicationResultData(String applicationName, IList<DataMap> newdataMaps, IList<DataMap> updateDataMaps, IList<string> deletedRecords = null) {
            ApplicationName = applicationName;
            NewdataMaps = newdataMaps;
            UpdatedDataMaps = updateDataMaps ?? new List<DataMap>();
            DeletedRecordIds = deletedRecords ?? new List<string>();
            InsertOrUpdateDataMaps = new List<DataMap>();
            TextIndexes = new List<string>();
            NumericIndexes = new List<string>();
            DateIndexes = new List<string>();
        }





        public Boolean IsEmpty {
            get {
                return !UpdatedDataMaps.Any() && !NewdataMaps.Any() && !DeletedRecordIds.Any() &&
                       !InsertOrUpdateDataMaps.Any();
            }
        }

        public Boolean IsEmptyExceptDeletion {
            get {
                return !UpdatedDataMaps.Any() && !NewdataMaps.Any() && !InsertOrUpdateDataMaps.Any();
            }
        }


        public ApplicationMetadata Metadata {
            get; set;
        }

        public bool HasNewEntries {
            get {
                return NewdataMaps.Count > 0;
            }
        }

        public int NewCount {
            get {
                return NewdataMaps.Count;
            }
        }





    }
}
