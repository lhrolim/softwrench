using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using softwrench.sw4.offlineserver.model.dto.association;
using softwrench.sW4.Shared2.Metadata;
using softWrench.sW4.Data;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Util;

namespace softwrench.sw4.offlineserver.services.util {
    public class IndexUtil {
        public class IndexRepresentation {
            public IList<string> TextIndexes { get; set; } = new List<string>();
            public IList<string> NumericIndexes { get; set; } = new List<string>();
            public IList<string> DateIndexes { get; set; } = new List<string>();
        }

        public static void ParseIndexes(IList<string> textIndexes, IList<string> numericIndexes, IList<string> dateIndexes, CompleteApplicationMetadataDefinition topLevelApp) {
            var indexesString = topLevelApp.GetProperty(ApplicationSchemaPropertiesCatalog.ListOfflineTextIndexes);
            if (!string.IsNullOrEmpty(indexesString)) {
                indexesString.Split(',').ToList().ForEach(idx => ParseIndex(idx, textIndexes));
            }

            indexesString = topLevelApp.GetProperty(ApplicationSchemaPropertiesCatalog.ListOfflineNumericIndexes);
            if (!string.IsNullOrEmpty(indexesString)) {
                indexesString.Split(',').ToList().ForEach(idx => ParseIndex(idx, numericIndexes));
            }

            indexesString = topLevelApp.GetProperty(ApplicationSchemaPropertiesCatalog.ListOfflineDateIndexes);
            if (!string.IsNullOrEmpty(indexesString)) {
                indexesString.Split(',').ToList().ForEach(idx => ParseIndex(idx, dateIndexes));
            }
        }

        public static IndexRepresentation BuildRepresentation(ApplicationMetadata topLevelApp) {
            var indexesString = topLevelApp.GetProperty(ApplicationSchemaPropertiesCatalog.ListOfflineTextIndexes);
            var repr = new IndexRepresentation();
            if (!string.IsNullOrEmpty(indexesString)) {
                indexesString.Split(',').ToList().ForEach(idx => ParseIndex(idx, repr.TextIndexes));
            }

            indexesString = topLevelApp.GetProperty(ApplicationSchemaPropertiesCatalog.ListOfflineNumericIndexes);
            if (!string.IsNullOrEmpty(indexesString)) {
                indexesString.Split(',').ToList().ForEach(idx => ParseIndex(idx, repr.NumericIndexes));
            }

            indexesString = topLevelApp.GetProperty(ApplicationSchemaPropertiesCatalog.ListOfflineDateIndexes);
            if (!string.IsNullOrEmpty(indexesString)) {
                indexesString.Split(',').ToList().ForEach(idx => ParseIndex(idx, repr.DateIndexes));
            }
            return repr;
        }

        private static void ParseIndex(string index, IList<string> indexList) {
            var trimmed = index.Trim();
            if (string.IsNullOrEmpty(trimmed)) {
                return;
            }
            indexList.Add(trimmed);
        }

        public static void HandleIndexes(IEnumerable<CompleteApplicationMetadataDefinition> associations, AssociationSynchronizationResultDto results) {
            associations?.ToList().ForEach(association => {
                var textIndexes = new List<string>();
                results.TextIndexes.Add(association.ApplicationName, textIndexes);

                var numericIndexes = new List<string>();
                results.NumericIndexes.Add(association.ApplicationName, numericIndexes);

                var dateIndexes = new List<string>();
                results.DateIndexes.Add(association.ApplicationName, dateIndexes);

                ParseIndexes(textIndexes, numericIndexes, dateIndexes, association);
            });
        }

        public static IDictionary<string, object> PopulateIndexes(ApplicationMetadata appMetadata, DataMap datamap) {
            var repr = BuildRepresentation(appMetadata);
            var result = new Dictionary<string, object>();
            var i = 1;
            foreach (var idx in repr.TextIndexes) {
                result.Add("t" + i++, datamap.GetStringAttribute(idx));
            }

            i = 1;
            foreach (var idx in repr.NumericIndexes) {
                result.Add("n" + i++, datamap.GetStringAttribute(idx));
            }
            i = 1;
            foreach (var idx in repr.DateIndexes) {
                var date = datamap.GetAttribute(idx);
                if (date == null) {
                    continue;
                }
                var dt = DateTime.Now;
                if (date is DateTime) {
                    dt = (DateTime)date;
                } else if (date is DateTimeOffset) {
                    var d = (DateTimeOffset)date;
                    dt = d.DateTime;
                } else if (date is string) {
                    var d = DateTime.Parse(date as string);
                    dt = d;
                }


                result.Add("d" + i++, dt.ToTimeInMillis());

            }
            return result;
        }
    }
}
