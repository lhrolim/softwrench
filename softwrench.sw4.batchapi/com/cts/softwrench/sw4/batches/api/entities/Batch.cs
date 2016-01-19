using System;
using System.Collections.Generic;
using System.Data;
using Iesi.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NHibernate.Mapping.Attributes;
using softwrench.sw4.api.classes.user;
using softwrench.sw4.problem.classes;
using softWrench.sW4.Data.Persistence.WS.API;

namespace softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities {

    ///
    [Class(Table = "BAT_BATCH", Lazy = false)]
    public class Batch : IBatch {

        public static string ActiveBatchesofApplication = "from Batch where Application =? and Status = 'INPROG' ";
        public static string OldSubmittedBatches = "from Batch where Application =? and Status = 'COMPLETE' ";
        public static string BatchesByRemoteId = "from Batch where RemoteId in (:p0)";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public DateTime CreationDate {
            get; set;
        }
        [Property]
        public DateTime? UpdateDate {
            get; set;
        }
        [Property]
        public int? CreatedBy {
            get; set;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [Property(TypeType = typeof(BatchStatusType))]
        public BatchStatus Status {
            get; set;
        }

        [Property]
        public String RemoteId {
            get; set;
        }

        [Property]
        public String Application {
            get; set;
        }


        [JsonIgnore]
        [Set(0, Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "batch_id", NotNull = true)]
        [OneToMany(2, ClassType = typeof(BatchItem))]
        public virtual Iesi.Collections.Generic.ISet<BatchItem> Items {
            get; set;
        }


        /// <summary>
        ///  There´s no necessity for the BatchReport here, this class plus the BatchItems will hold the status after all.
        ///   
        ///  For synchronous operations this will be built in-memory as the batch proceeds. Otherwise a query will be fetched to determine it
        ///    
        /// </summary>
        public Iesi.Collections.Generic.ISet<string> SuccessItems = new HashedSet<string>();

        public Iesi.Collections.Generic.ISet<TargetResult> TargetResults = new HashedSet<TargetResult>();

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, Problem> Problems = new Dictionary<string, Problem>();

        public int NumberOfItems {
            get {
                return Items.Count;
            }
        }

        public static Batch TransientInstance(string application, ISWUser user) {
            return new Batch() {
                CreationDate = DateTime.Now,
                CreatedBy = user.UserId,
                Status = BatchStatus.SUBMITTING,
                Application = application
            };

        }

    }


}
