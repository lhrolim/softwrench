using System;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Parsing;
using SQLite;

namespace softWrench.Mobile.Persistence
{
    [Table("DataOperation")]
    public class PersistableDataOperation
    {
        public PersistableDataOperation(DataOperation dataOperation, DateTime localRowStamp)
        {
            LocalId = dataOperation.LocalId;
            Application = dataOperation.Application;
            Handler = dataOperation.Handler;
            Data = dataOperation.Data.ToJson();
            LocalRowStamp = localRowStamp;
        }

        public PersistableDataOperation()
        {
        }

        public DataOperation ToDataOperation()
        {
            var data = JsonParser.DataOperation(Data);
            return new DataOperation(Application, data, Handler, LocalId);
        }

        [PrimaryKey]
        public Guid LocalId { get; set; }

        [Indexed("DataOperationQualifiedName", 0)]
        public string Application { get; set; }

        [Indexed("DataOperationQualifiedName", 0)]
        public string Handler { get; set; }

        public string Data { get; set; }

        public DateTime LocalRowStamp { get; set; }
    }
}