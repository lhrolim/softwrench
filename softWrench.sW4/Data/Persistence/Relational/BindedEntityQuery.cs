using System;
using System.Collections.Generic;
using System.Dynamic;
using JetBrains.Annotations;

namespace softWrench.sW4.Data.Persistence.Relational {
    public class BindedEntityQuery {
        private readonly string _sql;
        private dynamic _parameters;

        public BindedEntityQuery([NotNull] string sql, [CanBeNull] IEnumerable<KeyValuePair<string, object>> parameters) {
            if (sql == null) throw new ArgumentNullException("sql");

            _sql = sql;
            _parameters = BuildDynamic(parameters);
        }

        private object BuildDynamic(IEnumerable<KeyValuePair<string, object>> parameters) {
            var eo = new ExpandoObject();
            var eoColl = (ICollection<KeyValuePair<string, object>>)eo;

            foreach (var kvp in parameters) {
                eoColl.Add(kvp);
            }
            return eo;
        }

        [NotNull]
        public string Sql {
            get { return _sql; }
        }

        [CanBeNull]
        public ExpandoObject Parameters {
            get { return _parameters; }
        }
    }
}