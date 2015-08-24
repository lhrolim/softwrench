using FluentMigrator;

namespace softWrench.sW4.Web.DB_Migration._1._0 {

    [Migration(201401011440)]
    public class Issue20140101ModifyingSwUser2 : FluentMigrator.Migration {
        public override void Up() {
            Alter.Column("siteid").OnTable("SW_USER2").AsString(50).Nullable();
            Alter.Column("orgid").OnTable("SW_USER2").AsString(50).Nullable();
        }

        public override void Down() {
            Delete.Column("siteid").FromTable("SW_USER2");
            Delete.Column("orgid").FromTable("SW_USER2");
        }
    }
}