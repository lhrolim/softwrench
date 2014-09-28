using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._1._0 {

    [Migration(201401061645)]
    public class Issue20140106ModifyingSwUser2 : FluentMigrator.Migration {
        public override void Up() {
            Alter.Column("password").OnTable("SW_USER2").AsString(255).Nullable();
        }

        public override void Down() {
            Delete.Column("password").FromTable("SW_USER2");
        }
    }
}