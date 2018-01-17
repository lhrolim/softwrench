using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softwrench.sw4.dynforms.classes.model.migration {

    [Migration(201801161742)]
    public class Swweb3356Migration : Migration {

        public override void Up() {

            Create.Table("FORM_METADATA_OPTIONS")
                .WithIdColumn()
                .WithColumn("alias").AsString(MigrationUtil.StringSmall).NotNullable()
                .WithColumn("description").AsString(MigrationUtil.StringLarge).Nullable()
                .WithColumn("list").AsBinary().Nullable();


            IfDatabase("SqlServer").Execute.Sql("ALTER TABLE FORM_METADATA_OPTIONS ALTER COLUMN list varbinary(max)");

        }

        public override void Down() {
        }
    }
}

