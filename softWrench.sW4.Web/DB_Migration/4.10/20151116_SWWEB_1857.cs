using FluentMigrator;
using softWrench.sW4.Extension;


namespace softWrench.sW4.Web.DB_Migration._4._10
{
    [Migration(201511161615)]
    public class Migration201511161615SWWEB1857 : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("PREF_GENERAL")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("UserId").AsInt64().Nullable()
                .WithColumn("ProfileId").AsInt64().Nullable()
                .WithColumn("Signature").AsClob().Nullable();
        }

        public override void Down()
        {
            Delete.Table("PREF_GENERAL");
        }
    }
}