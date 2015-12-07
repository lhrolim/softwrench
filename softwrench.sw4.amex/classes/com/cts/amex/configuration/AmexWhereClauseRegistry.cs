﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.amex.classes.com.cts.amex.configuration {
    public class AmexWhereClauseRegistry : ISingletonComponent {
        public AmexWhereClauseRegistry(IWhereClauseFacade facade) {
            if (!ApplicationConfiguration.ClientName.Equals("amex")) {
                return;
            }
            facade.Register("asset", "@amexWhereClauseRegistry.RrfdListWhereClauseProvider", new WhereClauseRegisterCondition() {
                AppContext = new ApplicationLookupContext() {
                    Schema = "rrfdlist"
                }
            });
        }

        public string RrfdListWhereClauseProvider() {
            if (!ApplicationConfiguration.ClientName.Equals("amex")) {
                return null;
            }

            var user = SecurityFacade.CurrentUser();
            //amex relies on DCI_SITE rather than siteid for filtering their data
            var dciSite = user.Genericproperties["DCI_SITE"];
            return string.Format("ASSET.DCI_SITE = '{0}' AND ASSET.STATUS in ('NOT READY','SPACE PLANNED')", dciSite);
        }

    }
}
