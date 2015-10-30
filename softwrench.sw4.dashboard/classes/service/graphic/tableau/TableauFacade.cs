using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using cts.commons.portable.Util;
using softwrench.sw4.api.classes.user;
using softwrench.sw4.dashboard.classes.service.graphic.exception;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sw4.dashboard.classes.service.graphic.tableau {
    /// <summary>
    /// Tableau's implementation of <see cref="IGraphicStorageSystemFacade"></see>
    /// </summary>
    public class TableauFacade : IGraphicStorageSystemFacade {

        public const string SYSTEM_NAME = "Tableau";

        private const string REST_API_CONTENT_TYPE = "application/xml;charset=utf-8";
        private const string REST_API_URL_PATTERN = "{0}api/2.0/{1}";
        private const string REST_API_DOCUMENT_NAMESPACE = "http://tableausoftware.com/api";
        private const string REST_API_AUTH_METHOD = "auth/signin";
        private const string REST_API_AUTH_PAYLOAD = "<tsRequest>" +
                                                        "<credentials name=\"{0}\" password=\"{1}\">" +
                                                        "<site contentUrl=\"{2}\"/>" +
                                                        "</credentials>" +
                                                        "</tsRequest>";
        /* CONFIGURATION */
        private readonly string _server; // tableau's server url
        private readonly string _site; // tableau's server's site url
        private readonly string _username; // tableau username to authenticate as
        private readonly string _password; // tableau user's password to authenticate as
        /// <summary>
        /// <para>
        /// Flag indicating if should fail if couldn't obtain trusted ticket.
        /// Default's to true.
        /// </para>
        /// <para>
        /// It's only false in Local and Development environments 
        /// (usually the developers's machines are not all marked as trusted by Tableau server)
        /// so the developer can manually authenticate to Tableau. 
        /// </para>
        /// </summary>
        private readonly bool _requireTicket;

        public TableauFacade() {
            var site = MetadataProvider.GlobalProperty("tableau_site");
            _site = site == null || site == "default" ? "" : site;
            var server = MetadataProvider.GlobalProperty("tableau_server", true);
            _server = server + (server.EndsWith("/") ? "" : "/");
            _username = MetadataProvider.GlobalProperty("tableau_username", true);
            _password = MetadataProvider.GlobalProperty("tableau_password", true);
            _requireTicket = !ApplicationConfiguration.IsLocal() || !ApplicationConfiguration.IsDev();
        }

        public string SystemName() {
            return SYSTEM_NAME;
        }

        private void AuthToRestApi(TableauAuthDto auth) {
            // url and payload
            var url = string.Format(REST_API_URL_PATTERN, _server, REST_API_AUTH_METHOD);
            var payload = string.Format(REST_API_AUTH_PAYLOAD, _username, _password, _site);
            var body = Encoding.UTF8.GetBytes(payload);
            // configure httprequest
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = REST_API_CONTENT_TYPE;
            request.ContentLength = body.Length;
            request.Method = "POST";
            // write payload to requests stream
            using (var requestStream = request.GetRequestStream()) {
                requestStream.Write(body, 0, body.Length);
            }
            // fetch response
            var response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK) {
                throw GraphicStorageSystemException.AuthenticationFailed(SYSTEM_NAME, _server);
            }
            // read response's payload
            using (var responseStream = response.GetResponseStream()) {
                using (var responseReader = new StreamReader(responseStream)) {
                    // parse xml response
                    var text = responseReader.ReadToEnd();
                    var xml = XDocument.Parse(text);
                    XNamespace xmlns = REST_API_DOCUMENT_NAMESPACE; // implicit convertion doesn't work with constants
                    var credentials = xml.Root.Descendants(xmlns + "credentials").First();
                    var token = credentials.Attribute("token").Value;
                    var userid = credentials.Descendants(xmlns + "user").First().Attribute("id").Value;
                    var siteid = credentials.Descendants(xmlns + "site").First().Attribute("id").Value;
                    // fill rest api auth data
                    auth.Token = token;
                    auth.UserId = userid;
                    auth.SiteId = siteid;
                }
            }
        }

        private void FetchTrustedTicket(TableauAuthDto auth) {
            using (var client = new WebClient()) {
                var values = new NameValueCollection() {
                    { "username", _username }
                };
                var response = client.UploadValues(_server + "trusted", values);
                var ticket = Encoding.Default.GetString(response);
                // -1 gets returned if user being used does not have enough permissions
                // or the domain in which SW is deployed is not trusted by the Tableau server
                if ((string.IsNullOrWhiteSpace(ticket) || ticket == "-1")) {
                    if (_requireTicket) {
                        throw GraphicStorageSystemException.AuthenticationFailed(SYSTEM_NAME, _server);
                    }
                    // fail silently: require manual authentication
                    ticket = null;
                }
                auth.Ticket = ticket;
            }
        }

        public IGraphicStorageSystemAuthDto Authenticate(ISWUser user) {
            var auth = new TableauAuthDto() {
                SiteName = _site,
                ServerUrl = _server
            };
            var tasks = new Task[2];
            try {
                // fill authdto with rest api authentication data
                tasks[0] = Task.Factory.StartNew(() => AuthToRestApi(auth));
                // fill authdto with trustedticket for the js api
                tasks[1] = Task.Factory.StartNew(() => FetchTrustedTicket(auth));
                Task.WaitAll(tasks);
                // return fully filled authdto
                return auth;
            } catch (Exception e) {
                if (e is GraphicStorageSystemException) throw;
                throw GraphicStorageSystemException.AuthenticationFailed(SYSTEM_NAME, _server, e);
            }

        }
    }
}
