using System;
using System.Linq;
using cts.commons.portable.Util;
using cts.commons.Util;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.batch.api.entities {

    ///
    [Class(Table = "BAT_MULBATCH", Lazy = false)]
    public class MultiItemBatch : IBatch {

        public static string ActiveBatchesofApplication = "from MultiItemBatch where Application =? and Status = 'INPROG' ";
        public static string OldSubmittedBatches = "from MultiItemBatch where Application =? and Status = 'COMPLETE' ";

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
        public DateTime CreationDate { get; set; }

        [Property]
        public DateTime? UpdateDate { get; set; }

        //TODO: change table
        public int? CreatedBy { get; set; }

        /// <summary>
        /// a comma separated list of the item ids inside this batch
        /// </summary>
        [Property]
        public String ItemIds { get; set; }

        public int NumberOfItems {
            get {
                if (ItemIds == null) {
                    return 0;
                }
                var numberOfCommas = ItemIds.Count(f => f == ',');
                return numberOfCommas + 1;
            }
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
