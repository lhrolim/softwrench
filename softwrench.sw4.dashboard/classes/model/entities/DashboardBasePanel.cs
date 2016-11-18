using System;
using cts.commons.persistence;
using cts.commons.portable.Util;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.dashboard.classes.model.entities {

    /// <summary>
    /// BAse class for panels that can go inside of dashboards. Implementations can be grids, graphics, etc
    /// </summary>
    [Class(Table = "DASH_BASEPANEL", Lazy = false)]
    public abstract class DashboardBasePanel : IBaseAuditEntity {

        public static string ByUser(string panelType, int?[] enumerable) {
            return "from {0} where (userid is null or userid = ?) and (userprofiles is null or {0})".Fmt(panelType, DashboardFilter.GetUserProfileString(enumerable));
        }

        public static string ByUserNoProfile(string panelType) {
            return "from {0} where (userid is null or userid = ?) and (userprofiles is null)".Fmt(panelType);
        }

        public static string SwAdminQuery(string panelType) {
            return "from {0}".Fmt(panelType);
        }


        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id {
            get; set;
        }

        [Property(Column = "alias_")]
        public virtual string Alias {
            get; set;
        }

        [Property]
        public virtual string Title {
            get; set;
        }

        private bool _visible = true;

        [ComponentProperty]
        public virtual DashboardFilter Filter {
            get; set;
        }

        [Property]
        public DateTime CreationDate {
            get; set;
        }
        [Property]
        public DateTime? UpdateDate {
            get; set;
        }
        [Property]
        public int? CreatedBy {
            get; set;
        }

        [Property]
        public int Size {
            get; set;
        }

        public virtual string WhereClause {
            get; set;
        }

        /// <summary>
        /// Transient property that can make the panel invisible due to a presence of a role blocking it, for instance, or some other business logic
        /// </summary>
        public bool Visible {
            get {
                return _visible;
            }
            set {
                _visible = value;
            }
        }

        public string Type {
            get {
                return GetType().Name;
            }
        }

        protected bool Equals(DashboardBasePanel other) {
            return string.Equals(Alias, other.Alias);
        }

        public abstract string GetApplicationName();

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DashboardBasePanel)obj);
        }

        public override int GetHashCode() {
            return (Alias != null ? Alias.GetHashCode() : 0);
        }
    }
}
