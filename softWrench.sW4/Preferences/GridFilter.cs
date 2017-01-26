using System;
using cts.commons.persistence;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;
using softwrench.sw4.user.classes.entities;

namespace softWrench.sW4.Preferences {

    /// <summary>
    /// Specify a filter to be applied on a grid.
    /// 
    /// Filters can be shared amongst different users, and the GridFilterAssociation table controls the relationship amongst Users and Filters.
    /// 
    /// A Shared Filter cannot be edited. In that case a new filter would appear keeping the original one intact. The user has then the option to remove the original shared one.
    /// 
    /// </summary>
    [Class(Table = "PREF_GRIDFILTER", Lazy = false)]
    public class GridFilter : IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property(Column = "alias_")]
        public string Alias { get; set; }

        [Property]
        public string Fields { get; set; }

        [Property]
        public string Operators { get; set; }

        [Property(Column = "values_")]
        public string Values { get; set; }

        [Property]
        public string Template { get; set; }

        [Property]
        public string Application { get; set; }

        [Property(Column = "schema_")]
        public string Schema { get; set; }

        [Property]
        public DateTime? CreationDate { get; set; }

        [Property]
        public DateTime? UpdateDate { get; set; }

        /// <summary>
        /// Maps to a quick search operation
        /// </summary>
        [Property]
        public string AdvancedSearch { get; set; }

        [Property]
        public string Sort { get; set; }

        [JsonIgnore]
        [ManyToOne(Column = "creator", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        public User Creator { get; set; }

        public int? CreatorId => Creator.Id;

        public override string ToString() {
            return string.Format("Alias: {0}, Application: {4}, Fields: {1}, Operators: {2}, Values: {3}, Sort: {5}", Alias, Fields, Operators, Values, Application, Sort);
        }

        protected bool Equals(GridFilter other) {
            return Id == other.Id;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GridFilter)obj);
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }
    }
}
