using FluentMigrator;
using softWrench.sW4.Web.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace softWrench.sW4.Web.DB_Migration._4._0 {
    [Migration(201412101300)]
    public class Migration20141210Swweb717 : FluentMigrator.Migration {
        public override void Up() {
            Alter.Table("sw_user2").AddColumn("storeloc").AsString(MigrationUtil.StringMedium).Nullable();
        }

        public override void Down() {
            Delete.Column("storeloc").FromTable("sw_user2");
        }
    }
}