using System;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Interfaces;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.entities {

    [Class(Table = "BATCH", Lazy = false)]
    public class Batch : IBaseEntity {
        public int? Id { get; set; }

        [Property]
        public String Application { get; set; }

        [Property]
        public String Schema { get; set; }

        [Property]
        public int? UserId { get; set; }

        [Property]
        public string Status { get; set; }
        
        /// <summary>
        /// this will hold only the editable fields, as the others should be fetched from the database
        /// </summary>
        [Property]
        public string DataMapJson { get; set; }

        

    }
}
