using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.Http;
using System.Web.Mvc;
using cts.commons.persistence;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Controllers.Help {

    public class HelpController : ApiController {
        //
        // GET: /Faq/

        [System.Web.Http.HttpGet]
        [SPFRedirect("Help", "help.title", "Help")]
        public GenericResponseResult<IList<KeyValuePair<string, string>>> Index() {

            IList<KeyValuePair<string, string>> aboutData = new List<KeyValuePair<string, string>>();

            var maximoDB = ApplicationConfiguration.DBConnectionStringBuilder(DBType.Maximo);
            var swDB = ApplicationConfiguration.DBConnectionStringBuilder(DBType.Swdb);

            aboutData.Add(new KeyValuePair<string, string>(_resolver.I18NValue("about.version", "Version"), ApplicationConfiguration.SystemVersion));
            aboutData.Add(new KeyValuePair<string, string>(_resolver.I18NValue("about.revision", "Revision"), ApplicationConfiguration.SystemRevision));
            aboutData.Add(new KeyValuePair<string, string>(_resolver.I18NValue("about.builddate", "Build Date"), ApplicationConfiguration.SystemBuildDate.ToString(CultureInfo.InvariantCulture.DateTimeFormat)));
            aboutData.Add(new KeyValuePair<string, string>(_resolver.I18NValue("about.clientname", "Client Name"), ApplicationConfiguration.ClientName));
            aboutData.Add(new KeyValuePair<string, string>(_resolver.I18NValue("about.profile", "Profile"), ApplicationConfiguration.Profile));
            aboutData.Add(new KeyValuePair<string, string>(_resolver.I18NValue("about.maximourl", "Maximo URL"), _configService.Lookup<string>(ConfigurationConstants.Maximo.WsdlPath, "baseWSPrefix")));
            aboutData.Add(new KeyValuePair<string, string>(_resolver.I18NValue("about.maximodb", "Maximo DB"),
                $"{maximoDB.DataSource}/{maximoDB.Catalog}"));
            aboutData.Add(new KeyValuePair<string, string>(_resolver.I18NValue("maximodb.version", "SW DB"),
                $"{swDB.DataSource}/{swDB.Catalog}"));

            return new GenericResponseResult<IList<KeyValuePair<string, string>>>(aboutData);
        }
    }
}
