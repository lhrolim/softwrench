using System.Collections.Generic;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sw4.offlineserver.model.dto {
    public class UserSyncData {
        public bool Found { get; set; }
        public string UserName { get; set; }
        public string OrgId { get; set; }
        public string SiteId { get; set; }
        public string PersonId { get; set; }
        public string Email { get; set; }
        public long? UserTimezoneOffset { get; set; }
        public IDictionary<string, object> Properties { get; set; }

        /// <summary>
        /// Empty constructor for serializers/desserializer only
        /// </summary>
        public UserSyncData() : base() { }

        public UserSyncData(InMemoryUser user) {
            if (user == null) {
                Found = false;
            } else {
                Found = true;
                UserName = user.Login;
                OrgId = user.OrgId;
                SiteId = user.SiteId;
                PersonId = user.MaximoPersonId;
                Email = user.Email;
                UserTimezoneOffset = user.TimezoneOffset;
                Properties = user.GenericSyncProperties;
            }
        }
    }

}
