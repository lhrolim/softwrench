using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.user.Migration {
    [Migration(201602091139)]
    public class MigrationSwweb1216 : FluentMigrator.Migration {

        public override void Up() {

            Create.Table("SEC_APPLICATION_PER")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("creationdate").AsDateTime().NotNullable()
                .WithColumn("updatedate").AsDateTime().NotNullable()
                .WithColumn("createdby").AsInt32().ForeignKey("fk_secapcrus", "sw_user2", "id").NotNullable()
                .WithColumn("profile_id").AsInt32().ForeignKey("fk_secapprid", "sw_userprofile", "id").NotNullable()
                .WithColumn("applicationname").AsString().NotNullable()
                .WithColumn("allowcreation").AsBoolean().WithDefaultValue(false)
                .WithColumn("allowupdate").AsBoolean().WithDefaultValue(false)
                .WithColumn("allowremoval").AsBoolean().WithDefaultValue(false);


            Create.Table("SEC_SCHEMAGROUP_PER")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("mode").AsString().NotNullable()
                .WithColumn("schema").AsString()
                .WithColumn("app_id").AsInt32().ForeignKey("fk_sg_ap", "SEC_APPLICATION_PER", "id").NotNullable();


            Create.Table("SEC_ACTION_PER")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("creationdate").AsDateTime().NotNullable()
                .WithColumn("updatedate").AsDateTime().NotNullable()
                .WithColumn("createdby").AsInt32().ForeignKey("fk_secacpcrus", "sw_user2", "id").NotNullable()
                .WithColumn("actionid").AsString().NotNullable()
                .WithColumn("schemagroup_id").AsInt32().ForeignKey("fk_ap_sgp", "SEC_SCHEMAGROUP_PER", "id").NotNullable();


            Create.Table("SEC_COMPOSITION_PER")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("creationdate").AsDateTime().NotNullable()
                .WithColumn("updatedate").AsDateTime().NotNullable()
                .WithColumn("createdby").AsInt32().ForeignKey("fk_seccpcrus", "sw_user2", "id").NotNullable()
                .WithColumn("compositionkey").AsString().NotNullable()
                .WithColumn("allowcreation").AsBoolean().WithDefaultValue(false)
                .WithColumn("allowupdate").AsBoolean().WithDefaultValue(false)
                .WithColumn("allowremoval").AsBoolean().WithDefaultValue(false)
                .WithColumn("schemagroup_id").AsInt32().ForeignKey("fk_cp_sgp", "SEC_SCHEMAGROUP_PER", "id").NotNullable();



            Create.Table("SEC_SCHEMA_PER")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("creationdate").AsDateTime().NotNullable()
                .WithColumn("updatedate").AsDateTime().NotNullable()
                .WithColumn("createdby").AsInt32().ForeignKey("fk_secspcrus", "sw_user2", "id").NotNullable()
                .WithColumn("schemakey").AsString().NotNullable()
                .WithColumn("readonly").AsBoolean().WithDefaultValue(false)
                .WithColumn("schemagroup_id").AsInt32().ForeignKey("fk_sp_sgp", "SEC_SCHEMAGROUP_PER", "id").NotNullable();

            Create.Table("SEC_FIELD_PER")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("creationdate").AsDateTime().NotNullable()
                .WithColumn("updatedate").AsDateTime().NotNullable()
                .WithColumn("createdby").AsInt32().ForeignKey("fk_secfpcrus", "sw_user2", "id").NotNullable()
                .WithColumn("fieldkey").AsString().NotNullable()
                .WithColumn("readonly").AsBoolean().WithDefaultValue(false)
                .WithColumn("schema_id").AsInt32().ForeignKey("fk_fp_sp", "SEC_SCHEMA_PER", "id")
                .WithColumn("composition_id").AsInt32().ForeignKey("fk_fp_cp", "SEC_COMPOSITION_PER", "id");
        }

        public override void Down() {
        }
    }
}
