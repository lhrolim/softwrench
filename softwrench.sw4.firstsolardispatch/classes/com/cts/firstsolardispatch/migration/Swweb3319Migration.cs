using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.migration {

    [Migration(201712271700)]
    public class Swweb3319Migration : Migration {
        public override void Up() {
            Alter.Table("DISP_INVERTER").AddColumn("assetdescription").AsString(MigrationUtil.StringMedium).Nullable();
        }

        public override void Down() {
        }
    }
}
