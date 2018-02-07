using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence.Util;
using FluentMigrator;
using softwrench.sw4.api.classes.migration;
using softWrench.sW4.Extension;

namespace softwrench.sw4.offlineserver.migration {

    [Migration(201802061525)]
    public class MigrationSwweb3402 : Migration {

        public override void Up() {

            Create.Table("OFF_SYNCOPERATION_INPUT").WithIdColumn(true)
                .WithColumn("operation_id").AsInt32().ForeignKey("sw_offsoi_so", "OFF_SYNCOPERATION", "id").NotNullable()
                .WithColumn("_key").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("value").AsString(MigrationUtil.StringMax).NotNullable();


            Alter.Table("OFF_SYNCOPERATION").AddColumn("initialload").AsBoolean().Nullable();
        }

        public override void Down() {
        }
    }





}
