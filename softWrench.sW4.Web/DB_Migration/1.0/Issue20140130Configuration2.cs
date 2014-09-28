using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Web;
using FluentMigrator;
using Quartz.Impl.AdoJobStore.Common;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.DB_Migration._1._0 {

    [Migration(201401301640)]
    public class Issue20140130Configuration2 : FluentMigrator.Migration {
        public override void Up() {
            var providerName = System.Configuration.ConfigurationManager.ConnectionStrings["SWDB"].ProviderName;
            if (string.IsNullOrEmpty(providerName)) return;
            switch (providerName) {
                case "MySql.Data.MySqlClient":
                    Alter.Column("blobvalue").OnTable("conf_propertyvalue").AsBinary().Nullable();
                    Alter.Column("defaultblobvalue").OnTable("conf_propertydefinition").AsBinary().Nullable();
                    break;
                case "System.Data.SQL":
                    Alter.Column("blobvalue").OnTable("conf_propertyvalue").AsBinary(int.MaxValue).Nullable();
                    Alter.Column("defaultblobvalue").OnTable("conf_propertydefinition").AsBinary(int.MaxValue).Nullable();
                    break;
            }
        }

        public override void Down() {

        }
    }
}