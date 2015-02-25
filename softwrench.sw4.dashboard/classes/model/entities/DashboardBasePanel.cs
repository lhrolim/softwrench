using System;
using cts.commons.persistence;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.dashboard.classes.model.entities {

    /// <summary>
    /// BAse class for panels that can go inside of dashboards. Implementations can be grids, graphics, etc
    /// </summary>
    [Class(Table = "DASH_BASEPANEL", Lazy = false)]
    public class DashboardBasePanel : IBaseEntity {
        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property(Column = "alias_")]
        public virtual String Alias { get; set; }

        [Property(Column = "alias_")]
        public virtual String Title { get; set; }


        [ComponentProperty]
        public virtual DashboardFilter Filter { get; set; }


    }
}
