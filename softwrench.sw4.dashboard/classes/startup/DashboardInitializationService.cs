﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using cts.commons.persistence.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.dashboard.classes.model.entities;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Util;

namespace softwrench.sw4.dashboard.classes.startup {
    public class DashboardInitializationService : ISingletonComponent {

        private readonly FixedUserSWDBHibernateDao _dao = new FixedUserSWDBHibernateDao(new ApplicationConfigurationAdapter());
        private readonly IWhereClauseFacade _whereClauseFacade;

        public DashboardInitializationService(IWhereClauseFacade whereClauseFacade) {
            _whereClauseFacade = whereClauseFacade;
        }

        public bool DashBoardExists(string alias) {
            var parameters = new ExpandoObject();
            var parameterCollection = (ICollection<KeyValuePair<string, object>>)parameters;
            parameterCollection.Add(new KeyValuePair<string, object>("alias", alias));

            var count = _dao.CountByNativeQuery("select count(id) from dash_dashboard where alias=:alias", parameters);
            return count > 0;
        }

        public Dashboard CreateDashboard(string title, string alias, ICollection<DashboardBasePanel> panels) {
            var now = DateTime.Now;

            // save panels and replace references by hibernate-managed ones
            foreach (var panel in panels) {
                panel.CreationDate = now;
                panel.UpdateDate = now;
                panel.Visible = true;
                panel.Filter = new DashboardFilter();
                var graphicPanel = panel as DashboardGraphicPanel;
                if (graphicPanel != null) {
                    graphicPanel.Provider = "swChart";
                }
            }
            panels = _dao.BulkSave(panels).ToList();

            // create relationship entities
            var position = 0;
            var panelRelationships = panels
                .Select(p => new DashboardPanelRelationship() {
                    Position = position++,
                    Panel = p
                })
                .ToList();

            // create dashboard
            var dashboard = new Dashboard() {
                Filter = new DashboardFilter(),
                CreationDate = now,
                UpdateDate = now,
                Alias = alias,
                System = true,
                Application = alias,
                Title = title,
                Panels = panelRelationships,
                Active = true
            };

            return _dao.Save(dashboard);
        }

        public void RegisterWhereClause(string application, string query, string alias, string metadataId) {
            _whereClauseFacade.Register(application, query, new WhereClauseRegisterCondition() {
                Alias = alias,
                AppContext = new ApplicationLookupContext() {
                    MetadataId = metadataId,
                }
            });
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
                return (int)_dao.FindSingleByNativeQuery<object>("select id from sw_user2 where username = ?", "swadmin");
            }
        }

    }
}
