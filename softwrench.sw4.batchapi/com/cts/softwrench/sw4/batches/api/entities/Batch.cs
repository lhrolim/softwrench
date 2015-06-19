using System;
using cts.commons.persistence;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities {

    ///
    [Class(Table = "BAT_BATCH", Lazy = false)]
    public class Batch : IBatch {

        public static string ActiveBatchesofApplication = "from Batch where Application =? and Status = 'INPROG' ";
        public static string OldSubmittedBatches = "from Batch where Application =? and Status = 'COMPLETE' ";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id { get; set; }

        [Property]
        public DateTime CreationDate { get; set; }
        [Property]
        public DateTime? UpdateDate { get; set; }
        [Property]
        public int? CreatedBy { get; set; }

        [Property(TypeType = typeof(BatchStatusType))]
        public BatchStatus Status { get; set; }
        [Property]
        public String RemoteId { get; set; }

        [Property]
        public String Application { get; set; }


        [JsonIgnore]
        [Set(0, Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "batch_id", NotNull = true)]
        [OneToMany(2, ClassType = typeof(BatchItem))]
        public virtual Iesi.Collections.Generic.ISet<BatchItem> Items { get; set; }


        public int NumberOfItems { get { return Items.Count; } }
    }


}
