using System.IO;
using System.Text;
using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Data.Scheduler.Jobs.Helper {

    /// <summary>
    /// Fix precondition:
    /// ASSET.STATUS = 120 Active
    /// Extract all Assets where “Computer_name”* is starting with h,c,n,t,a,b,k (small and capital letters).
    /// 
    /// </summary>

    public class R104ExtractorHelper
    {

        public const string ComputerNameQuery ="CASE WHEN LOCATE('//',asset.Description) > 0 THEN LTRIM(RTRIM(SUBSTR(asset.Description,1,LOCATE('//',asset.Description)-1))) ELSE LTRIM(RTRIM(asset.Description)) END";

        internal static SearchRequestDto BuildDTO() {

            var dto = new SearchRequestDto {
                SearchSort = "computername",
                SearchAscending = true,
                QueryAlias = "rI104"
            };
            //h,c,n,t,a,b,k 
            char[] chars = { 'h', 'c', 'n', 't', 'a', 'b', 'k'};
            dto.AppendWhereClause("upper(STATUS) in ('{0}') ".Fmt(AssetConstants.Active.ToUpper()));
            var sb = new StringBuilder(" ( ");

            for (var index = 0;index < chars.Length;index++) {
                var c = chars[index];
                sb.AppendFormat(" {0} like ('{1}%') ",ComputerNameQuery, c);
                if (index != chars.Length - 1) {
                    sb.AppendFormat(" or ");
                }

            }
            sb.Append(")");

            dto.AppendWhereClause(sb.ToString());

            

            //            var configFilePath = MetadataProvider.GlobalProperty("RI104Path");
            //            if (!File.Exists(configFilePath)) {
            //                return dto;
            //            }
            //            var fileLines = File.ReadAllLines(configFilePath);
            //
            //            var sb = ClassStructureConfigFileReader.DoBuildQuery(fileLines);
            //            dto.AppendWhereClause(sb);
            return dto;

        }



    }
}
