using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.Security;

namespace softWrench.sW4.Web.Controllers {
    public class MenuController : ApiController {

        public SecuredDisplayables Get(ClientPlatform platform) {
            try {
                var user = SecurityFacade.CurrentUser();
                var isSysAdmin = user.Roles.Any(r => r.Name == "sysadmin") || ApplicationConfiguration.IsLocal();
                var isClientAdmin = user.Roles.Any(r => r.Name == "clientadmin") || ApplicationConfiguration.IsLocal();
                var securedMenu = user.Menu(platform);
                var securedBars = user.SecuredBars(MetadataProvider.CommandBars());
                return new SecuredDisplayables(securedMenu, securedBars, isSysAdmin, isClientAdmin);
            } catch (InvalidOperationException) {
                FormsAuthentication.SignOut();
                return null;
            }
        }

        public class SecuredDisplayables {
            private readonly MenuDefinition _menu;
            private readonly IDictionary<string, CommandBarDefinition> _commandBars;
            private readonly bool _isSysAdmin;
            private readonly bool _isClientAdmin;

            public SecuredDisplayables(MenuDefinition menu, IDictionary<string,CommandBarDefinition> commandBars, bool isSysAdmin, bool isClientAdmin) {
                _menu = menu;
                _isSysAdmin = isSysAdmin;
                _isClientAdmin = isClientAdmin;
                _commandBars = commandBars;
            }

            public MenuDefinition Menu {
                get { return _menu; }
            }

            public bool IsSysAdmin {
                get { return _isSysAdmin; }
            }

            public bool IsClientAdmin {
                get { return _isClientAdmin; }
            }

            public IDictionary<string, CommandBarDefinition> CommandBars {
                get { return _commandBars; }
            }
        }
    }
}