using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Iesi.Collections.Generic;
using softWrench.sW4.Metadata;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Data {

    public class AssetReportControlWhereClauseProvider : ISingletonComponent {


        public String GetCommodityOwnedByWhere() {
            var dict = GetDescriptionCommodityDictionary();
            var sb = new StringBuilder("case when alclistreport_.commodity is null then null ");
            foreach (var entry in dict) {
                sb.AppendFormat(" when alclistreport_.commodity in ('{0}') then '{1}'", String.Join(",", entry.Value),
                    entry.Key);
            }
            sb.Append(" else null end");
            return sb.ToString();

        }

        public String GetCommodityOwnedByJoin() {
            var dict = GetDescriptionCommodityDictionary();
            Set<string> allCommodities = new HashedSet<string>();
            foreach (var descCommodities in dict.Values) {
                foreach (var descCommodity in descCommodities) {
                    allCommodities.Add("'" + descCommodity + "'");
                }
            }
            var commodities = String.Join(",", allCommodities);
            return "alclistreport_.commodity in ({0})".Fmt(commodities);
        }


        /// <summary>
        /// retrieves a list of services that should be present in the path specified in the properties.xml, and uses them as commodities filters.
        /// 
        /// The format of the file is as follows:
        /// 
        /// HLC-0777	HLC-SW-SSO-SSO-ANALYZE	HLC-SWG
        /// HLC-0778	HLC-SW-SSO-SSO-CONFIG	HLC-SWG
        /// HLC-0779	HLC-SW-SSO-SSO-HANDLING	HLC-SWG
        /// HLC-0780	HLC-SW-SSO-SSO-OTHER	HLC-SWG
        /// HLC-0781	HLC-SW-SSO-SSO-OUTAGE	HLC-SWG
        /// 
        /// Just the first "column" is used
        /// </summary>
        /// <returns></returns>
        protected IDictionary<string, System.Collections.Generic.ISet<String>> GetDescriptionCommodityDictionary() {
            IDictionary<string, System.Collections.Generic.ISet<String>> dict = new Dictionary<string, System.Collections.Generic.ISet<string>>();

            var path = MetadataProvider.GlobalProperty("ownedbycommoditydescriptionsfile");
            if (String.IsNullOrEmpty(path)) {
                return GetDefaultDict();
            }
            if (!File.Exists(@path)) {
                return GetDefaultDict();
            }
            foreach (var row in File.ReadAllLines(@path)) {
                var serviceAux = row.Split(null);
                var description = serviceAux[1];
                if (!dict.ContainsKey(description)) {
                    dict[description] = new HashSet<string>();
                }
                dict[description].Add(serviceAux[0]);

            }

            return dict;
        }

        private IDictionary<string, System.Collections.Generic.ISet<string>> GetDefaultDict() {
            var dict = new Dictionary<string, System.Collections.Generic.ISet<string>>
            {
                {"HLAG", new HashSet<string>() {"HLC-0001"}},
                {"SITA", new HashSet<string>() {"HLC-0002"}}
            };
            return dict;
        }
    }
}
