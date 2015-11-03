using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using softwrench.sw4.api.classes.user;
using softwrench.sw4.dashboard.classes.service.graphic.exception;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;
using WebGrease.Css.Extensions;

namespace softwrench.sw4.dashboard.classes.service.graphic.tableau {
    /// <summary>
    /// Tableau's implementation of <see cref="IGraphicStorageSystemFacade"></see>
    /// </summary>
    public class TableauFacade : IGraphicStorageSystemFacade {

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
            var site = MetadataProvider.GlobalProperty(TableauConstants.Config.KEY_SITE);
            // if 'tableau_site' is configured use it
            // otherwise use the clientname as the site name
            if (site != null) {
                _site = site == "" || site == "default" ? "" : site;
            } else {
                _site = ApplicationConfiguration.ClientName;
            }
            // 'sanitize' to add last '/'
            var server = MetadataProvider.GlobalProperty(TableauConstants.Config.KEY_SERVER);
            if (server == null) {
                return;
            }

            _server = server + (server.EndsWith("/") ? "" : "/");

            _username = MetadataProvider.GlobalProperty(TableauConstants.Config.KEY_USERNAME, true);
            _password = MetadataProvider.GlobalProperty(TableauConstants.Config.KEY_PASSWORD, true);
            _requireTicket = !ApplicationConfiguration.IsLocal() || !ApplicationConfiguration.IsDev();
        }

        public string SystemName() {
            return TableauConstants.SYSTEM_NAME;
        }

        private HttpWebResponse CallRestApi(string resourceUrl, string method, Dictionary<string, string> headers = null, string payload = null) {
            var url = string.Format(TableauConstants.RestApi.URL_PATTERN, _server, resourceUrl);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            // headers
            if (headers != null) {
                headers.ForEach((e) => request.Headers[e.Key] = e.Value);
            }
            request.ContentType = TableauConstants.RestApi.CONTENT_TYPE;
            // write payload to requests stream
            if (!string.IsNullOrEmpty(payload)) {
                var body = Encoding.UTF8.GetBytes(payload);
                request.ContentLength = body.Length;
                using (var requestStream = request.GetRequestStream()) {
                    requestStream.Write(body, 0, body.Length);
                }
            }
            // fetch response
            var response = (HttpWebResponse)request.GetResponse();
            return response;
        }

        private void AuthToRestApi(TableauAuthDto auth) {
            var payload = string.Format(TableauConstants.RestApi.AUTH_PAYLOAD, _username, _password, _site);
            var response = CallRestApi(TableauConstants.RestApi.AUTH_METHOD, "POST", null, payload);
            if (response.StatusCode != HttpStatusCode.OK) {
                throw GraphicStorageSystemException.AuthenticationFailed(TableauConstants.SYSTEM_NAME, _server);
            }
            // read response's payload
            using (var responseStream = response.GetResponseStream()) {
                using (var responseReader = new StreamReader(responseStream)) {
                    // parse xml response
                    var text = responseReader.ReadToEnd();
                    var xml = XDocument.Parse(text);
                    XNamespace xmlns = TableauConstants.RestApi.DOCUMENT_NAMESPACE; // implicit convertion doesn't work with constants
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
                if (ticket == "-1" /* permission check */ || string.IsNullOrWhiteSpace(ticket) /* safety check (should never happen) */ ) {
                    if (_requireTicket) {
                        throw DomainNotTrustedByTableauException.InvalidTicket(ticket);
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
                throw GraphicStorageSystemException.AuthenticationFailed(TableauConstants.SYSTEM_NAME, _server, e);
            }
        }

        public string LoadExternalResource(string resource, JObject request) {
            var auth = (TableauAuthDto)request.GetValue("auth").ToObject(typeof(TableauAuthDto));
            string url;
            if (resource == "workbook") {
                url = string.Format("sites/{0}/users/{1}/workbooks", auth.SiteId, auth.UserId);
            } else if (resource == "view") {
                var workbook = (string)request.GetValue("workbook").ToObject(typeof(string));
                url = string.Format("sites/{0}/workbooks/{1}/views", auth.SiteId, workbook);
            } else {
                throw GraphicStorageSystemException.ExternalResourceLoadFailed(TableauConstants.SYSTEM_NAME, resource);
            }
            var headers = new Dictionary<string, string>() { { TableauConstants.RestApi.AUTH_HEADER_NAME, auth.Token } };
            var response = CallRestApi(url, "GET", headers);
            if (response.StatusCode != HttpStatusCode.OK) {
                throw GraphicStorageSystemException.ExternalResourceLoadFailed(TableauConstants.SYSTEM_NAME, resource);
            }
            using (var responseStream = response.GetResponseStream()) {
                using (var responseReader = new StreamReader(responseStream)) {
                    //TODO: smart caching
                    return responseReader.ReadToEnd();
                }
            }
        }
    }
}
