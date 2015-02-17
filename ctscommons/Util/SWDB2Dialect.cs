using NHibernate;
using NHibernate.Dialect;
using NHibernate.Mapping;
using NHibernate.SqlTypes;
using System.Data;

namespace softWrench.sW4.Util {
    public class SWDB2Dialect : DB2Dialect {

        private readonly TypeNames castTypeNames = new TypeNames();

        public SWDB2Dialect():base() {
            RegisterColumnType(DbType.Boolean, "TINYINT(1)"); // SELECT IF(0, 'true', 'false');
        }

        public override string GetCastTypeName(SqlType sqlType)
        {
            var type = base.GetCastTypeName(sqlType);


            string result = castTypeNames.Get(sqlType.DbType, Column.DefaultLength, Column.DefaultPrecision, Column.DefaultScale);

            if (result == null) {
                throw new HibernateException(string.Format("No CAST() type mapping for SqlType {0}", sqlType));
            }
            return result;
        }
    }
}
