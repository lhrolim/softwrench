using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using cts.commons.Util;
using NHibernate.Mapping.Attributes;
using NHibernate.SqlCommand;

namespace softwrench.sw4.dynforms.classes.model.entity {


    [Class(Table = "FORM_METADATA_OPTIONS", Lazy = false)]
    public class FormMetadataOptions
    {

        public const string AliasQuery = "select id,alias from FORM_METADATA_OPTIONS";


        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id { get; set; }

        [Property]
        public string Alias { get; set; }

        [Property]
        public string Description { get; set; }

        [Property(Type = "BinaryBlob")]
        public byte[] List { get; set; }

        public virtual string ListDefinitionStringValue {
            get { return StringExtensions.GetString(CompressionUtil.Decompress(List)); }
            set { List = CompressionUtil.Compress(value?.GetBytes()); }
        }

    }
}
