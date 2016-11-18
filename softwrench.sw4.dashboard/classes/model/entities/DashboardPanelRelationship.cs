using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.dashboard.classes.model.entities {

    [Class(Table = "DASH_DASHBOARDREL", Lazy = false)]
    public class DashboardPanelRelationship : IBaseEntity {

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        /// <summary>
        /// holds the position of panel inside of a dashboard
        /// </summary>
        [Property]
        public virtual int Position {
            get; set;
        }

        //actually ONETOONE, but who cares, since NHIB doesnt seem to work fine with it
        [ManyToOne(Column = "panel_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        public virtual DashboardBasePanel Panel {
            get; set;
        }

        #region JSON-oriented parameters
        //these are set on client side, since the deserialization of the json is failing, probably due to a presence of an hierarchy

        public virtual int? PanelId {
            get; set;
        }

        public virtual string PanelType {
            get; set;
        }
        #endregion



        //actually ONETOONE, but who cares, since NHIB doesnt seem to work fine with it
        // [ManyToOne(Column = "dashboard_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        // public virtual Dashboard DashBoard { get; set; }

        protected bool Equals(DashboardPanelRelationship other) {
            return Equals(Panel, other.Panel) && PanelId == other.PanelId && string.Equals(PanelType, other.PanelType);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((DashboardPanelRelationship)obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (Panel != null ? Panel.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ PanelId.GetHashCode();
                hashCode = (hashCode * 397) ^ (PanelType != null ? PanelType.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
