using System;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.batch.api.entities {

    [Component]
    public class BatchAttributes {

        [Property]
        public int? UserId { get; set; }

        [Property(TypeType = typeof(BatchStatusType))]
        public BatchStatus Status { get; set; }


        [Property]
        public DateTime? CreationDate { get; set; }

        [Property]
        public DateTime? UpdateDate { get; set; }


    }
}
