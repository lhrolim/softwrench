using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.Util;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Data.Entities.Attachment {

    [Class(Table = "SW_DOCINFO", Lazy = false)]
    public class DocInfo : IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }


        [Property]
        public string Document { get; set; }

        [Property]
        public string Extension { get; set; }

        [Property]
        public string Description { get; set; }

        [Property]
        public string CheckSum { get; set; }

        [Property(Type = "BinaryBlob")]
        [JsonIgnore]
        public virtual byte[] Data { get; set; }

        [Property]
        public string Url { get; set; }


        public static DocInfo FromLink(DocLink dl) {
            return new DocInfo {
                Document = dl.Document,
                Extension = dl.Extension,
                Description = dl.Description
            };
        }

        public virtual string DataStringValue {
            get { return StringExtensions.GetString(CompressionUtil.Decompress(Data)); }
            set { Data = CompressionUtil.Compress(value?.GetBytes()); }
        }

        [JsonIgnore]
        public virtual byte[] DataUncompressed => Util.CompressionUtil.Decompress(Data);

    }
}
