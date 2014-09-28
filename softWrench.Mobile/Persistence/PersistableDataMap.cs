using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Applications;
using softWrench.Mobile.Metadata.Parsing;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using SQLite;

namespace softWrench.Mobile.Persistence
{
    [Table("DataMap")]
    public class PersistableDataMap
    {
        public PersistableDataMap(IApplicationIdentifier application, DataMap dataMap, bool isDirty, DateTime localRowStamp)
        {
            Application = application.ApplicationName;
            Id = dataMap.Id(application);
            IsDirty = isDirty;
            Fields = dataMap.Fields.ToJson();
            LocalRowStamp = localRowStamp;
            LocalId = dataMap.LocalState.LocalId;
            ParentId = dataMap.LocalState.ParentId;
            IsLocal = dataMap.LocalState.IsLocal;
            IsBouncing = dataMap.LocalState.IsBouncing;
            BounceReason = dataMap.LocalState.BounceReason;
            Flags = dataMap.LocalState.Flags;

            // If the custom fields is empty we'll explicitly
            // serialize it as a null. This comes in handy as
            // we can now easily identify through SQL queries
            // those data maps that do have custom fields, as
            // they generally require special behavior during
            // synchronization.
            CustomFields = dataMap.CustomFields.Any()
                ? dataMap.CustomFields.ToJson()
                : null;
        }

        public PersistableDataMap()
        {
        }

        public DataMap ToDataMap()
        {
            // If the custom fields list was empty before being
            // serialized, we explicitly persisted it as a null.
            // Now we undo this behavior and transform null into
            // an empty dictionary.
            var customFields = string.IsNullOrWhiteSpace(CustomFields)
                ? new Dictionary<string, string>()
                : JsonParser.FromJson<Dictionary<string, string>>(CustomFields);

            var fields =  JsonParser.FromJson<Dictionary<string, string>>(Fields);
            var dataMap = new DataMap(Application, fields, customFields);

            dataMap.LocalState.LocalId = LocalId;
            dataMap.LocalState.ParentId = ParentId;
            dataMap.LocalState.IsLocal = IsLocal;
            dataMap.LocalState.IsBouncing = IsBouncing;
            dataMap.LocalState.BounceReason = BounceReason;
            dataMap.LocalState.Flags = Flags;

            return dataMap;
        }

        [PrimaryKey]
        public Guid LocalId { get; set; }

        [Indexed]
        public Guid? ParentId { get; set; }

        [Indexed("DataMapQualifiedId", 0)]
        public string Application { get; set; }

        [Indexed("DataMapQualifiedId", 1)]
        public string Id { get; set; }

        [Indexed]
        public string Fields { get; set; }

        public string CustomFields { get; set; }

        [Indexed]
        public DateTime LocalRowStamp { get; set; }

        public bool IsDirty { get; set; }

        public bool IsLocal { get; set; }

        public bool IsBouncing { get; set; }

        public string BounceReason { get; set; }

        public LocalStateFlag Flags { get; set; }
    }
}

