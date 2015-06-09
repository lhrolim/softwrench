using System.Collections.Generic;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Data.Search;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {

    class RowstampQueryBuilder : IWhereBuilder {
        private const string Both = " Cast(rowstamp as BIGINT) > :lowerrowstamp and Cast(rowstamp as BIGINT) < :upperrowstamp ";
        private const string Upper = " CAST(rowstamp as BIGINT)< :upperrowstamp ";
        private const string Lower = " CAST(rowstamp as BIGINT) > :lowerrowstamp ";

        private readonly Rowstamps _rowstamps;

        public RowstampQueryBuilder(Rowstamps rowstamps) {
            _rowstamps = rowstamps;
        }
        
        public string BuildWhereClause(string entityName, SearchRequestDto searchDto = null) {
            if (_rowstamps.CurrentMode() == Rowstamps.RowstampMode.None) {
                return null;
            }
            IDictionary<string, object> parameters = _rowstamps.GetParameters();
            string sql = null;
            switch (_rowstamps.CurrentMode()) {
                case Rowstamps.RowstampMode.Both:
                    sql = Both;
                    break;
                case Rowstamps.RowstampMode.Lower:
                    sql = Lower;
                    break;
                case Rowstamps.RowstampMode.Upper:
                    sql = Upper;
                    break;
            }
            return sql;

        }

        public IDictionary<string, object> GetParameters() {
            return _rowstamps.GetParameters();
        }
    }
}
