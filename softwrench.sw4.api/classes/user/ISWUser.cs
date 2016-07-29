using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.api.classes.user {
    public interface ISWUser :IPrincipal
    {
        bool IsInProfile(string profileName);

        IEnumerable<int?> ProfileIds { get; }

        int? UserId { get; }

        IDictionary<string, object> Genericproperties { get; set; }

        bool IsSwAdmin();

        bool IsAllowedInApp(string applicationName);
    }
}
