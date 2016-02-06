using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;

namespace softWrench.sW4.Web.DB_Migration.DB2_FluentMigrator {
    class EmptyDescriptionGenerator : IDescriptionGenerator {
        public IEnumerable<string> GenerateDescriptionStatements(CreateTableExpression expression) {
            return Enumerable.Empty<string>();
        }

        public string GenerateDescriptionStatement(AlterTableExpression expression) {
            return string.Empty;
        }

        public string GenerateDescriptionStatement(CreateColumnExpression expression) {
            return string.Empty;
        }

        public string GenerateDescriptionStatement(AlterColumnExpression expression) {
            return string.Empty;
        }
    }
}
