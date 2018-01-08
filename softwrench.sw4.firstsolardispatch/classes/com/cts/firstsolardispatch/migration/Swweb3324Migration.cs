using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.migration {

    [Migration(201801081701)]
    public class Swweb3324Migration : Migration {
        public override void Up() {
            Alter.Table("DISP_TICKET").AddColumn("primarycontactemail").AsString(MigrationUtil.StringMedium).Nullable();
            Alter.Table("DISP_TICKET").AddColumn("escalationcontactemail").AsString(MigrationUtil.StringMedium).Nullable();

            Execute.Sql("update disp_ticket set primarycontactemail = s.primarycontactemail, escalationcontactemail = s.escalationcontactemail from disp_ticket as t inner join gfed_site s on t.gfedid = s.gfedid");

        }

        public override void Down() {
        }
    }
}
