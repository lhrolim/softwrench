using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {

    [Migration(201706201103)]
    public class Swweb3022Migration : Migration {

        public override void Up() {
            Create.Column("cc").OnTable("OPT_MAINTENANCE_ENG").AsString(MigrationUtil.StringLarge);
        }

        public override void Down() {

        }

    }



}
