using JetBrains.Annotations;
using softwrench.sW4.Shared2.Metadata.Applications;
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

        [NotNull]
        public MenuAndTopNav Get(ClientPlatform platform) {
            try {
                var user = SecurityFacade.CurrentUser();
                var isSysAdmin = user.Roles.Any(r => r.Name == "sysadmin") || ApplicationConfiguration.IsLocal();
                var isClientAdmin = user.Roles.Any(r => r.Name == "clientadmin") || ApplicationConfiguration.IsLocal();
                var securedMenu = user.Menu(platform);
                return new MenuAndTopNav(securedMenu, isSysAdmin, isClientAdmin);
            }
            catch (InvalidOperationException) {
                FormsAuthentication.SignOut();
                return new MenuAndTopNav(MetadataProvider.Menu(platform), false, false);
            }
        }

        public class MenuAndTopNav {
            private readonly MenuDefinition _menu;
            private readonly bool _isSysAdmin;
            private readonly bool _isClientAdmin;

            public MenuAndTopNav(MenuDefinition menu, bool isSysAdmin, bool isClientAdmin) {
                _menu = menu;
                this._isSysAdmin = isSysAdmin;
                this._isClientAdmin = isClientAdmin;
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
        }
    }
}