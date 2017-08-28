using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softwrench.sw4.offlineserver.migration {

    [Migration(201708231925)]
    public class MigrationSwoff298 : Migration {

        public override void Up() {

            Create.Table("OFF_SYNCOPERATION").WithIdColumn()
                .WithColumn("trail_id").AsInt32().ForeignKey("sw_offso_at", "AUDI_TRAIL", "id").NotNullable()
                .WithColumn("serverversion").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("serverenv").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("metadatadownload").AsBoolean()
                .WithColumn("hasuploadoperation").AsBoolean()
                .WithColumn("clientversion").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("user_id").AsInt32().ForeignKey("sw_offso_user2", "sw_user2", "id").NotNullable()
                .WithColumn("user_tzoffset").AsInt32().NotNullable()
                .WithColumn("registertime").AsDateTime().NotNullable()
                .WithColumn("user_properties").AsString(MigrationUtil.StringMax).NotNullable()
                .WithColumn("device_platform").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("device_version").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("device_model").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("compositioncounts").AsInt32()
                .WithColumn("associationcounts").AsInt32()
                .WithColumn("topappcounts").AsInt32();

            Alter.Table("AUDI_TRAIL").AddColumn("externalid").AsString(MigrationUtil.StringMedium).Nullable();

            Alter.Table("audit_entry").AddColumn("trail_id").AsInt32().ForeignKey("sw_ae_at", "AUDI_TRAIL", "id").Nullable();

            Alter.Table("BAT_BATCH").AddColumn("clientoperationid").AsString(MigrationUtil.StringMedium).Nullable();

            Alter.Table("BAT_BATCHITEM").AddColumn("sentxml").AsBinary().Nullable();

            Alter.Table("AUD_SESSION").AddColumn("cookie").AsString(MigrationUtil.StringMax).Nullable();
            Alter.Table("AUD_SESSION").AddColumn("timezoneoffset").AsInt32().Nullable();


        }

        public override void Down() {
        }
    }


    [Migration(201708251205)]
    public class MigrationSwoff298_1 : Migration {

        public override void Up() {
            Create.Table("AUD_QUERY").WithIdColumn(true)
                .WithColumn("ellapsedmillis").AsInt64().Nullable()
                .WithColumn("countResult").AsInt32()
                .WithColumn("registertime").AsDateTime().NotNullable()
                .WithColumn("query").AsString(MigrationUtil.StringMax).NotNullable()
                .WithColumn("qualifier").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("trail_id").AsInt32().ForeignKey("sw_audq_at", "AUDI_TRAIL", "id").NotNullable();
        }

        public override void Down() {
        }
    }


    [Migration(201708281425)]
    public class MigrationSwoff298_2 : Migration {

        public override void Up() {
            Alter.Table("audit_entry").AlterColumn("data").AsBinary(int.MaxValue);
        }

        public override void Down() {
        }
    }
}
