using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.user.Migration {
    [Migration(201602091139)]
    public class MigrationSwweb1216 : FluentMigrator.Migration {

        public override void Up()
        {

            Create.Table("SEC_APPLICATION_PER")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("profile_id").AsInt32().ForeignKey("fk_secapprid", "sw_userprofile", "id").Nullable()
                .WithColumn("applicationname").AsString().NotNullable()
                .WithColumn("allowcreation").AsBoolean().WithDefaultValue(true)
                .WithColumn("allowupdate").AsBoolean().WithDefaultValue(true)
                .WithColumn("allowremoval").AsBoolean().WithDefaultValue(true)
                .WithColumn("allowview").AsBoolean().WithDefaultValue(false);


            Create.Table("SEC_ACTION_PER")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("actionid").AsString().NotNullable()
                .WithColumn("schema_").AsString().NotNullable()
                .WithColumn("app_id").AsInt32().ForeignKey("fk_ap_ap", "SEC_APPLICATION_PER", "id").Nullable();


            Create.Table("SEC_COMPOSITION_PER")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("allowcreation").AsBoolean().WithDefaultValue(true)
                .WithColumn("allowupdate").AsBoolean().WithDefaultValue(true)
                .WithColumn("allowremoval").AsBoolean().WithDefaultValue(true)
                .WithColumn("allowview").AsBoolean().WithDefaultValue(false)
                .WithColumn("schema_").AsString().NotNullable()
                .WithColumn("compositionkey").AsString().NotNullable()
                .WithColumn("app_id").AsInt32().ForeignKey("fk_cp_ap", "SEC_APPLICATION_PER", "id").Nullable();



            Create.Table("SEC_CONTAINER_PER")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("schema_").AsString().NotNullable()
                .WithColumn("containerkey").AsString().NotNullable()
                .WithColumn("app_id").AsInt32().ForeignKey("fk_sp_ap", "SEC_APPLICATION_PER", "id").Nullable();


            Create.Table("SEC_FIELD_PER")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("fieldkey").AsString().NotNullable()
                .WithColumn("permission").AsString()
                .WithColumn("schema_id").AsInt32().ForeignKey("fk_fp_sp", "SEC_CONTAINER_PER", "id").Nullable()
                .WithColumn("composition_id").AsInt32().ForeignKey("fk_fp_cp", "SEC_COMPOSITION_PER", "id").Nullable();
        }

        public override void Down() {
        }
    }
}
