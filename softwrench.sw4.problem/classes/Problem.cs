using System;
using cts.commons.Util;
using cts.commons.persistence;
using cts.commons.portable.Util;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.problem.classes {
    [Class(Table = "PROB_PROBLEM", Lazy = false)]
    public class Problem : IBaseEntity {

        public static string ByEntryAndType = "from Problem where recordId ='{0}' and RecordType ='{1}' and problemtype ='{2}'";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id {
            get; set;
        }
        [Property]
        public virtual string RecordType {
            get; set;
        }

        [Property]
        public virtual string RecordSchema {
            get; set;
        }

        [Property]
        public virtual string RecordId {
            get; set;
        }


        [Property]
        public virtual string RecordUserId {
            get; set;
        }

        [Property(Type = "BinaryBlob")]
        [JsonIgnore]
        public virtual byte[] Data {
            get; set;
        }
        [Property]
        public virtual DateTime CreatedDate {
            get; set;
        }
        [Property]
        public virtual int? CreatedBy {
            get; set;
        }
        [Property]
        public virtual string Assignee {
            get; set;
        }
        [Property]
        public virtual int Priority {
            get; set;
        }
        [Property]
        public virtual string StackTrace {
            get; set;
        }
        [Property]
        public virtual string Message {
            get; set;
        }
        [Property]
        public virtual string Profiles {
            get; set;
        }
//        [Property]
//        public virtual string ProblemHandler {
//            get; set;
//        }

        [Property]
        public virtual string ProblemType {
            get; set;
        }

        [Property]
        public virtual string Status {
            get; set;
        }

        public virtual string DataAsString {
            get {
                return StringExtensions
                    .GetString(CompressionUtil.Decompress(Data));
            }
            set {
                Data = CompressionUtil.Compress(value.GetBytes());
            }
        }

        public Problem() {

        }
        /// <summary>
        /// Creates a problem with the minimum and usual data required
        /// </summary>
        /// <param name="recordType">Usually the application name</param>
        /// <param name="recordSchema"></param>
        /// <param name="recordId"></param>
        /// <param name="recordUserId"></param>
        /// <param name="stackTrace"></param>
        /// <param name="message"></param>
        /// <param name="problemType"></param>
        /// <returns></returns>
        public static Problem BaseProblem(string recordType,string recordSchema, string recordId, string recordUserId,
            string stackTrace, string message, string problemType) {
            return new Problem() {
                RecordType = recordType,
                RecordSchema = recordSchema,
                RecordId = recordId,
                RecordUserId = recordUserId,
                StackTrace = stackTrace,
                Message = message,
                ProblemType = problemType,
                Status = ProblemStatus.Open.ToString(),
                CreatedDate = DateTime.Now
            };
        }

        public Problem(string recordType, string recordId, string recordUserId,
            string data, DateTime createdDate, int? createdBy,
            string assignee, int priority, string stackTrace,
            string message, string profiles, string problemType,
            string status) {
            RecordType = recordType;
            RecordId = recordId;
            RecordUserId = recordUserId;
            DataAsString = data;
            CreatedDate = createdDate;
            CreatedBy = createdBy;
            Assignee = assignee;
            Priority = priority;
            StackTrace = stackTrace;
            Message = message;
            Profiles = profiles;
            ProblemType = problemType;
            Status = status;
        }
    }
}
