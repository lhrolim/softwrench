using System;
using System.Linq;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Interfaces;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.entities {

    [Class(Table = "BAT_BATCH", Lazy = false)]
    public class Batch : IBaseEntity
    {

        public static string ActiveBatchesofApplication = "from Batch where Application =? and Status = 'INPROG' ";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id { get; set; }

        [Property]
        public String Application { get; set; }

        [Property(Column = "alias_")]
        public String Alias { get; set; }

        [Property(Column = "schema_")]
        public String Schema { get; set; }

        [Property]
        public int? UserId { get; set; }

        [Property(TypeType = typeof(BatchStatusType))]
        public BatchStatus Status { get; set; }

        [Property]
        public DateTime? CreationDate { get; set; }

        [Property]
        public DateTime? UpdateDate { get; set; }

        /// <summary>
        /// a comma separated list of the item ids inside this batch
        /// </summary>
        [Property]
        public String ItemIds { get; set; }

        public int NumberOfItems {
            get { return ItemIds == null ? 0 : ItemIds.Count(f => f == ','); }
        }

        /// <summary>
        /// this will hold only the editable fields, as the others should be fetched from the database
        /// </summary>
        public virtual string DataMapJsonAsString {
            get {
                return DataMapJson == null ? null : StringExtensions.GetString(CompressionUtil.Decompress(DataMapJson));
            }
            set {
                DataMapJson = CompressionUtil.Compress(value.GetBytes());
            }
        }

        [Property(Type = "BinaryBlob")]
        [JsonIgnore]
        public virtual byte[] DataMapJson { get; set; }



    }
}
