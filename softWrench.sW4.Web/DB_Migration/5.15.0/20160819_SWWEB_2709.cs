using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softWrench.sW4.Web.DB_Migration._5._15._0 {
    [Migration(201608191800)]
    public class MigrationSWWEB2709 : Migration {
        public override void Up() {
            Create.Table("DYN_SCRIPT_ENTRY")
                .WithIdColumn(true)
                .WithColumn("Name").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("Target").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("Description").AsString(MigrationUtil.StringLarge).Nullable()
                .WithColumn("Script").AsString(MigrationUtil.StringMax).NotNullable()
                .WithColumn("Lastupdate").AsDateTime().NotNullable()
                .WithColumn("Deploy").AsBoolean().WithDefaultValue(false)
                .WithColumn("Isoncontainer").AsBoolean().WithDefaultValue(false)
                .WithColumn("Isuptodate").AsBoolean().WithDefaultValue(false)
                .WithColumn("Appliestoversion").AsString(MigrationUtil.StringSmall).NotNullable();
        }

        public override void Down() {
            Delete.Table("DYN_SCRIPT_ENTRY");
        }
    }
}