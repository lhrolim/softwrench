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

        private const string BY_USER_PROFILES_APPLICATIONS_TEMPLATE = "from Dashboard where (userid is null or userid = :p0) and (userprofiles is null or {0}) and (application is null or application in (:p1))";
        private const string BY_USER_PROFILES_APPLICATIONS_ACTIVE_TEMPLATE = BY_USER_PROFILES_APPLICATIONS_TEMPLATE + " and active is true order by preferredorder";

        public static string ByUserAndApplications(IEnumerable<int?> profiles, bool includeInactive = false) {
            return includeInactive
                ? (string.Format(BY_USER_PROFILES_APPLICATIONS_TEMPLATE, DashboardFilter.GetUserProfileString(profiles)) + "order by preferredorder")
                : string.Format(BY_USER_PROFILES_APPLICATIONS_ACTIVE_TEMPLATE, DashboardFilter.GetUserProfileString(profiles));
        }

        public const string ByUserAndApplicationsNoProfile = "from Dashboard where (userid is null or userid = :p0) and (userprofiles is null) and (application is null or application in (:p1)) and active is true order by preferredorder";

        public const string SwAdminQuery = "from Dashboard where active is true order by preferredorder";


        [Id(0, Name = "Id")]
        [Generator(1, Class = "native")]
        public int? Id {
            get; set;
        }

        [Property]
        public string Title {
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
        public bool Active {
            get; set;
        }

        [Property]
        public bool System {
            get; set;
        }

        [Property]
        public string Alias {
            get; set;
        }

        [Property]
        public string Application {
            get; set;
        }

        [Property]
        public int PreferredOrder {
            get; set;
        }

        public bool Cloning {
            get; set;
        }

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
        public ISet<DashboardPanelRelationship> PanelsSet {
            get; set;
        }

        //Adapter cause asp.net won´t serialize interfaces
        public List<DashboardPanelRelationship> Panels {
            get {
                return PanelsSet != null ? new List<DashboardPanelRelationship>(PanelsSet) : null;
            }
            set {
                PanelsSet = new LinkedHashSet<DashboardPanelRelationship>(value);
            }
        }

        public void PopulatePanelRelationshipsForStorage() {
            if (Panels == null) {
                return;
            }

            foreach (var panelRelationship in Panels) {
                if (panelRelationship.Panel == null && panelRelationship.PanelType != null) {
                    panelRelationship.Panel = panelRelationship.PanelType.EqualsIc(typeof(DashboardGridPanel).Name)
                        ? (DashboardBasePanel)new DashboardGridPanel { Id = panelRelationship.PanelId }
                        : new DashboardGraphicPanel { Id = panelRelationship.PanelId };
                }
            }
        }




        [ComponentProperty]
        public DashboardFilter Filter {
            get; set;
        }
    }
}
