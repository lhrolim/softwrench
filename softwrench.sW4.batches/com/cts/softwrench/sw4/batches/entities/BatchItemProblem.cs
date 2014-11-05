using System;
using System.Linq;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Interfaces;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.entities {

    [Class(Table = "BAT_BATCHITEMPROBLEM", Lazy = false)]
    public class BatchItemProblem : IBaseEntity {

        public static string ActiveBatchesofApplication = "from Batch where Application =? and Status = 'INPROG' ";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id { get; set; }

        [Property]
        public String ItemId { get; set; }

        [Property(Type = "BinaryBlob")]
        [JsonIgnore]
        public virtual byte[] DataMapJson { get; set; }

        [ManyToOne(Column = "report_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        public virtual BatchReport Report { get; set; }

        [Property]
        public String ErrorMessage { get; set; }

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


     



    }
}
