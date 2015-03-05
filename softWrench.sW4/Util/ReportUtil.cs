using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Util {
    public class ReportUtil {

        private const string reportDescriptor = "~/Reports/{0}.rdlc";
        private const string reportDataSetName = "{0}DataSet";
        
        public static string GetDescriptorPath(string reportName) {
            return String.Format(reportDescriptor, reportName);
        }

        public static string GetDataSetName(string reportName) {
            return String.Format(reportDataSetName, reportName);
        }

        public static DataTable ConvertMapListToTable(IList<IDictionary<string,object>> mapList, ApplicationSchemaDefinition schema) {
            var table = new DataTable();
            var user = SecurityFacade.CurrentUser();

            if (mapList != null && mapList.Count > 0) {

                table.Columns.AddRange(mapList[MaximumColumnIndex(mapList)].Select(f => new DataColumn(f.Key)).ToArray());
                
                foreach (var map in mapList) {
                    var dr = table.NewRow();

                    foreach (var key in map.Keys) {
                        if (map[key] is DateTime) {
                           // taking out formatting of date before sending to report as CDate in the report can't handle
                           // European date format easily. As Hapag wants European Date format in general, we can just do the
                           // formatting in the report itself
                           dr[key] = ((DateTime)map[key]).FromMaximoToUser(user).ToString();
                        } else {
                            dr[key] = map[key];
                        }                        
                    }

                    table.Rows.Add(dr);
                }
            }

            return table;
        }

        /// <summary>
        /// Returns the index of the object with maximum columns in the list
        /// </summary>
        /// <param name="mapList"></param>
        /// <returns></returns>
        private static int MaximumColumnIndex(IList<IDictionary<string, object>> mapList) {
            int maxCount = mapList[0].Count;
            int index = 0;
            for (int i = 0; i < mapList.Count; i++) {
                if (mapList[i].Count > maxCount) {
                    maxCount = mapList[i].Count;
                    index = i;
                }
            }
            return index;
        }

    }
}
