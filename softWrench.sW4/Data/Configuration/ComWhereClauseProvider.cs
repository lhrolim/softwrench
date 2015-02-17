using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Configuration {
    public class ComWhereClauseProvider : ISingletonComponent {
        private readonly IContextLookuper _contextLookuper;
        public ComWhereClauseProvider(IContextLookuper contextLookuper) {
            _contextLookuper = contextLookuper;
        }

        public String SrGridQuery() {
            var owner = GetOwnerFromUser();
            return OwnerQuery.Fmt("SR", owner);
        }

        public String WorkOrderGridQuery() {
            var owner = GetOwnerFromUser();
            return OwnerQuery.Fmt("workorder", owner);
        }

        private static string GetOwnerFromUser() {
            var user = SecurityFacade.CurrentUser();
            return user.Login.ToUpper();
        }

        internal const string OwnerQuery = @"
        {0}.owner = '{1}'";
    }
}
