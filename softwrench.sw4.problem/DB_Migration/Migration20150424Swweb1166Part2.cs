using FluentMigrator;

namespace softwrench.sw4.problem.DB_Migration {

    [Migration(201506221500)]
    public class Migration20150424Swweb1166Part2 : Migration {

        public override void Up() {
            Alter.Column("CreatedBy").OnTable("PROB_PROBLEM").AsInt32();
            Alter.Column("RecordId").OnTable("PROB_PROBLEM").AsString().Nullable();
            Alter.Column("data").OnTable("PROB_PROBLEM").AsBinary().Nullable();
            Rename.Column("description").OnTable("PROB_PROBLEM").To("message");
        }

        public override void Down() {

        }
    }
}