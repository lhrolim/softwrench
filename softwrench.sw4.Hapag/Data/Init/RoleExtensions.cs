using softwrench.sw4.Hapag.Data.Sync;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Entities;
using System;
using System.ComponentModel;
using System.Linq;

namespace softwrench.sw4.Hapag.Data.Init {

    public static class RoleExtensions {

        public static string GetName(this Enum value) {
            return value.ToString().ToLower();
        }

        public static string GetDescription(this ProfileType value) {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name != null) {
                var field = type.GetField(name);
                if (field != null) {
                    var attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null) {
                        return attr.Description;
                    }
                }
            }
            return null;
        }


        public static string GetHapagPersonGroupName(this FunctionalRole value) {
            switch (value) {
                case FunctionalRole.AssetControl:
                    return HapagPersonGroupConstants.Actrl;
                case FunctionalRole.AssetRamControl:
                    return HapagPersonGroupConstants.ActrlRam;
                case FunctionalRole.Purchase:
                    return HapagPersonGroupConstants.Purchase;
                case FunctionalRole.XItc:
                    return HapagPersonGroupConstants.XITC;
                case FunctionalRole.Itom:
                    return HapagPersonGroupConstants.Itom;
                case FunctionalRole.Tom:
                    return HapagPersonGroupConstants.Tom;
                case FunctionalRole.Ad:
                    return HapagPersonGroupConstants.Ad;
                case FunctionalRole.Change:
                    return HapagPersonGroupConstants.Change;
                case FunctionalRole.Offering:
                    return HapagPersonGroupConstants.Offering;
                case FunctionalRole.Sso:
                    return HapagPersonGroupConstants.SSO;
                case FunctionalRole.Tui:
                    return HapagPersonGroupConstants.Tui;

            }
            return null;
        }

        public static bool HasProfile(this InMemoryUser value, ProfileType profile) {
            if (value.Profiles == null) {
                return false;
            }
            return value.Profiles.Contains(new UserProfile() { Name = profile.GetName() });
        }

        public static bool HasPersonGroup(this InMemoryUser value, string groupName) {
            if (value.PersonGroups == null) {
                return false;
            }
            return value.PersonGroups.Any(p => p.PersonGroup.Name == groupName);
        }

        public static bool IsWWUser(this InMemoryUser user) {
            return user.PersonGroups.Any(p => p.PersonGroup.Name.Equals(HapagPersonGroupConstants.HapagWWGroup));
        }

        public static bool IsInModule(this ContextHolder ctx, FunctionalRole role) {
            return role.GetName().Equals(ctx.Module, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsInAnyModule(this ContextHolder ctx, params FunctionalRole[] roles) {
            return roles.Any(role => role.GetName().Equals(ctx.Module, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
