using softwrench.sw4.Hapag.Data.Configuration;
using softWrench.sW4.Security.Context;
using softWrench.sW4.SimpleInjector;
using System;

namespace softwrench.sw4.Hapag.Data {
    public class DashboardWhereClauseProvider : ISingletonComponent {
        readonly R0017WhereClauseProvider _r0017WhereClauseProvider;
        readonly IContextLookuper _contextLookuper;

        public DashboardWhereClauseProvider(R0017WhereClauseProvider r0017WhereClauseProvider, IContextLookuper contextLookuper)
        {
            _r0017WhereClauseProvider = r0017WhereClauseProvider;
            _contextLookuper = contextLookuper;
        }

        public string LocalITCDashboardSRWhereClause() {
            //appends EU logic
            return "(" + _r0017WhereClauseProvider.LocalITCSRWhereClause() + ")" + " AND " + HapagQueryConstants.SrITCDashboard();
        }

        public string XITCDashboardSRWhereClause() {
            return "(" + _r0017WhereClauseProvider.SRWhereClause() + ")" + " AND " + HapagQueryConstants.SrITCDashboard();
        }

        public string ITCDashboardIncidentWhereClause() {
            return "(" + _r0017WhereClauseProvider.IncidentWhereClause() + ")" + " AND " + HapagQueryConstants.IncidentITCDashboard();
        }

        public string DashboardIncidentWhereClause() {
            return HapagQueryConstants.IncidentITCDashboard();
        }

        public string DashboardIMACWhereClause()
        {
            var ctx =_contextLookuper.LookupContext();
            var isViewAllOperation = ctx.ApplicationLookupContext != null &&
                                     "list".Equals(ctx.ApplicationLookupContext.Schema);
            return "(" + _r0017WhereClauseProvider.ImacWhereClause() + ")" + " AND " + HapagQueryConstants.ITCOpenImacs(GetUserPersonGroupsUtils(),isViewAllOperation);
        }

        public string ITomDashboardIMACWhereClause() {
            var ctx = _contextLookuper.LookupContext();
            var isViewAllOperation = ctx.ApplicationLookupContext != null &&
                                     ctx.ApplicationLookupContext.Schema.Equals("list");
            return HapagQueryConstants.ITCOpenImacs(GetUserPersonGroupsUtils(),isViewAllOperation);
        }

        public string DashboardAdIncidentsWhereClause() {
            var personGroups = GetUserPersonGroupsUtils();
            return String.Format(HapagQueryConstants.DashBoardADOpenIncidents, personGroups);
        }

        public string AdIncidentsWhereClause() {
            var personGroups = GetUserPersonGroupsUtils();
            return String.Format(HapagQueryConstants.ADOpenIncidents, personGroups);
        }

        internal string GetUserPersonGroupsUtils() {
            var module = _contextLookuper.LookupContext().Module;
            var personGroups = InMemoryUserExtensions.GetPersonGroupsForQuery(module);
            return personGroups;
        }
    }
}
