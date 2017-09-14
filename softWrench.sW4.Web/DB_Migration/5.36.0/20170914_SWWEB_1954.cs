using cts.commons.persistence.Util;
using FluentMigrator;
using softwrench.sw4.api.classes.migration;
using softWrench.sW4.Extension;

namespace softWrench.sW4.Web.DB_Migration._5._36._0 {
    [Migration(201709140630)]

    public class Migration20170914Swweb1954 : Migration {

        public override void Up() {

            Create.Table("HELP_ENTRY")
                .WithIdColumn()
                .WithColumn("label").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("documentname").AsString(MigrationUtil.StringMedium).Nullable()
                .WithColumn("type_").AsString(MigrationUtil.StringMedium).NotNullable()
                .WithColumn("url").AsString(MigrationUtil.StringLarge).Nullable();


            if (MigrationContext.IsMySql) {
                Alter.Table("HELP_ENTRY").AddColumn("data").AsBinary().Nullable();
            } else {
                Alter.Table("HELP_ENTRY").AddColumn("data").AsBinary((int.MaxValue)).Nullable();
            }
        }

        public override void Down() {

        }
    }
}
