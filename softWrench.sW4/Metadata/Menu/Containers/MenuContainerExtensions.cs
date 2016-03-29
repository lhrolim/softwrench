using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using softwrench.sw4.user.classes.entities;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Metadata.Menu;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Menu.Containers {
    public static class MenuContainerExtensions {


        //        public static bool IsRestrictedByRole(this MenuBaseDefinition leaf, InMemoryUser user) {
        //            var roles = user.Roles;
        //            string applicationBasedRole = null;
        //            if (leaf is MenuContainerDefinition) {
        //                var applicationRef = ((MenuContainerDefinition)leaf).ApplicationContainer;
        //                if (MetadataProvider.ApplicationRoleAlias.ContainsKey(applicationRef)) {
        //                    //this means for instance, that a menu protecting a workorder application could be activated by a role name called workorder, regardless of the the name of the role property of the menu itself
        //                    applicationBasedRole = applicationRef;
        //                }
        //            } else if (leaf.RoleDefinedByParent) {
        //                applicationBasedRole = ((ApplicationMenuItemDefinition)leaf).Application;
        //            }
        //            return !roles.Any(r => r.Active && (r.Name.EqualsIc(leaf.Role) || (applicationBasedRole != null && r.Name.EqualsIc(applicationBasedRole))));
        //        }

        public static bool PassesSecurityCheck(MenuBaseDefinition leaf, MergedUserProfile mergedUserProfile, ClientPlatform platform) {

            #region legacy support
            string applicationBasedRole = null;
            if (leaf is MenuContainerDefinition) {
                var applicationRef = ((MenuContainerDefinition)leaf).ApplicationContainer;
                if (MetadataProvider.ApplicationRoleAlias.ContainsKey(applicationRef)) {
                    //this means for instance, that a menu protecting a workorder application could be activated by a role name called workorder, regardless of the the name of the role property of the menu itself
                    applicationBasedRole = applicationRef;
                }
            } else if (leaf.RoleDefinedByParent && leaf is ApplicationMenuItemDefinition) {
                applicationBasedRole = ((ApplicationMenuItemDefinition)leaf).Application;
            }

            var legacyRolePresent =
                mergedUserProfile.Roles.Any(r => r.Active && (r.Name.EqualsIc(leaf.Role) || (applicationBasedRole != null && r.Name.EqualsIc(applicationBasedRole))));
            if (legacyRolePresent) {
                return true;
            }
            #endregion

            var permissionExpression = leaf.PermissionExpresion;

            if (!string.IsNullOrEmpty(permissionExpression) && !ApplicationConfiguration.IsUnitTest && !GenericSwMethodInvoker.Invoke<bool>(null, permissionExpression)) {
                return false;
            }


            if (leaf is ApplicationMenuItemDefinition) {
                var appLeaf = (ApplicationMenuItemDefinition)leaf;
                var application = mergedUserProfile.GetPermissionByApplication(appLeaf.Application);
                if (application == null) {
                    //not allowed by default, no permission rule
                    return false;
                }
                if (application.HasNoPermissions) {
                    return false;
                }
                var schema = MetadataProvider.Schema(appLeaf.Application, appLeaf.Schema, platform);
                if (schema != null && schema.IsCreation() && !application.AllowCreation) {
                    return false;
                }
                return true;
            }
            if (leaf.Role == null) {
                return true;
            }
            return !mergedUserProfile.Roles.Any(r => r.Active && (r.Name.EqualsIc(leaf.Role)));
        }


        public static MenuBaseDefinition Secure(this MenuContainerDefinition container, InMemoryUser user, ClientPlatform platform) {
            var secureLeafs = new List<MenuBaseDefinition>();
            foreach (var leaf in container.Leafs) {
                if (leaf is MenuContainerDefinition) {
                    var secured = ((MenuContainerDefinition)leaf).Secure(user, platform);
                    if (secured != null) {
                        secureLeafs.Add(secured);
                    }
                } else {
                    if (user.IsSwAdmin()) {
                        secureLeafs.Add(leaf);
                    } else {
                        if (PassesSecurityCheck(leaf, user.MergedUserProfile, platform)) {
                            secureLeafs.Add(leaf);
                        }
                    }
                }
            }
            var permissionExpression = container.PermissionExpresion;

            if (!string.IsNullOrEmpty(permissionExpression) && (!ApplicationConfiguration.IsUnitTest && !GenericSwMethodInvoker.Invoke<bool>(null, permissionExpression))) {
                return null;
            }

            return !secureLeafs.Any() ? null : new MenuContainerDefinition(container.Id,
                container.Title, container.Role, container.Tooltip, container.Icon, container.Module, container.Controller, container.Action, container.HasMainAction, container.CustomizationPosition, container.PermissionExpresion, secureLeafs);
        }
    }
}
