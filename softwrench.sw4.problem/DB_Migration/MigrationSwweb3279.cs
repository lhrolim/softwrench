using System;
using cts.commons.persistence.Util;
using FluentMigrator;

namespace softWrench.sw4.problem.DB_Migration {

    [Migration(201711231152)]
    public class MigrationSwweb3279 : Migration {

        public override void Up() {
            Execute.Sql("alter table prob_problem alter column data VARBINARY(max)");

        }

        public override void Down() {

        }
    }
}