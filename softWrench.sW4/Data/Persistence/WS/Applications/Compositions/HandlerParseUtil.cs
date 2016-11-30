using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NHibernate.Linq;
using NHibernate.Util;
using softWrench.sW4.Data.Persistence.Operation;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Compositions {
    class HandlerParseUtil {
        /// <summary>
        /// Parses the list o labor or tools for the LabtransHandler and ToolsHandler.
        /// </summary>
        /// <param name="crudOperationDataArray"></param>
        /// <param name="parsedOperationData">The parse result</param>
        /// <param name="compositionSchemaId"></param>
        /// <param name="setValues"></param>
        public static void ParseUnmappedCompositionInline(CrudOperationData[] crudOperationDataArray,
            List<CrudOperationData> parsedOperationData, string compositionSchemaId, Action<CrudOperationData, JObject> setValues) {
            crudOperationDataArray.ForEach(co => Parse(co, parsedOperationData, compositionSchemaId, setValues));
        }

        private static void Parse(CrudOperationData crudOperationData,
            List<CrudOperationData> parsedOperationData, string compositionSchemaId, Action<CrudOperationData, JObject> setValues) {
            if (crudOperationData.UnmappedAttributes == null || crudOperationData.UnmappedAttributes.Count == 0) {
                parsedOperationData.Add(crudOperationData);
                return;
            }

            var compositionListString = "";
            try {
                var compositionListPair = crudOperationData.UnmappedAttributes.First(pair => compositionSchemaId.Equals(pair.Key));
                compositionListString = compositionListPair.Value;
            } catch (Exception) {
                // just ignores
            }

            if (string.IsNullOrEmpty(compositionListString)) {
                parsedOperationData.Add(crudOperationData);
                return;
            }
            compositionListString = "[" + compositionListString + "]";
            var jsonArray = JArray.Parse(compositionListString);
            jsonArray.ForEach(token => Parse(token, crudOperationData, parsedOperationData, setValues));
        }

        private static void Parse(JToken token, CrudOperationData crudOperationData, List<CrudOperationData> parsedOperationData, Action<CrudOperationData, JObject> setValues) {
            var jsonObject = token as JObject;
            if (jsonObject == null) {
                return;
            }

            var newcrudOperationData = crudOperationData.Clone();
            setValues(newcrudOperationData, jsonObject);
            parsedOperationData.Add(newcrudOperationData);
        }
    }
}
