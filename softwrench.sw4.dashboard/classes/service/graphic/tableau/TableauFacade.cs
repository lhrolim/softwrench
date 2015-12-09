using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using log4net;
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

        private static readonly ILog Log = LogManager.GetLogger(typeof (TableauFacade));

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

            Log.DebugFormat("Instantiating TableauFacade: server:{0}, site:{1}, username:{2}", _server, _site, _username);
        }

        public string SystemName { get { return TableauConstants.SYSTEM_NAME; } }

        private async Task<HttpWebResponse> CallRestApi(string resourceUrl, string method, Dictionary<string, string> headers = null, string payload = null) {
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
            var response = await request.GetResponseAsync();
            return (HttpWebResponse) response;
        }

        private async Task<TableauAuthDto> AuthToRestApi() {
            var payload = string.Format(TableauConstants.RestApi.AUTH_PAYLOAD, _username, _password, _site);
            var response = await CallRestApi(TableauConstants.RestApi.AUTH_METHOD, "POST", null, payload);
            if (response.StatusCode != HttpStatusCode.OK) {
                throw GraphicStorageSystemException.AuthenticationFailed(TableauConstants.SYSTEM_NAME, _server);
            }
            // read response's payload
            using (var responseStream = response.GetResponseStream()) {
                using (var responseReader = new StreamReader(responseStream)) {
                    // parse xml response
                    var text = await responseReader.ReadToEndAsync();
                    var xml = XDocument.Parse(text);
                    XNamespace xmlns = TableauConstants.RestApi.DOCUMENT_NAMESPACE; // implicit convertion doesn't work with constants
                    var credentials = xml.Root.Descendants(xmlns + "credentials").First();
                    var token = credentials.Attribute("token").Value;
                    var userid = credentials.Descendants(xmlns + "user").First().Attribute("id").Value;
                    var siteid = credentials.Descendants(xmlns + "site").First().Attribute("id").Value;
                    // fill rest api auth data
                    return new TableauAuthDto() {
                        SiteName = _site,
                        ServerUrl = _server,
                        Token = token,
                        UserId = userid,
                        SiteId = siteid
                    };
                }
            }
        }

        private async Task<TableauAuthDto> FetchTrustedTicket() {
            using (var client = new HttpClient()) {
                var content = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("username", _username)
                });
                var response = await client.PostAsync(_server + "trusted", content);
                var ticket = await response.Content.ReadAsStringAsync();
                // -1 gets returned if user being used does not have enough permissions
                // or the domain in which SW is deployed is not trusted by the Tableau server
                if (ticket == "-1" /* permission check */ || string.IsNullOrWhiteSpace(ticket) /* safety check (should never happen) */ ) {

                    if (_requireTicket) throw DomainNotTrustedByTableauException.InvalidTicket(ticket);
                    
                    // fail silently: require manual authentication
                    ticket = null;
                }
                return new TableauAuthDto() {
                    SiteName = _site,
                    ServerUrl = _server,
                    Ticket = ticket
                };
            }
        }

        /// <summary>
        /// Supports two forms of authentication to tableau server. The auth mechanism should come as requestConfig["authtype"]:
        /// - "REST": authenticates to server's rest api returning a session token, userid and siteid
        /// - "TICKET": fetches trusted ticket
        /// </summary>
        /// <param name="user"></param>
        /// <param name="requestConfig"></param>
        /// <returns></returns>
        public async Task<IGraphicStorageSystemAuthDto> Authenticate(ISWUser user, IDictionary<string, string> requestConfig) {
            // lookup auth type
            var authType = requestConfig[TableauConstants.AuthType.KEY];
            try {
                switch (authType) {

                    case TableauConstants.AuthType.REST_API:
                        return await AuthToRestApi();

                    case TableauConstants.AuthType.TRUSTED_TICKET:
                        return await FetchTrustedTicket();

                    default:
                        throw GraphicStorageSystemException.AuthenticationFailed(TableauConstants.SYSTEM_NAME, _server);
                }
            } catch (Exception e) {
                if (e is GraphicStorageSystemException) throw;
                throw GraphicStorageSystemException.AuthenticationFailed(TableauConstants.SYSTEM_NAME, _server, e);
            }
        }

        /// <summary>
        /// Makes a request to tableau server's REST api to fetch the resource from the server.
        /// Supported resources:
        /// - "workbook": fetches a xml list of the workbooks visible by the user in the site
        /// - "view":  fetches a xml list of the views pertaining to a workbook; requires a workbook id under requestConfig["workbook"]
        /// Both resources require the REST api authentication information to be passed in the requestConfig dictionary.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="requestConfig"></param>
        /// <returns></returns>
        public async Task<string> LoadExternalResource(string resource, IDictionary<string, string> requestConfig) {
            string url;

            switch (resource) {
                case TableauConstants.RestApi.RESOURCE_WORKBOOK:
                    url = string.Format("sites/{0}/users/{1}/workbooks", requestConfig["siteId"], requestConfig["userId"]);
                    break;
                case TableauConstants.RestApi.RESOURCE_VIEW:
                    var workbook = requestConfig["workbook"];
                    url = string.Format("sites/{0}/workbooks/{1}/views", requestConfig["siteId"], workbook);
                    break;
                default:
                    throw GraphicStorageSystemException.ExternalResourceLoadFailed(TableauConstants.SYSTEM_NAME, resource);
            }

            var headers = new Dictionary<string, string>() { { TableauConstants.RestApi.AUTH_HEADER_NAME, requestConfig["token"] } };
            var response = await CallRestApi(url, "GET", headers);

            if (response.StatusCode != HttpStatusCode.OK) {
                throw GraphicStorageSystemException.ExternalResourceLoadFailed(TableauConstants.SYSTEM_NAME, resource);
            }

            using (var responseStream = response.GetResponseStream()) {
                using (var responseReader = new StreamReader(responseStream)) {
                    //TODO: smart caching
                    return await responseReader.ReadToEndAsync();
                }
            }
        }
    }
}
