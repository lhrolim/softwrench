using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using cts.commons.simpleinjector.Events;
using JetBrains.Annotations;
using log4net;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using softWrench.sW4.Metadata.Menu.Containers;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Metadata.Menu {
    public class MenuSecurityManager : ISWEventListener<ClearMenuEvent>, ISWEventListener<ClearCacheEvent> {
        public void HandleEvent(ClearMenuEvent eventToDispatch) {
            SecurityFacade.CurrentUser().ClearMenu();
        }

        public void HandleEvent(ClearCacheEvent eventToDispatch) {
            SecurityFacade.CurrentUser().ClearMenu();
            SecurityFacade.CurrentUser().ClearBars();
        }

        private readonly ILog _log = LogManager.GetLogger(typeof(MenuSecurityManager));

        private const string BlankUser = "menu is blank for user {0} review his security configuration";
        private const string MenuNotFound = "menu not found for platform {0}. ";
        private const string MenuConcurrencyIssue = "Concurrency error when adding menu to cache, already there";

        [CanBeNull]
        public MenuDefinition Menu([NotNull]InMemoryUser user, ClientPlatform platform, out bool fromCache) {

            if (user.CachedMenu.ContainsKey(platform)) {
                fromCache = true;
                return user.CachedMenu[platform];
            }
            fromCache = false;

            var unsecureMenu = MetadataProvider.Menu(platform);
            if (unsecureMenu == null) {
                _log.Warn(string.Format(MenuNotFound, platform));
                return null;
            }

            var secureLeafs = new List<MenuBaseDefinition>();
            if (unsecureMenu.Leafs != null) {
                foreach (var leaf in unsecureMenu.Leafs) {
                    if (leaf is MenuContainerDefinition) {
                        var secured = ((MenuContainerDefinition)leaf).Secure(user, platform);
                        if (secured != null) {
                            secureLeafs.Add(secured);
                        }
                    } else {
                        if (user.IsSwAdmin()) {
                            secureLeafs.Add(leaf);
                        } else {
                            if (MenuContainerExtensions.PassesSecurityCheck(leaf, user.MergedUserProfile, platform)) {
                                secureLeafs.Add(leaf);
                            }
                        }
                    }
                }
            }
            if (!secureLeafs.Any()) {
                _log.Warn(string.Format(BlankUser, user.Login));
            }
            var menuDefinition = new MenuDefinition(secureLeafs, unsecureMenu.MainMenuDisplacement.ToString(), unsecureMenu.ItemindexId);
            try {
                user.CachedMenu.Add(platform, menuDefinition);
                // ReSharper disable once EmptyGeneralCatchClause
            } catch {
                _log.Warn(string.Format(MenuConcurrencyIssue));
            }
            return menuDefinition;
        }


        public MenuBaseDefinition GetIndexMenuForUser(ClientPlatform platform, InMemoryUser user) {

            bool fromCache;
            var menu = this.Menu(user, platform, out fromCache);
            if (menu == null) {
                return null;
            }

            MenuBaseDefinition indexItem = null;
            var indexItemId = menu.ItemindexId;
            indexItem = menu.ExplodedLeafs.FirstOrDefault(l => indexItemId.EqualsIc(l.Id));
            if (indexItem == null) {
                //first we´ll try to get the item declared, if it´s null (that item is role protected for that user, for instance, let´s pick the first leaf one as a fallback to avoid problems
                indexItem = menu.ExplodedLeafs.FirstOrDefault(a => a.Leaf);
            }
            return indexItem;
        }

    }
}
