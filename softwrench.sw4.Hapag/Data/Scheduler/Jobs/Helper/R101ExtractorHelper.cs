﻿using System.IO;
using System.Text;
using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Data.Scheduler.Jobs.Helper {
    public class R101ExtractorHelper {

        internal static SearchRequestDto BuildDTO() {
            var dto = new SearchRequestDto {
                SearchSort = "computername",
                SearchAscending = true,
                QueryAlias = "rI101"
            };

            dto.AppendWhereClause("upper(STATUS) in ('{0}','{1}')".Fmt(AssetConstants.Active.ToUpper(), AssetConstants.Accepted.ToUpper()));

            var configFilePath = MetadataProvider.GlobalProperty("RI101Path");
            if (!File.Exists(configFilePath)) {
                return dto;
            }
            var fileLines = File.ReadAllLines(configFilePath);

            var sb = ClassStructureConfigFileReader.DoBuildQuery(fileLines);
            dto.AppendWhereClause(sb);
            return dto;

        }

    

    }
}
