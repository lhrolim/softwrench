using cts.commons.persistence.Util;
using FluentMigrator;

namespace softwrench.sw4.user.Migration {
    [Migration(201611221200)]
    public class MigrationSwweb2827 : FluentMigrator.Migration {

        public override void Up()
        {

            Create.Table("USER_PASSHISTORY")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("password").AsString(MigrationUtil.StringLarge).NotNullable()
                .WithColumn("registertime").AsDateTime().NotNullable()
                .WithColumn("userid").AsInt32().NotNullable().ForeignKey("fk_user_pass_user","SW_USER2", "ID");


            Alter.Table("SW_USER2").AddColumn("passwordexpirationtime").AsDateTime().Nullable();
            Alter.Table("SW_USER2").AddColumn("locked").AsBoolean().WithDefaultValue(false).Nullable();

            Create.Table("USER_AUTHATTEMPT")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("registertime").AsDateTime().Nullable()
                .WithColumn("numberofattempts").AsInt32().NotNullable()
                .WithColumn("globalnumberofattempts").AsInt32().NotNullable()
                .WithColumn("userid").AsInt32().ForeignKey("fk_auth_attempt_user", "SW_USER2", "ID");


            Create.UniqueConstraint("usr_uq_authattempt").OnTable("USER_AUTHATTEMPT").Column("userid");

            Execute.Sql("delete from PREF_GENERALUSER where user_id is null");

            Create.UniqueConstraint("usr_uq_prefuser").OnTable("PREF_GENERALUSER").Column("user_id");

            Alter.Table("SW_USERPROFILE").AddColumn("applybydefault").AsBoolean().WithDefaultValue(false);
        }

        public override void Down() {
        }
    }
}
