using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.persistence.Util;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Util;
using WebGrease.Css.Extensions;

namespace softwrench.sw4.dashboard.classes.startup {
    public class DashboardInitializationService : ISingletonComponent {

        private readonly FixedUserSWDBHibernateDao _dao = new FixedUserSWDBHibernateDao(new ApplicationConfigurationAdapter());
        private readonly IWhereClauseFacade _whereClauseFacade;

        public DashboardInitializationService(IWhereClauseFacade whereClauseFacade) {
            _whereClauseFacade = whereClauseFacade;
        }

        public bool DashBoardExists(string alias) {
            var parameters = FromDictionary(new Dictionary<string, object> { { "alias", alias } });
            var count = _dao.CountByNativeQuery("select count(id) from dash_dashboard where alias=:alias", parameters);
            return count > 0;
        }

        public bool PanelExists(string alias) {
            var parameters = FromDictionary(new Dictionary<string, object> { { "alias", alias } });
            var count = _dao.CountByNativeQuery("select count(id) from dash_basepanel where alias_=:alias", parameters);
            return count > 0;
        }

        public Dashboard FindByAlias(string alias) {
            return _dao.FindByQuery<Dashboard>("from Dashboard where alias = ?", alias).FirstOrDefault();
        }


        public Dashboard AddPanelsToDashboard(Dashboard dashboard, ICollection<DashboardBasePanel> panels) {
            var hasPanels = dashboard.PanelsSet != null && dashboard.PanelsSet.Any();

            // save panels and replace references by hibernate-managed ones
            panels = MergePanels(dashboard.PanelsSet, panels);

            // create relationship entities
            var initialPosition = hasPanels ? dashboard.PanelsSet.Max(p => p.Position) + 1 : 0;
            var relationships = BuildRelationShips(panels, initialPosition).ToList();

            // add/set panels on dashboard
            if (hasPanels) {
                dashboard.PanelsSet.AddAll(relationships);
            } else {
                dashboard.Panels = relationships;
            }
            return _dao.Save(dashboard);
        }

        private IEnumerable<DashboardPanelRelationship> BuildRelationShips(ICollection<DashboardBasePanel> panels, int initialPosition = 0) {
            return panels.Select(p => new DashboardPanelRelationship() {
                Position = initialPosition++,
                Panel = p
            }).ToList();
        }

        private ICollection<DashboardBasePanel> MergePanels([CanBeNull]ISet<DashboardPanelRelationship> databasePanels, ICollection<DashboardBasePanel> panels) {
            var now = DateTime.Now;
            // save panels and replace references by hibernate-managed ones
            foreach (var panel in panels) {

                if (databasePanels != null) {
                    var matchingPanel = databasePanels.FirstOrDefault(f => f.Panel.Alias.EqualsIc(panel.Alias));
                    if (matchingPanel != null) {
                        //if there was already a panel on the database, let's make sure we're not creating a new one, but rather updating it
                        panel.Id = matchingPanel.Panel.Id;
                        panel.CreatedBy = matchingPanel.Panel.CreatedBy;
                    }
                }

                panel.CreationDate = now;
                panel.UpdateDate = now;
                panel.Visible = true;
                panel.Filter = new DashboardFilter();
                var graphicPanel = panel as DashboardGraphicPanel;
                if (graphicPanel != null) {
                    graphicPanel.Provider = "swChart";
                }
            }
            return _dao.BulkSave(panels).ToList();
        }

        public Dashboard CreateDashboard(string title, string alias, ICollection<DashboardBasePanel> panels) {

            // save panels and replace references by hibernate-managed ones
            panels = MergePanels(null, panels);

            // create relationship entities
            var panelRelationships = BuildRelationShips(panels);

            // create dashboard
            var now = DateTime.Now;
            var dashboard = new Dashboard() {
                Filter = new DashboardFilter(),
                CreationDate = now,
                UpdateDate = now,
                Alias = alias,
                System = true,
                Application = alias,
                Title = title,
                Panels = panelRelationships.ToList(),
                Active = true
            };

            return _dao.Save(dashboard);
        }

        [Transactional(DBType.Swdb)]
        public virtual void RegisterWhereClause(string application, string query, string alias, string metadataId) {
            var lookupContext = new ApplicationLookupContext() { MetadataId = metadataId };

            var wc = _whereClauseFacade.Lookup(application, lookupContext);
            if (wc != null && !wc.IsEmpty() && wc.Query != null && !"1=1".Equals(wc.Query.Trim())) {
                return;
            }

            _whereClauseFacade.Register(application, query, new WhereClauseRegisterCondition() {
                Alias = alias,
                AppContext = lookupContext
            });
        }

        private ExpandoObject FromDictionary(IDictionary<string, object> dict) {
            var parameters = new ExpandoObject();
            var parameterCollection = (ICollection<KeyValuePair<string, object>>)parameters;
            dict.ForEach(e => parameterCollection.Add(e));
            return parameters;
        }

        /// <summary>
        /// Restricts <see cref="GetCreatedByUser"/> to swadmin.Id.
        /// </summary>
        public class FixedUserSWDBHibernateDao : SWDBHibernateDAO {
            private readonly SWDBHibernateDAO _dao;

            public FixedUserSWDBHibernateDao(ApplicationConfigurationAdapter applicationConfiguration) : base(applicationConfiguration, new HibernateUtil(applicationConfiguration)) {
                _dao = new SWDBHibernateDAO(applicationConfiguration, HibernateUtil);
            }

            protected override int? GetCreatedByUser() {
                // force createdby to be 'swadmin' user
                var id = _dao.FindSingleByNativeQuery<object>("select id from sw_user2 where username = ?", "swadmin");
                return Convert.ToInt32(id);
            }
        }

    }
}
