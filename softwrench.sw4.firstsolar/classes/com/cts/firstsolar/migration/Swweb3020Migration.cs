using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.migration {

    [Migration(201706201352)]
    public class Swweb3020Migration : Migration {

        public override void Up() {
            Create.Column("accesstoken").OnTable("OPT_WORKPACKAGE").AsString(MigrationUtil.StringMedium).Nullable();

//            Create.Index("idx_wp_accesstoken").OnTable("OPT_WORKPACKAGE").OnColumn("accesstoken");

            Execute.Sql("CREATE UNIQUE NONCLUSTERED INDEX idx_wp_accesstoken ON OPT_WORKPACKAGE(accesstoken) WHERE accesstoken IS NOT NULL; ");

            Create.Table("OPT_WPEMAILSTATUS")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
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



}
