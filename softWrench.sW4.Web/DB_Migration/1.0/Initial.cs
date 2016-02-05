using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._1._0
{
    [Migration(201309121453)]
    public class Initial : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("SW_ROLE")
                  .WithColumn("ID").AsInt32().PrimaryKey().Identity()
                  .WithColumn("name").AsString(100).Unique()
                  .WithColumn("isactive").AsBoolean().WithDefaultValue(true);

            Create.Table("SW_USER2")
                  .WithColumn("ID").AsInt32().PrimaryKey().Identity()
                  .WithColumn("username").AsString(50).Unique("sw_user_uq_username").NotNullable()
                  .WithColumn("password").AsString(255).NotNullable()
                  .WithColumn("firstname").AsString(100).NotNullable()
                  .WithColumn("lastname").AsString(100).NotNullable()
                  .WithColumn("isactive").AsBoolean()
                  .WithColumn("orgid").AsString(50)
                  .WithColumn("siteid").AsString(50)
                  .WithColumn("email").AsString(255).Nullable();
            

            Create.Table("SW_USERPROFILE")
                  .WithColumn("ID").AsInt32().PrimaryKey().Identity()
                  .WithColumn("name").AsString(100).Unique();

            Create.Table("SW_DATACONSTRAINT")
                 .WithColumn("ID").AsInt32().PrimaryKey().Identity()
                 .WithColumn("whereclause").AsString(4000).NotNullable()
                 .WithColumn("entityname").AsString(100).NotNullable()
                 .WithColumn("isactive").AsBoolean().WithDefaultValue(true);


            Create.Table("SW_USER_CUSTOMROLE")
                  .WithColumn("ID").AsInt32().PrimaryKey().Identity()
                  .WithColumn("user_id").AsInt32().ForeignKey("fk_user_role_user", "SW_USER2", "ID").Nullable()
                  .WithColumn("role_id").AsInt32().ForeignKey("fk_user_role_role", "SW_ROLE", "id")
                  .WithColumn("exclusion").AsBoolean().WithDefaultValue(false);
                  

            Create.Table("SW_USER_CUSTOMDATACONSTRAINT")
                  .WithColumn("ID").AsInt32().PrimaryKey().Identity()
                  .WithColumn("user_id").AsInt32().ForeignKey("fk_user_dc_user", "SW_USER2", "ID").Nullable()
                  .WithColumn("constraint_id").AsInt32().ForeignKey("fk_user_dc_contraint", "SW_DATACONSTRAINT", "ID")
                  .WithColumn("exclusion").AsBoolean().WithDefaultValue(false);
            

            Create.Table("SW_USERPROFILE_ROLE")
                  .WithColumn("ID").AsInt32().PrimaryKey().Identity()
                  .WithColumn("profile_id").AsInt32().ForeignKey("fk_userp_profile", "SW_USERPROFILE", "id")
                  .WithColumn("role_id").AsInt32().ForeignKey("fk_userp_role", "SW_ROLE", "id");
                  

            Create.Table("SW_USERPROFILE_DATACONSTRAINT")
                  .WithColumn("profile_id").AsInt32().ForeignKey("fk_userp_dc_profile", "SW_USERPROFILE", "id")
                  .WithColumn("constraint_id").AsInt32().ForeignKey("fk_userp_dc_contraint", "SW_DATACONSTRAINT", "ID");


            Create.Table("SW_USER_USERPROFILE")
                  .WithColumn("user_id").AsInt32().ForeignKey("fk_userp_user_user", "SW_USER2", "ID")
                  .WithColumn("profile_id").AsInt32().ForeignKey("fk_userp_user_profile", "SW_USERPROFILE", "id");

        }

        public override void Down()
        {
            Delete.Table("SW_DATACONSTRAINT");
            Delete.Table("SW_ROLE");
            Delete.Table("SW_USER2");
            Delete.Table("SW_USER_PROFILE");
            Delete.Table("SW_USER_ROLE");
            Delete.Table("SW_USER_DATACONSTRAINT");
            Delete.Table("SW_USERPROFILE_ROLE");
            Delete.Table("SW_USERPROFILE_DATACONSTRAINT");
        }
    }
}
