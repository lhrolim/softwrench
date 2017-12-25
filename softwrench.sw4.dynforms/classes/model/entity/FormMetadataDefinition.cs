using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.Util;
using NHibernate.Mapping.Attributes;
using NHibernate.SqlTypes;
using NHibernate.Type;

namespace softwrench.sw4.dynforms.classes.model.entity {



    [Class(Table = "FORM_METADATA_DEF", Lazy = false)]
    public class FormMetadataDefinition : IBaseEntity {

        public static string ByMetadataId = "from FormMetadataDefinition where Metadata.Name = ? ";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id { get; set; }

        //        [Property]
        //        public string DetailDefinition { get; set; }
        //
        //        [Property]
        //        public string NewDetailDefinition { get; set; }
        //
        //        [Property]
        //        public string ListDefinition { get; set; }


        [Property(Type = "BinaryBlob")]
        public byte[] DetailSerialized { get; set; }

        [Property(Type = "BinaryBlob")]
        public byte[] ListSerialized { get; set; }

        [Property(Type = "BinaryBlob")]
        public byte[] NewDetailSerialized { get; set; }


        public virtual string DetailDefinitionStringValue {
            get { return StringExtensions.GetString(CompressionUtil.Decompress(DetailSerialized)); }
            set { DetailSerialized = CompressionUtil.Compress(value?.GetBytes()); }
        }

        public virtual string NewDetailDefinitionStringValue {
            get {
                if (NewDetailSerialized == null) {
                    return DetailDefinitionStringValue;
                }
                return StringExtensions.GetString(CompressionUtil.Decompress(NewDetailSerialized));
            }
            set { DetailSerialized = CompressionUtil.Compress(value?.GetBytes()); }
        }


        public virtual string ListDefinitionStringValue {
            get { return StringExtensions.GetString(CompressionUtil.Decompress(ListSerialized)); }
            set { ListSerialized = CompressionUtil.Compress(value?.GetBytes()); }
        }



        [ManyToOne(Column = "form_name", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, NotNull = true, Cascade = "none")]
        public FormMetadata Metadata {
            get; set;
        }


//        public static FormMetadataDefinition FromXmls(string listXmlDefinition, string detailXmlDefinition) {
//            return new FormMetadataDefinition() {
//                DetailDefinition = detailXmlDefinition,
//                ListDefinition = listXmlDefinition
//            };
//        }
    }
}
