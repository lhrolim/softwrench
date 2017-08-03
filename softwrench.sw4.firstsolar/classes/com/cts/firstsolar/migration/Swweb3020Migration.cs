using cts.commons.persistence.Util;
using FluentMigrator;
using softwrench.sw4.api.classes.migration;
using softWrench.sW4.Extension;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {

    [Migration(201706201352)]
    public class Swweb3020Migration : Migration {

        public override void Up() {
            Create.Column("accesstoken").OnTable("OPT_WORKPACKAGE").AsString(MigrationUtil.StringMedium).Nullable();

            //            Create.Index("idx_wp_accesstoken").OnTable("OPT_WORKPACKAGE").OnColumn("accesstoken");

            if (!MigrationContext.IsMySql) {
                Execute.Sql("CREATE UNIQUE NONCLUSTERED INDEX idx_wp_accesstoken ON OPT_WORKPACKAGE(accesstoken) WHERE accesstoken IS NOT NULL; ");
            }


            Create.Table("OPT_WPEMAILSTATUS")
                .WithIdColumn()
                .WithColumn("workpackageid").AsInt32().NotNullable()
                .WithColumn("email").AsString(MigrationUtil.StringMedium)
                .WithColumn("operation").AsString(MigrationUtil.StringMedium)
                .WithColumn("qualifier").AsString(MigrationUtil.StringMedium)
                .WithColumn("senddate").AsDateTime().Nullable()
                .WithColumn("ackdate").AsDateTime().Nullable();
        }

        public override void Down() {

        }

    }


    [Migration(201708011352)]
    public class Swweb3020_2Migration : Migration {

        public override void Up() {
            Alter.Column("email").OnTable("OPT_WPEMAILSTATUS").AsString(MigrationUtil.StringMax);
        }

        public override void Down() {

        }

    }



}
