using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._1._0 {

    [Migration(201401131803)]
    public class Issue20140113ModifyingSwUser2 : FluentMigrator.Migration {
        public override void Up() {
//            Alter.Column("criptoproperties").OnTable("SW_USER2").AsString(4000).Nullable();
            Alter.Column("password").OnTable("SW_USER2").AsString(255).Nullable();
//            Alter.Column("department").OnTable("SW_USER2").AsString(200).Nullable();
//            Alter.Column("phone").OnTable("SW_USER2").AsString(20).Nullable();
//            Alter.Column("language").OnTable("SW_USER2").AsString(20).Nullable();
        }

        public override void Down() {
            Delete.Column("criptoproperties").FromTable("SW_USER2");
            Delete.Column("password").FromTable("SW_USER2");
            Delete.Column("department").FromTable("SW_USER2");
            Delete.Column("phone").FromTable("SW_USER2");
            Delete.Column("language").FromTable("SW_USER2");
        }
    }
}