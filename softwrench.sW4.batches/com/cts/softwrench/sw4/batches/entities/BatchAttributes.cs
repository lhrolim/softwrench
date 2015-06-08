using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.entities {

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
