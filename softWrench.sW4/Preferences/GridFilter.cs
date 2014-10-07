using System;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Interfaces;

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

        [Property]
        public string Alias { get; set; }

        [Property]
        public string QueryString { get; set; }

        [Property]
        public string Application { get; set; }

        [Property]
        public string Schema { get; set; }

        [Property]
        public DateTime CreationDate { get; set; }

        public override string ToString() {
            return string.Format("Alias: {0}, Application: {1}, QueryString: {2}", Alias, Application, QueryString);
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
