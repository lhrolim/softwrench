using softWrench.sW4.Data.Offline;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {
    public class QueryCacheKey {
        public enum QueryMode { Count,List, Detail,Sync,Union }

        private readonly Rowstamps.RowstampMode _rowstamp;
        private readonly QueryMode _queryMode;

        public QueryCacheKey(Rowstamps.RowstampMode rowstamp, QueryMode queryMode) {
            _rowstamp = rowstamp;
            _queryMode = queryMode;
        }

        public QueryCacheKey(QueryMode queryMode) {
            _queryMode = queryMode;
        }

        protected bool Equals(QueryCacheKey other) {
            return _rowstamp == other._rowstamp && _queryMode == other._queryMode;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((QueryCacheKey)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((int)_rowstamp * 397) ^ (int)_queryMode;
            }
        }
    }
}
