using System.Collections.Generic;
using NHibernate.Hql.Ast.ANTLR;
using SimpleInjector;
using softwrench.sw4.api.classes;
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

        private readonly IEnumerable<IMenuManager> _managers;

        public MenuController(Container container) {
            _managers = container.GetAllInstances<IMenuManager>();
        }


        public SecuredDisplayables Get(ClientPlatform platform) {
            try {
                var user = SecurityFacade.CurrentUser();
                var isSysAdmin = user.IsInRole("sysadmin") || ApplicationConfiguration.IsLocal();
                var isClientAdmin = user.IsInRole("clientadmin") || ApplicationConfiguration.IsLocal();
                bool fromCache;
                var securedMenu = user.Menu(platform, out fromCache);
                if (!user.Genericproperties.ContainsKey("menumanagerscached")) {
                    //to avoid adding items multiple times
                    foreach (var menuManager in _managers) {
                        securedMenu = menuManager.ModifyMenu(securedMenu, user);
                    }
                    user.Genericproperties["menumanagerscached"] = true;
                }
                var securedBars = user.SecuredBars(platform, MetadataProvider.CommandBars());
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

            public SecuredDisplayables(MenuDefinition menu, IDictionary<string, CommandBarDefinition> commandBars, bool isSysAdmin, bool isClientAdmin) {
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