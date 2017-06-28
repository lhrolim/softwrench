using NHibernate.Mapping.Attributes;

namespace softWrench.sW4.Data.Persistence.SWDB.Entities {

    /// <summary>
    /// Used to store list relationships on a normalized fashion (i.e instead of a comma separated column)
    /// </summary>
    [Class(Table = "GEN_LISTRELATIONSHIP", Lazy = false)]
    public class GenericListRelationship {

        public const string AllOfParent = "from GenericListRelationship where ParentEntity =? and ParentId = ?";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public long? Id {
            get; set;
        }

        [Property]
        public string ParentEntity { get; set; }

        [Property]
        public long ParentId { get; set; }

        [Property]
        public string ParentColumn { get; set; }

        [Property]
        public string Value { get; set; }
    }
}
