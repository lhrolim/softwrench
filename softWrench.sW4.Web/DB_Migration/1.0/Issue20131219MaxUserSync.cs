using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._1._0 {
    
    [Migration(201312191505)]
    public class Issue20131219MaxUserSync : FluentMigrator.Migration {
        public override void Up()
        {
            Create.Column("criptoproperties").OnTable("SW_USER2").AsString(4000).Nullable();
        }

        public override void Down()
        {
            Delete.Column("criptoproperties").FromTable("SW_USER2");
        }
    }
}