using System;
using cts.commons.persistence;
using cts.commons.portable.Util;
using Iesi.Collections.Generic;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.batch.api.entities {
    [Class(Table = "BAT_REPORT", Lazy = false)]
    public class BatchReport : IBaseEntity {

        public const string ByBatchId = "from BatchReport where OriginalBatch.Id =?";


        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Set(0, Inverse = true, Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "report_id")]
        [OneToMany(2, ClassType = typeof(BatchItemProblem))]
        public virtual ISet<BatchItemProblem> ProblemItens { get; set; }

        //actually it would be one-to-one
        [ManyToOne(Column = "batch")]
        public virtual MultiItemBatch OriginalMultiItemBatch { get; set; }

        [Property]
        public virtual DateTime? CreationDate { get; set; }

        /// <summary>
        /// a comma separated list of the item ids that were successfully sent
        /// </summary>
        [Property]
        public virtual String SentItemIds { get; set; }

        public Int32 NumberOfSentItens {
            get { return SentItemIds == null ? 0 : SentItemIds.GetNumberOfItems(",") + 1; }
        }

        public Int32 NumberOfProblemItens {
            get { return ProblemItens == null ? 0 : ProblemItens.Count; }
        }

        public void AppendSentItem(string id) {
            if (String.IsNullOrEmpty(SentItemIds)) {
                SentItemIds += id;
            } else {
                SentItemIds += "," + id;
            }
        }

        public Int32 PercentageDone {
            get {
                var numberOfItems = OriginalMultiItemBatch.NumberOfItems;
                var totalSentItens = NumberOfProblemItens + NumberOfSentItens;
                var percentageDone = totalSentItens * 100 / numberOfItems;
                //return 100 to avoid strange scenarios on screen
                return percentageDone > 100 ? 100 : percentageDone;
            }
        }

        public string GetReportKey() {

            if (OriginalMultiItemBatch.Id != null) {
                return "sw_batchreport{0}".Fmt(OriginalMultiItemBatch.Id);
            }
            return null;

        }
    }
}
