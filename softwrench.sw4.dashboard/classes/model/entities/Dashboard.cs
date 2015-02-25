using cts.commons.persistence;
using Iesi.Collections.Generic;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.dashboard.classes.model.entities {

    [Class(Table = "DASH_DASHBOARD", Lazy = false)]
    public class Dashboard : IBaseEntity
    {

        public static string ByUser = "from Dashboard where (userid is null or userid = ?) or (userprofile is null or userprofiles like ?)";
        public static string ByUserNoProfile = "from Dashboard where (userid is null or userid = ?) or (userprofiles is null)";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id { get; set; }

        [Property]
        public string Title { get; set; }

        /// <summary>
        /// comma separated list of columns. (3,1,2 means 3 columns on first row, 1 on the second and 2 on the third).
        /// </summary>
        [Property]
        public virtual string Layout { get; set; }

        [Set(0, Table = "DASH_DASHBOARDREL",
        Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "dashboard_id")]
        [OneToMany(2, ClassType = typeof(DashboardPanelRelationship))]
        public virtual ISet<DashboardPanelRelationship> Panels { get; set; }


        [ComponentProperty]
        public virtual DashboardFilter Filter { get; set; }


    }
}
