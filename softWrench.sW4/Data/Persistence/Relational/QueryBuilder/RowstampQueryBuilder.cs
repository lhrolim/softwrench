using System.Collections.Generic;
using cts.commons.portable.Util;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Data.Search;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {

    public class RowstampQueryBuilder : IWhereBuilder {
        //TODO: this will work on MSSQL Maximos, but need to review for DB2/ Oracle
        private const string Both = " Cast({0}.rowstamp as BIGINT) > :lowerrowstamp and Cast({0}.rowstamp as BIGINT) < :upperrowstamp ";
        private const string Upper = " CAST({0}.rowstamp as BIGINT)< :upperrowstamp ";
        private const string Lower = " CAST({0}.rowstamp as BIGINT) > :lowerrowstamp ";

        private readonly Rowstamps _rowstamps;

        public RowstampQueryBuilder(Rowstamps rowstamps) {
            _rowstamps = rowstamps;
        }
        
        public string BuildWhereClause(string entityName, SearchRequestDto searchDto = null) {
            if (_rowstamps.CurrentMode() == Rowstamps.RowstampMode.None) {
                return null;
            }
            string sql = null;
            switch (_rowstamps.CurrentMode()) {
                case Rowstamps.RowstampMode.Both:
                    sql = Both.Fmt(entityName);
                    break;
                case Rowstamps.RowstampMode.Lower:
                    sql = Lower.Fmt(entityName);
                    break;
                case Rowstamps.RowstampMode.Upper:
                    sql = Upper.Fmt(entityName);
                    break;
            }
            return sql;

        }

        public IDictionary<string, object> GetParameters() {
            return _rowstamps.GetParameters();
        }
    }
}
