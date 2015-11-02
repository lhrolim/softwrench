using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softWrench.sW4.Scheduler.Jobs.helper {
    internal class R101ExtractorHelper {
        private const string LeafChildrenBaseQuery = "asset.CLASSSTRUCTUREID in ({0})";


        internal static SearchRequestDto BuildDTO() {
            var dto = new SearchRequestDto {
                SearchSort = "computername",
                SearchAscending = true
            };
            var configFilePath = MetadataProvider.GlobalProperty("RI101Path");
            if (!File.Exists(configFilePath)) {
                return dto;
            }
            var fileLines = File.ReadAllLines(configFilePath);

            var sb = DoBuildQuery(fileLines);
            dto.AppendWhereClause(sb);
            return dto;

        }

        //To allow unit testing
        internal static string DoBuildQuery(string[] fileLines) {
            var sb = new StringBuilder();
            var leafChildren = new StringBuilder();
            foreach (var row in fileLines) {
                if (!row.Contains("\\")) {
                    leafChildren.Append("'").Append(row).Append("'").Append(",");
                }
            }
            if (leafChildren.Length > 0) {
                var comalessClassifications = leafChildren.ToString(0, leafChildren.Length - 1);
                sb.Append(LeafChildrenBaseQuery.Fmt(comalessClassifications));
            }

            return sb.ToString();
        }

    }
}
