using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softwrench.sw4.Hapag.Data.Sync;
using softwrench.sw4.Hapag.Security;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;

namespace softwrench.sw4.Hapag.Data {


    public class AssetControlWhereClauseProvider : ISingletonComponent {


        public AssetControlWhereClauseProvider(R0017WhereClauseProvider rooR0017WhereClauseProvider, IHlagLocationManager locationManager) {
        }

        //HAP-838 item 6
        public string AssetWhereClause() {
            return "asset.status !='{0}'".Fmt(AssetConstants.Decommissioned);
        }

    }
}
