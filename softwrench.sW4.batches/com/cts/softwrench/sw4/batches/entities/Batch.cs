using System;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.Util;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Entities;
using CompressionUtil = cts.commons.Util.CompressionUtil;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.entities {

    ///
    [Class(Table = "BAT_BATCH", Lazy = false)]
    public class Batch : IBaseAuditEntity {

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
        [Key(1, Column = "batch_id",NotNull = true)]
        [OneToMany(2, ClassType = typeof(BatchItem))]
        public virtual Iesi.Collections.Generic.ISet<BatchItem> Items { get; set; }


    }


}
