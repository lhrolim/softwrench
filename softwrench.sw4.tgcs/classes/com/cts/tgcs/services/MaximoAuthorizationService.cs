using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.services.setup;
using softWrench.sW4.AUTH;
using softWrench.sW4.Data.Entities.SyncManagers;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.services {

    [OverridingComponent(ClientFilters = "tgcs")]
    public class TgcsUserManager : UserManager {

        public TgcsUserManager(UserLinkManager userLinkManager, MaximoHibernateDAO maxDAO, UserSetupEmailService userSetupEmailService, LdapManager ldapManager, UserSyncManager userSyncManager)
            : base(userLinkManager, maxDAO, userSetupEmailService, ldapManager, userSyncManager) {

        }

        public override User CreateMissingDBUser(string userName, bool save = true) {
            return base.CreateMissingDBUser(userName, save);
        }

    }
}
