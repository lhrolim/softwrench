using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using cts.commons.simpleinjector;
using SimpleInjector;
using softwrench.sw4.api.classes;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Menu;
using softWrench.sW4.Metadata.Menu;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Common;
using softWrench.sW4.Web.Models.Home;
using softWrench.sW4.Web.Security;
using softwrench.sw4.user.classes.entities;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Data.Configuration;

namespace softWrench.sW4.Web.Controllers.MenuHelper {
    public class MenuHelper : ISingletonComponent {

        private readonly IEnumerable<IMenuManager> _managers;
        private readonly MenuSecurityManager _menuSecurityManager;
        private readonly ContextLookuper _contextLookuper;
        private readonly ConfigurationFacade _configurationFacade;

        public MenuHelper(Container container, MenuSecurityManager menuSecurityManager, ContextLookuper contextLookuper, ConfigurationFacade configurationFacade) {
            _menuSecurityManager = menuSecurityManager;
            _contextLookuper = contextLookuper;
            _configurationFacade = configurationFacade;
            _managers = container.GetAllInstances<IMenuManager>();
        }




        public MenuModel BuildMenu(ClientPlatform platform) {
            try {
                var user = SecurityFacade.CurrentUser();
                var isSysAdmin = user.IsInRole(Role.SysAdmin) || (ApplicationConfiguration.IsLocal() && _contextLookuper.LookupContext().MockSecurity);
                var isClientAdmin = user.IsInRole(Role.ClientAdmin) || (ApplicationConfiguration.IsLocal() && _contextLookuper.LookupContext().MockSecurity);
                var isDynamicAdmin = user.IsInRolInternal(Role.DynamicAdmin, false) || (ApplicationConfiguration.IsLocal() && _contextLookuper.LookupContext().MockSecurity);
                bool fromCache;
                var securedMenu = _menuSecurityManager.Menu(user, platform, out fromCache);
                if (!user.Genericproperties.ContainsKey("menumanagerscached")) {
                    //to avoid adding items multiple times
                    foreach (var menuManager in _managers) {
                        securedMenu = menuManager.ModifyMenu(securedMenu, user);
                    }
                    user.Genericproperties["menumanagerscached"] = true;
                }
                var securedBars = user.SecuredBars(platform, MetadataProvider.CommandBars(platform));
                var myProfileEnabled = _configurationFacade.Lookup<bool>(ConfigurationConstants.MyProfileEnabled);
                return new MenuModel(securedMenu, securedBars, isSysAdmin, isClientAdmin, isDynamicAdmin, myProfileEnabled);
            } catch (InvalidOperationException) {
                FormsAuthentication.SignOut();
                return null;
            }
        }

        public string GetUrlFromAction(IMenuAction item) {
            var action = item.Action;
            if (String.IsNullOrWhiteSpace(action)) {
                action = "Get";
            }
            var controller = item.Controller;
            var actionURL = String.Format("api/generic/{0}/{1}", controller, action);
            string queryString = null;
            if (item.Parameters != null) {
                queryString = String.Join("&", GetParameter(item.Parameters));
            }
            return WebAPIUtil.GetRelativeRedirectURL(actionURL, queryString);
        }

        // Allow for multiple parameters and also make use of the id.  
        public List<String> GetParameter(IDictionary<string, object> parameters) {
            return parameters.Select(param => String.Format("{0}={1}", param.Key, param.Value)).ToList();
        }
    }
}
