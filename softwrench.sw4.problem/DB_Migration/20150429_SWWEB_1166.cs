using FluentMigrator;
using FluentMigrator.Expressions;

namespace softWrench.sW4.Web.DB_Migration._4._2._0 {

    [Migration(201504290935)]
    public class Migration20150429SWWEB1166 : Migration {

        public override void Up()
        {
            Create.Index("PROB_PROBLEM_ASSIGNEE").OnTable("PROB_PROBLEM")
                .OnColumn("Assignee");

            Create.Index("PROB_PROBLEM_TYPE").OnTable("PROB_PROBLEM")
                .OnColumn("RecordType");

            Create.ForeignKey("FK_PROBLEM").FromTable("PROB_ADDITIONALARGS")
                .ForeignColumn("ProblemId").ToTable("PROB_PROBLEM")
                .PrimaryColumn("Id");
        }

        public override void Down() {

        }
    }
}