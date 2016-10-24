﻿using cts.commons.persistence;
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

        //actually ONETOONE, but who cares, since NHIB doesnt seem to work fine with it
        // [ManyToOne(Column = "dashboard_id", OuterJoin = OuterJoinStrategy.False, Lazy = Laziness.False, Cascade = "none")]
        // public virtual Dashboard DashBoard { get; set; }

        protected bool Equals(DashboardPanelRelationship other) {
            return Equals(Panel, other.Panel);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DashboardPanelRelationship)obj);
        }

        public override int GetHashCode() {
            return (Panel != null ? Panel.GetHashCode() : 0);
        }
    }
}