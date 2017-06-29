using FluentMigrator;
using softWrench.sW4.Extension;


namespace softWrench.sW4.Web.DB_Migration._4._10
{
    [Migration(201511161618)]
    public class Migration201511161618SWWEB1857 : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("PREF_GENERALUSER")
                .WithIdColumn()
                .WithColumn("user_id").AsInt32().ForeignKey("fk_prefgeneraluser_user", "SW_USER2", "id").Nullable()
                .WithColumn("signature").AsClob().Nullable();
        }

        public override void Down()
        {
            Delete.Table("PREF_GENERALUSER");
        }
    }
}