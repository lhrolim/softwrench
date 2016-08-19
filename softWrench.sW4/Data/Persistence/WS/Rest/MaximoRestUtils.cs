using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using softWrench.sW4.Data.Persistence.WS.Internal.Constants;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Connectors;
using softWrench.sW4.Util;
using CompressionUtil = cts.commons.Util.CompressionUtil;

namespace softWrench.sW4.Data.Persistence.WS.Rest {

    public class MaximoRestUtils {

        public const string BaseRestURLProp = "basewsRestURL";

        public const string NewUrlTemplate = "rest.{0}.url";
        public const string RestUserCredentials = "rest.{0}.credentials.user";
        public const string RestPasswordCredentials = "rest.{0}.credentials.password";

        [CanBeNull]
        public static string GenerateRestUrl(EntityMetadata entityMetadata, string entityId, string restKey = null) {

            if (!IsRestSetup(restKey)) {
                return null;
            }

            var baseRestURL = GetRestBaseUrl(restKey);

            var entityKey = entityMetadata.ConnectorParameters.GetWSEntityKey(ConnectorParameters.UpdateInterfaceParam, WsProvider.REST);
            if (baseRestURL.EndsWith("/mbo/")) {
                //the url can point either to a mbo or to a Object structure (OS)
                if (entityKey.StartsWith("SW")) {
                    entityKey = entityKey.Substring(2);
                }
            }

            if (!baseRestURL.EndsWith("/")) {
                //normalizing just in case
                baseRestURL += "/";
            }

            return baseRestURL + entityKey + "/" + entityId;
        }

        

        /// <summary>
        /// We always need to use mbo for querying multiple data otherwise it´s impossible to restrict the columns to bring.
        /// For a single record, though, it might be required to invoke the OS object in order to bring nested data
        /// </summary>
        /// <param name="entityMetadata"></param>
        /// <param name="entityId"></param>
        /// <param name="restKey"></param>
        /// <returns></returns>
        public static string GenerateRestUrlForQuery(EntityMetadata entityMetadata, string entityId , string restKey ) {
            if (entityId != null) {
                //if we´re bringing a specific record --> use OS integration
                return GenerateRestUrl(entityMetadata, entityId, restKey);
            }

            //just in case we have the property declared as a os (which is the default scenario)
            var baseRestURL = GetRestBaseUrl(restKey).Replace("/os/", "/mbo/");

            if (!baseRestURL.EndsWith("/")) {
                //normalizing just in case
                baseRestURL += "/";
            }

            return baseRestURL + entityMetadata.Name + "/";
        }


        private static string GenerateAuthHeader(string restKey) {
            var credentialsUser = RestCredentialsUser(restKey);
            var credentialsPassword = RestCredentialsPassword(restKey);
            var plainText = "{0}:{1}".Fmt(credentialsUser, credentialsPassword);
            return CompressionUtil.Base64Encode(plainText);
        }

        public static Dictionary<string, string> GetMaximoHeaders(string restKey = null) {

            var authToken = GenerateAuthHeader(restKey);
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"MAXAUTH", authToken},
                {"Content-Type", "application/x-www-form-urlencoded"},
                {"Authorization", "Basic " + authToken}
            };
            return headers;
        }

        public static bool IsRestSetup(string restKey = null) {
            return RestCredentialsUser(restKey) != null && RestCredentialsPassword(restKey) != null &&
                   GetRestBaseUrl(restKey) != null;
        }


        public static string RestCredentialsUser(string restKey = null) {
            if (restKey == null) {
                //legacy support
                return MetadataProvider.GlobalProperty("restcredentials.user");
            }
            return MetadataProvider.GlobalProperty(RestUserCredentials.Fmt(restKey));
        }

        public static string RestCredentialsPassword(string restKey = null) {
            if (restKey == null) {
                //legacy support
                return MetadataProvider.GlobalProperty("restcredentials.password");
            }
            return MetadataProvider.GlobalProperty(RestPasswordCredentials.Fmt(restKey));
        }

        private static string GetRestBaseUrl(string restKey) {
            return MetadataProvider.GlobalProperty(restKey == null ? BaseRestURLProp : NewUrlTemplate.Fmt(restKey));
        }

    }
}
