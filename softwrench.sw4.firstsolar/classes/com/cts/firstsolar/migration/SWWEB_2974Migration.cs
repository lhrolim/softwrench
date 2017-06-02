using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {

    /// <summary>
    /// Migration for the Outage OPT data
    /// </summary>
    [Migration(201706021303)]
    public class SWWEB_2974Migration : Migration {

        public override void Up() {

            Alter.Column("subcontractorid").OnTable("OPT_CALLOUT").AsString(MigrationUtil.StringSmall).NotNullable();
            Create.Column("subcontractorname").OnTable("OPT_CALLOUT").AsString(MigrationUtil.StringMedium).NotNullable().WithDefaultValue("");

        }

        public override void Down() {

        }

    }




}
