using System;
using System.Data;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace cts.commons.persistence.Util {
    public class BooleanToIntUserType : IUserType {
        public object Assemble(object cached, object owner) {
            return cached;
        }

        public object DeepCopy(object value) {
            return value;
        }

        public object Disassemble(object value) {
            return value;
        }

        public int GetHashCode(object x) {
            if (x == null)
                return 0;
            return x.GetHashCode();
        }
        public bool IsMutable {
            get { return false; }
        }

        async Task<object>  IUserType.NullSafeGet(IDataReader rs, string[] names, object owner) {
            var obj = NHibernateUtil.Int32.
             NullSafeGet(rs, names[0]);
            if (obj == null) {
                return false;
            }
            return (int)obj == 1;
        }

        async Task IUserType.NullSafeSet(IDbCommand cmd, object value, int index) {
            if (value == null) {
                ((IDataParameter)cmd.Parameters[index]).Value =
                      DBNull.Value;
            } else {
                var boolValue = (bool)value;
                ((IDataParameter)cmd.Parameters[index]).Value =
                      boolValue ? 1 : 0;
            }
        }

        // represents conversion on load-from-db operations:
        public object NullSafeGet(System.Data.IDataReader rs,
               string[] names, object owner) {
            var obj = NHibernateUtil.Int32.
                   NullSafeGet(rs, names[0]);
            if (obj == null) {
                return false;
            }
            return (int)obj == 1;
        }

        // represents conversion on save-to-db operations:
        public void NullSafeSet(System.Data.IDbCommand cmd,
               object value, int index) {
            if (value == null) {
                ((IDataParameter)cmd.Parameters[index]).Value =
                      DBNull.Value;
            } else {
                var boolValue = (bool)value;
                ((IDataParameter)cmd.Parameters[index]).Value =
                      boolValue ? 1 : 0;
            }
        }
        public object Replace(object original, object target,
               object owner) {
            return original;
        }



        bool IUserType.Equals(object x, object y) {
            return object.Equals(x, y);
        }



        public SqlType[] SqlTypes { get { return new[] { NHibernateUtil.String.SqlType }; } }
        public Type ReturnedType { get { return typeof(Boolean); } }
    }
}
