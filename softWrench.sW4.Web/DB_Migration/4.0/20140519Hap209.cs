using System.Data;
using cts.commons.persistence.Util;
using FluentMigrator;
using MySql.Data.MySqlClient;
using softwrench.sw4.Hapag.Data;
using softwrench.sw4.Hapag.Data.Init;
using softWrench.sW4.Web.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace softWrench.sW4.Web.DB_Migration._4._0 {

    [Migration(201405191400)]
    public class MigrationHap209 : FluentMigrator.Migration {

        public override void Up() {
            Create.Column("description").OnTable("SW_USERPROFILE").AsString(MigrationUtil.StringMedium).Nullable();
        }

        public override void Down() {
            Delete.Column("description").FromTable("SW_USERPROFILE");
        }
    }
}