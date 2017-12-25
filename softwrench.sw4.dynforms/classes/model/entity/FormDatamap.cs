using System;
using cts.commons.persistence;
using cts.commons.persistence.Util;
using cts.commons.portable.Util;
using cts.commons.Util;
using NHibernate.Mapping.Attributes;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Metadata.Validator;

namespace softwrench.sw4.dynforms.classes.model.entity {

    [Class(Table = "FORM_DATAMAP", Lazy = false)]
    public class FormDatamap
    {

        public static string FormDmQuery = @"select FormDatamapId,Datamap from FormDatamap where FormMetadata.Name = ?";


        [Id(0, Name = "FormDatamapId")]
        [Generator(1, Class = "native")]
        public int? FormDatamapId { get; set; }

        [Property]
        [UserIdProperty]
        public string UserId { get; set; }

        [ManyToOne(Column = "form_name", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "all")]
        public FormMetadata FormMetadata { get; set; }

        [Property]
        public DateTime? ChangeDate { get; set; }

        [ManyToOne(Column = "changeby", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "all")]
        public User ChangeBy { get; set; }

        [Property(Length = MigrationUtil.StringMax)]
        public string Datamap { get; set; }

//        /// <summary>
//        /// whether the byte[] stored is compressed or not.
//        /// non-compressed is useful for testing data
//        /// </summary>
//        [Property]
//        public bool Compressed { get; set; }


//        public virtual string DefinitionStringValue {
//            get { return StringExtensions.GetString(CompressionUtil.Decompress(Datamap)); }
//            set { Datamap = CompressionUtil.Compress(value?.GetBytes()); }
//        }
    }
}
