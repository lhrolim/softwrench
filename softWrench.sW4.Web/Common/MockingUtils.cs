using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Controllers;


namespace softWrench.sW4.Web.Common {
    public class MockingUtils {


        private const string MockingMaximoKey = "%%mockmaximo";
        private const string MockingErrorKey = "%%mockerror";

        public static bool IsMockingMaximoModeActive(JObject json) {
            if (ApplicationConfiguration.IsProd()) {
                return false;
            }

            var mockmaximo = json.Property(MockingMaximoKey);
            if (mockmaximo == null) {
                return false;
            }
            json.Remove(MockingMaximoKey);
            return true;
        }

        public static void EvalMockingErrorModeActive(JObject json, HttpRequestMessage request) {
            if (ApplicationConfiguration.IsProd()) {
                return;
            }

            var mockerror = json.Property(MockingErrorKey);
            if (mockerror == null || mockerror.Value.ToString().EqualsIc("false")) {
                return;
            }
            try {
                object a = new { };
                var b = (string)a;
            } catch (Exception e) {
                var errorResponse = request.CreateResponse(HttpStatusCode.InternalServerError, new ErrorDto(e.Message, e.StackTrace, e.StackTrace));
                throw new HttpResponseException(errorResponse);
            }
        }

        public static ApplicationDetailResult GetMockedDataMap(string applicationName, ApplicationSchemaDefinition schema, ApplicationMetadata metadata) {
            var dictionary = new Dictionary<string, object>();

            var applicationDisplayables = schema.Displayables;
            int i = 1;
            foreach (var applicationDisplayable in applicationDisplayables) {
                var field = applicationDisplayable as BaseApplicationFieldDefinition;
                if (field != null && !dictionary.ContainsKey(field.Attribute)) {
                    dictionary.Add(field.Attribute, "mocked value " + i);
                }
                i++;
            }
            var dataMap = new DataMap(applicationName, dictionary);
            return new ApplicationDetailResult(dataMap, null, schema,
                        CompositionBuilder.InitializeCompositionSchemas(metadata.Schema), "id");
        }
    }
}