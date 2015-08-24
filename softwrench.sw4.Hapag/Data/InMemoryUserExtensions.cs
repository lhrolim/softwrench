using cts.commons.portable.Util;
using softwrench.sw4.Hapag.Security;
using softWrench.sW4.Security.Services;
using System;
using System.Linq;

namespace softwrench.sw4.Hapag.Data {
    public static class InMemoryUserExtensions {

        public static string GetPersonGroupsForQuery(string module) {

            var user = SecurityFacade.CurrentUser();
            string[] personGroups = user.PersonGroups.Where(x => "xitc".EqualsIc(module) ^ !HlagLocationUtil.IsSuperGroup(x.PersonGroup))
                        .Select(f => f.PersonGroup.Name)
                        .ToArray();
            if ("xitc".EqualsIc(module)) {
                personGroups =
                    user.PersonGroups.Where(x => HlagLocationUtil.IsSuperGroup(x.PersonGroup))
                        .Select(f => f.PersonGroup.Name)
                        .ToArray();
            } else {
                personGroups =
                    user.PersonGroups.Where(x => !HlagLocationUtil.IsSuperGroup(x.PersonGroup))
                        .Select(f => f.PersonGroup.Name)
                        .ToArray();
            }

            var strPersonGroups = String.Join("','", personGroups);

            strPersonGroups = "'" + strPersonGroups + "'";
            return strPersonGroups;
        }
    }
}
