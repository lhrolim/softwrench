using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using cts.commons.portable.Util;
using softwrench.sw4.api.classes.user;
using softwrench.sw4.dashboard.classes.service.graphic.exception;
using softWrench.sW4.Metadata;

namespace softwrench.sw4.dashboard.classes.service.graphic.tableau {
    public class TableauFacade : IGraphicStorageSystemFacade {

        public const string SYSTEM_NAME = "Tableau";

        public string SystemName() {
            return SYSTEM_NAME;
        }

        public IGraphicStorageSystemAuthDto Authenticate(ISWUser user) {
            var username = MetadataProvider.GlobalProperty("tableau_username", true);
            var server = MetadataProvider.GlobalProperty("tableau_server", true);
            var site = MetadataProvider.GlobalProperty("tableau_site");
            if ("default".EqualsIc(site)) site = null;
            var url = server + (server.EndsWith("/") ? "" : "/") + "trusted";

            try {
                using (var client = new WebClient()) {
                    var values = new NameValueCollection() {
                        { "username", username }
                    };

                    var response = client.UploadValues(url, values);
                    var responseString = Encoding.Default.GetString(response);

                    // -1 gets returned if user being used does not have enough permissions
                    if (string.IsNullOrWhiteSpace(responseString) || responseString == "-1") {
                        throw GraphicStorageSystemException.AuthenticationFailed(SYSTEM_NAME, url);
                    }
                    var ticket = responseString;
                    var auth = new TableauAuthDto() {
                        Ticket = ticket,
                        SiteName = site,
                        ServerUrl = server,
                    };
                    return auth;
                }
            } catch (Exception e) {
                if (e is GraphicStorageSystemException) throw;
                throw GraphicStorageSystemException.AuthenticationFailed(SYSTEM_NAME, url, e);
            }

        }
    }
}
