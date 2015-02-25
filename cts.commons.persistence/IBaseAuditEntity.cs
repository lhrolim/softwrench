using System;
using NHibernate.Mapping.Attributes;

namespace cts.commons.persistence {
    public interface IBaseAuditEntity : IBaseEntity {
       
        [Property]
        DateTime? CreationDate { get; set; }

        [Property]
        DateTime? UpdateDate { get; set; }

        [Property]
        int? CreatedBy { get; set; }

    }
}