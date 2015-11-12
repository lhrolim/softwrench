using System.IO;
using System.Text;
using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Data.Scheduler.Jobs.Helper {
    public class ClassStructureConfigFileReader {

        private const string LeafChildrenBaseQuery = "asset.CLASSSTRUCTUREID in ({0})";

        private const string ParentNodeBaseQuery = "asset.CLASSSTRUCTUREID in (select c2.CLASSSTRUCTUREID from maximo.CLASSSTRUCTURE c1 inner join maximo.CLASSSTRUCTURE c2 on c2.PARENT = c1.CLASSSTRUCTUREID and c1.CLASSSTRUCTUREID in ({0}))";



        //To allow unit testing
        public static string DoBuildQuery(string[] fileLines) {
            var sb = new StringBuilder();
            var leafNodes = new StringBuilder();
            var parentNodes = new StringBuilder();
            foreach (var row in fileLines) {
                if (!row.Contains("\\")) {
                    leafNodes.Append("'").Append(row).Append("'").Append(",");
                } else {
                    parentNodes.Append("'").Append(row.Replace("\\","")).Append("'").Append(",");
                }
            }

            var hasBoth = leafNodes.Length > 0 && parentNodes.Length > 0;

            if (hasBoth) {
                sb.Append("(");
            }

            if (leafNodes.Length > 0) {
                var comalessClassifications = leafNodes.ToString(0, leafNodes.Length - 1);
                sb.Append(LeafChildrenBaseQuery.Fmt(comalessClassifications));
            }

            if (hasBoth) {
                sb.Append(" or ");
            }

            if (parentNodes.Length > 0) {
                var comalessClassifications = parentNodes.ToString(0, parentNodes.Length - 1);
                sb.Append(ParentNodeBaseQuery.Fmt(comalessClassifications));
            }

            if (hasBoth) {
                sb.Append(")");
            }

            return sb.ToString();
        }

    }
}
