using DocumentFormat.OpenXml.Wordprocessing;
using FluentMigrator;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.DB_Migration._4._0 {
    [Migration(201405150345)]
    public class Migration20140515Hap187 : FluentMigrator.Migration {
        public override void Up() {
            Create.Table("sw_extraattributes")
                .WithColumn("id").AsInt32().PrimaryKey("pk_swextravalues").Identity()
                .WithColumn("maximoid").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("maximotable").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("attributename").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("attributevalue").AsString(MigrationUtil.StringLarge).NotNullable();

            Create.Index("idx_extraattribute").OnTable("sw_extraattributes")
                .OnColumn("maximoid").Ascending()
                .OnColumn("maximotable").Ascending();

            Create.Index("idx_extraattribute2").OnTable("sw_extraattributes")
                .OnColumn("maximoid").Ascending()
                .OnColumn("maximotable").Ascending()
                .OnColumn("attributename").Ascending();
        }

        public override void Down() {

        }
    }
}