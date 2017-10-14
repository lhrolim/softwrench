using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using Quartz.Util;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.Search {

    public class SortHandler : ISingletonComponent {

        public void HandleSearchDTO(ApplicationSchemaDefinition schema, SearchRequestDto dto) {
            if (string.IsNullOrWhiteSpace(dto.SearchSort) &&
                (dto.MultiSearchSort == null || dto.MultiSearchSort.Count == 0 ||
                 dto.MultiSearchSort.All(a => a == null))) {
                return;
            }

            if (!string.IsNullOrWhiteSpace(dto.SearchSort) && dto.SearchSort.StartsWith("custom:")) {
                dto.SearchSort = dto.SearchSort.Substring(7);
                return;
            }

            var translatedFields = schema.Fields.Where(f => f.AttributeToServer != null);
            if (!translatedFields.Any()) {
                var searchField = schema.Fields.FirstOrDefault(f => f.Attribute.Equals(dto.SearchSort));
                if (searchField == null || searchField.IsTransient()) {
                    dto.SearchSort = null;
                    return;
                }
                return;
            }

            if (dto.MultiSearchSort != null) {
                dto.TranslatedMultiSearchSort = new List<SortOrder>();
                foreach (var searchSort in dto.MultiSearchSort) {
                    dto.TranslatedMultiSearchSort.Add(searchSort);
                }
            }

            foreach (var field in translatedFields) {
                if (dto.SearchSort != null) {
                    dto.TranslatedSearchSort = dto.SearchSort.Replace(field.Attribute, field.AttributeToServer);
                }

                if (dto.TranslatedMultiSearchSort != null) {
                    var multiSort = dto.TranslatedMultiSearchSort.FirstOrDefault(f => f.ColumnName.Equals(field.Attribute));
                    if (multiSort != null) {
                        dto.TranslatedMultiSearchSort.Remove(multiSort);
                        dto.TranslatedMultiSearchSort.Add(new SortOrder { ColumnName = field.AttributeToServer, IsAscending = multiSort.IsAscending });
                    }
                }
            }
        }

    }

}

