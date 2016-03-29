using System;
using System.Collections.Generic;
using cts.commons.persistence;
using cts.commons.portable.Util;
using Iesi.Collections.Generic;
using Newtonsoft.Json;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.dashboard.classes.model.entities {

    [Class(Table = "DASH_DASHBOARD", Lazy = false)]
    public class Dashboard : IBaseAuditEntity {

        public static string ByUser(IEnumerable<int?> profiles) {
            return "from Dashboard where (userid is null or userid = ?) and (userprofiles is null or {0})".Fmt(DashboardFilter.GetUserProfileString(profiles));
        }

        public static string ByUserNoProfile = "from Dashboard where (userid is null or userid = ?) and (userprofiles is null)";

        public static string SwAdminQuery = "from Dashboard ";

        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id { get; set; }

        [Property]
        public string Title { get; set; }

        [Property]
        public DateTime CreationDate { get; set; }

        [Property]
        public DateTime? UpdateDate { get; set; }

        [Property]
        public int? CreatedBy { get; set; }


        /// <summary>
        /// comma separated list of columns. (3,1,2 means 3 columns on first row, 1 on the second and 2 on the third).
        /// </summary>
        //[Property]
        //public string Layout { get; set; }

        [Set(0, Table = "DASH_DASHBOARDREL",
        Lazy = CollectionLazy.False, Cascade = "all")]
        [Key(1, Column = "dashboard_id")]
        [OneToMany(2, ClassType = typeof(DashboardPanelRelationship))]
        [JsonIgnore]

        public Iesi.Collections.Generic.ISet<DashboardPanelRelationship> PanelsSet { get; set; }


        //Adapter cause asp.net won´t serialize interfaces
        public List<DashboardPanelRelationship> Panels {
            get {
                if (PanelsSet != null) {
                    return new List<DashboardPanelRelationship>(PanelsSet);
                }
                return null;
            }
            set {
                PanelsSet = new HashedSet<DashboardPanelRelationship>(value);
            }
        }



        [ComponentProperty]
        public DashboardFilter Filter { get; set; }


    }
}
