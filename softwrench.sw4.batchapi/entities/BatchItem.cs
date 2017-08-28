using System;
using System.Collections.Generic;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate.Mapping.Attributes;
using softwrench.sw4.problem.classes;

namespace softwrench.sw4.batch.api.entities {

    [Class(Table = "BAT_BATCHITEM", Lazy = false)]
    public class BatchItem : IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public DateTime? UpdateDate {
            get; set;
        }

        [Property(TypeType = typeof(BatchStatusType))]
        public BatchStatus Status {
            get; set;
        }

        [Property]
        public string Application {
            get; set;
        }

        /// <summary>
        /// The id of the item in Maximo
        /// </summary>
        [Property]
        public string ItemId {
            get; set;
        }

        [Property(Column = "schema_")]
        public string Schema {
            get; set;
        }

        [Property]
        public string Operation {
            get; set;
        }

        [Property]
        public string RemoteId {
            get; set;
        }

        [Property(Type = "BinaryBlob")]
        [JsonIgnore]
        public virtual byte[] DataMapJson {
            get; set;
        }

        [Property(Type = "BinaryBlob")]
        [JsonIgnore]
        public virtual byte[] SentXml {
            get; set;
        }

        [ManyToOne(Column = "problem_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        public virtual Problem Problem {
            get; set;
        }


        public IDictionary<string, object> Fields {
            get; set;
        }


        /// <summary>
        /// for synchronous operations allows unnecessary serialization operations
        /// </summary>
        private JObject _jObject;

        public virtual string DataMapJsonAsString {
            get {
                return DataMapJson == null ? null : StringExtensions.GetString(CompressionUtil.Decompress(DataMapJson));
            }
            set {
                DataMapJson = CompressionUtil.Compress(value.GetBytes());
            }
        }

        public virtual string SentXmlAsString {
            get {
                return SentXml == null ? null : StringExtensions.GetString(CompressionUtil.Decompress(SentXml));
            }
            set {
                SentXml = CompressionUtil.Compress(value.GetBytes());
            }
        }

        [JsonIgnore]
        public JObject DataMapJSonObject {
            get {
                if (_jObject != null) {
                    return _jObject;
                } else if (Fields != null) {
                    DataMapJsonAsString = JsonConvert.SerializeObject(Fields);
                }
                _jObject = JObject.Parse(DataMapJsonAsString);
                return _jObject;
            }
            set {
                _jObject = value;
            }
        }

        [JsonIgnore]
        public JObject AdditionalData {
            get; set;
        }

    }


}
