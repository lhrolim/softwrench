using System;
using softWrench.Mobile.Metadata.Applications;
using softWrench.Mobile.Metadata.Parsing;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using SQLite;

namespace softWrench.Mobile.Persistence
{
    [Table("ApplicationSchemaDefinition")]
    public class PersistableApplicationMetadata
    {
        public PersistableApplicationMetadata(ApplicationSchemaDefinition schema)
        {
//            Id = ApplicationSchemaDefinition.Id;
            Name = schema.Name;
            Data = schema.ToJson();
        }

        public PersistableApplicationMetadata()
        {
        }

        [PrimaryKey]
        public string Name { get; set; }

        public string Data { get; set; }
    }
}

