using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Mapping.Attributes;

namespace softwrench.sw4.dashboard.classes.model.entities {

    [Component]
    public class DashboardFilter {


        public static string GetUserProfileString(IEnumerable<int?> profiles) {
            if (!profiles.Any()) {
                return "1=2";
            }

            var sb = new StringBuilder("");
            foreach (var profile in profiles) {
                sb.Append("userprofiles like %");
                sb.Append(";").Append(profile).Append(";");
                sb.Append("%");
                sb.Append(" or ");
            }

            var parameter = sb.ToString(0, (sb.Length - " or ".Length));
            return parameter;
        }

        [Property]
        public virtual int? UserId {
            get; set;
        }

        [Property]
        public virtual string UserProfiles {
            get; set;
        }


    }
}
