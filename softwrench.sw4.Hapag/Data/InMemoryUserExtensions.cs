using softwrench.sw4.Hapag.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Linq;

namespace softwrench.sw4.Hapag.Data {
    public static class InMemoryUserExtensions {

        public static string GetPersonGroupsForQuery(string module) {

            var user = SecurityFacade.CurrentUser();
            string[] personGroups;
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
