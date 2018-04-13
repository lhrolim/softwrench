using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence;

namespace softwrench.sw4.Hapag.Data.DataSet.Helper {
    public class R0042QueryHelper {




        public static void MergeData(IDictionary<string, DataMap> map, IList<dynamic> historicData) {
            foreach (var hist in historicData) {
                DataMap dm;
                map.TryGetValue(hist.assetid, out dm);
                if (dm == null) {
                    continue;
                }
                dm.SetAttribute("#old_puser_displayname", hist.itcname);
                dm.SetAttribute("#old_aucisow_displayname", hist.userid);
                dm.SetAttribute("#old_loc_description", hist.locdescription);
                dm.SetAttribute("#old_department", hist.department);
                dm.SetAttribute("#old_location_room", hist.room);
                dm.SetAttribute("#old_serialnum", hist.serialnum);
                dm.SetAttribute("#old_assetspeceosdate_alnvalue", hist.eosdate);
                dm.SetAttribute("#old_usage", hist.usage);
                dm.SetAttribute("#old_status", hist.status);
                dm.SetAttribute("#old_assetspecmacaddress_alnvalue", hist.macaddress);
            }
        }


        public static IDictionary<string, DataMap> BuildIdMap(ApplicationListResult dbList) {
            IDictionary<string, DataMap> map = new Dictionary<string, DataMap>();

            foreach (var attributeHolder in dbList.ResultObject) {
                map.Add(attributeHolder.GetAttribute("assetid").ToString(),(DataMap) attributeHolder);
            }

            return map;
        }
    }
}
