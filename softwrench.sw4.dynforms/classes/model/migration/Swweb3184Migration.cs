using cts.commons.persistence.Util;
using FluentMigrator;
using softWrench.sW4.Extension;

namespace softwrench.sw4.dynforms.classes.model.migration {

    [Migration(201712171422)]
    public class Swweb3184Migration : Migration {

        public override void Up() {

            Alter.Table("FORM_METADATA_DEF")
                .AddColumn("detailserialized").AsBinary().Nullable()
                .AddColumn("listserialized").AsBinary().Nullable()
                .AddColumn("newdetailserialized").AsBinary().Nullable();

            IfDatabase("SqlServer").Execute.Sql("ALTER TABLE FORM_METADATA_DEF ALTER COLUMN detailserialized varbinary(max)");
            IfDatabase("SqlServer").Execute.Sql("ALTER TABLE FORM_METADATA_DEF ALTER COLUMN listserialized varbinary(max)");
            IfDatabase("SqlServer").Execute.Sql("ALTER TABLE FORM_METADATA_DEF ALTER COLUMN newdetailserialized varbinary(max)");


            //            Alter.Table("FORM_METADATA_DEF")
            //                .AlterColumn("detaildefinition").AsString(MigrationUtil.StringMax).Nullable()
            //                .AlterColumn("listdefinition").AsString(MigrationUtil.StringMax).Nullable()
            //                .AlterColumn("newdetaildefinition").AsString(MigrationUtil.StringMax).Nullable();



        }

        public override void Down() {
        }
    }
}

