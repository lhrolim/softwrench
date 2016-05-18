using System;
using System.Collections.Generic;
using cts.commons.portable.Util;
using NHibernate.Mapping.Attributes;
using Newtonsoft.Json;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Entities {
    [Class(Table = "SW_METADATAEDITOR", Lazy = false)]
    public class Metadataeditor {

        public const String ByDefaultId = "from Metadataeditor where DefaultId = 1";

        public const String ByFileName = "from Metadataeditor where Name= '{0}'";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property(Type = "BinaryBlob")]
        [JsonIgnore]
        public virtual byte[] Metadata { get; set; }
        [Property]
        public virtual string Comments { get; set; }

        [Property]
        public virtual DateTime  CreatedDate{ get; set; }

        [Property]
        public virtual int DefaultId { get; set; }

        [Property]
        public virtual string Name { get; set; }

        [Property]
        public virtual string Path { get; set; }

        [Property]
        public virtual string BaselineVersion { get; set; }

        [Property]
        public virtual string ChangedBy { get; set; }


        public virtual string SystemStringValue
        {
            get
            {
                return StringExtensions.GetString(CompressionUtil.Decompress(Metadata)) ;
            }
            set
            {
                
                    Metadata = CompressionUtil.Compress(value.GetBytes());
             }
        }
       
        public override string ToString() {
            return string.Format("Metadata: {0}, CreatedDate: {1}, DefaultId: {2}", Metadata, CreatedDate, DefaultId);
        }

        private sealed class IdEqualityComparer : IEqualityComparer<Metadataeditor>
        {
            public bool Equals(Metadataeditor x, Metadataeditor y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id == y.Id;
            }

            public int GetHashCode(Metadataeditor obj)
            {
                return obj.Id.GetHashCode();
            }
        }

        private static readonly IEqualityComparer<Metadataeditor> IdComparerInstance = new IdEqualityComparer();

        public static IEqualityComparer<Metadataeditor> IdComparer
        {
            get { return IdComparerInstance; }
        }

    }
}

