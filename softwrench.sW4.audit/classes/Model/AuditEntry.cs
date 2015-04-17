using System;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softwrench.sW4.audit.classes.Model {
    [Class(Table = "audit_entry", Lazy = false)]
    public class AuditEntry : IBaseEntity
    {
        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }
        [Property]
        public virtual string Action { get; set; }
        [Property]
        public virtual string RefApplication { get; set; }
        [Property]
        public virtual string RefId { get; set; }
        [Property]
        public virtual string Data { get; set; }
        [Property]
        public virtual string CreatedBy { get; set; }
        [Property]
        public virtual DateTime CreatedDate { get; set; }

        public AuditEntry(string action, string refApplication, string refId, string data, string createdBy, DateTime createdDate)
        {
            Action = action;
            RefApplication = refApplication;
            RefId = refId;
            Data = data;
            CreatedBy = createdBy;
            CreatedDate = createdDate;
        }
    }
}
