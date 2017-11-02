using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Data.Entities.Attachment {

    [Class(Table = "SW_DOCLINK", Lazy = false)]
    public class DocLink : IBaseEntity {

        public static string ByOwnerTableAndId = "from DocLink where ownertable =? and ownerid =?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id { get; set; }

        [Property]
        public string Document { get; set; }

        [Property]
        public string Extension { get; set; }

        [Property]
        public string Description { get; set; }

        [Property]
        public string OwnerTable { get; set; }

        [Property]
        public long OwnerId { get; set; }

        [Property]
        public int CreateBy { get; set; }

        [Property]
        public DateTime? CreateDate { get; set; }

        [ManyToOne(Column = "docinfo_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.Proxy, Cascade = "all")]
        public DocInfo DocInfo { get; set; }

        [Set(0, Inverse = true, Lazy = CollectionLazy.False)]
        [Key(1, Column = "doclink_id")]
        [OneToMany(2, ClassType = typeof(DocLinkQualifier))]
        public ISet<DocLinkQualifier> Qualifiers { get; set; } = new HashSet<DocLinkQualifier>();

    }
}
