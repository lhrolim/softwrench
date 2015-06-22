using System;
using cts.commons.Util;
using cts.commons.persistence;
using cts.commons.portable.Util;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.problem.classes {
    [Class(Table = "PROB_PROBLEM", Lazy = false)]
    public class Problem : IBaseEntity {
        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }
        [Property]
        public virtual string RecordType { get; set; }
        [Property]
        public virtual string RecordId { get; set; }
        [Property(Type = "BinaryBlob")]
        [JsonIgnore]
        public virtual byte[] Data { get; set; }
        [Property]
        public virtual DateTime CreatedDate { get; set; }
        [Property]
        public virtual int? CreatedBy { get; set; }
        [Property]
        public virtual string Assignee { get; set; }
        [Property]
        public virtual int Priority { get; set; }
        [Property]
        public virtual string StackTrace { get; set; }
        [Property]
        public virtual string Message { get; set; }
        [Property]
        public virtual string Profiles { get; set; }
        [Property]
        public virtual string ProblemHandler { get; set; }
        [Property]
        public virtual string Status { get; set; }

        public virtual string DataAsString {
            get {
                return StringExtensions
                    .GetString(CompressionUtil.Decompress(Data));
            }
            set { Data = CompressionUtil.Compress(value.GetBytes()); }
        }

        public Problem() {

        }

        public Problem(string recordType, string recordId,
            string data, DateTime createdDate, int? createdBy,
            string assignee, int priority, string stackTrace,
            string message, string profiles, string problemHandler,
            string status) {
            RecordType = recordType;
            RecordId = recordId;
            DataAsString = data;
            CreatedDate = createdDate;
            CreatedBy = createdBy;
            Assignee = assignee;
            Priority = priority;
            StackTrace = stackTrace;
            Message = message;
            Profiles = profiles;
            ProblemHandler = problemHandler;
            Status = status;
        }
    }
}
