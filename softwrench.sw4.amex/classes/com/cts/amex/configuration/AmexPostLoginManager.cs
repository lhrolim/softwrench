using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.amex.classes.com.cts.amex.configuration {
    public class AmexPostLoginManager : ISWEventListener<UserLoginEvent> {

        private readonly MaximoHibernateDAO _dao;

        public AmexPostLoginManager(MaximoHibernateDAO dao) {
            _dao = dao;
        }


        public void HandleEvent(UserLoginEvent userEvent) {

            if (ApplicationConfiguration.ClientName != "amex") {
                return;
            }

            var user = userEvent.InMemoryUser;
            if (user.MaximoPersonId != null) {
                var dciSite = _dao.FindSingleByNativeQuery<string>("select DCI_SITE from person where personid = ?",
                    user.MaximoPersonId);
                user.Genericproperties["DCI_SITE"] = dciSite;
            } else {
                //this would cause all of the grids to show no data
                user.Genericproperties["DCI_SITE"] = "#$null";
            }

        }
    }
}
