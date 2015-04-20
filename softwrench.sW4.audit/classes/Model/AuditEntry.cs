using System;
using cts.commons.persistence;
using cts.commons.portable.Util;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Util;

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
        [Property(Type = "BinaryBlob")]
        [JsonIgnore]
        public virtual byte[] Data { get; set; }
        [Property]
        public virtual string CreatedBy { get; set; }
        [Property]
        public virtual DateTime CreatedDate { get; set; }

        public virtual string DataStringValue
        {
            get { return StringExtensions.GetString(CompressionUtil.Decompress(Data)); }
            set { Data = CompressionUtil.Compress(value.GetBytes()); }
        }

        public AuditEntry()
        {
            
        }

        public AuditEntry(int Id, string action, string refApplication, string refId, string data, string createdBy, DateTime createdDate)
        {
            this.Id = Id;
            Action = action;
            RefApplication = refApplication;
            RefId = refId;
            DataStringValue = data;
            CreatedBy = createdBy;
            CreatedDate = createdDate;
        }

        public AuditEntry(int id, string action, string refApplication, string refId, string data, string createdBy, DateTime createdDate)
        {
            Id = id;
            Action = action;
            RefApplication = refApplication;
            RefId = refId;
            DataStringValue = data;
            CreatedBy = createdBy;
            CreatedDate = createdDate;
        }
    }
}
