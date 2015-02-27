using System;
using cts.commons.persistence;
using cts.commons.portable.Util;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.dashboard.classes.model.entities {

    /// <summary>
    /// BAse class for panels that can go inside of dashboards. Implementations can be grids, graphics, etc
    /// </summary>
    [Class(Table = "DASH_BASEPANEL", Lazy = false)]
    public class DashboardBasePanel : IBaseAuditEntity {

        public static string ByUser(string panelType) {
            return "from {0} where (userid is null or userid = ?) or (userprofile is null or userprofiles like ?)".Fmt(panelType);
        }

        public static string ByUserNoProfile(string panelType) {
            return "from {0} where (userid is null or userid = ?) or (userprofiles is null)".Fmt(panelType);
        }


        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public virtual int? Id { get; set; }

        [Property(Column = "alias_")]
        public virtual String Alias { get; set; }

        [Property]
        public virtual String Title { get; set; }


        [ComponentProperty]
        public virtual DashboardFilter Filter { get; set; }

        [Property]
        public DateTime? CreationDate { get; set; }
        [Property]
        public DateTime? UpdateDate { get; set; }
        [Property]
        public int? CreatedBy { get; set; }


    }
}
