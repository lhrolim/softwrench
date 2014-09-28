using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using System;
using System.Data;

namespace softWrench.sW4.Util.NHibernate {
    class BooleanToShortUserType : IUserType {
        public bool Equals(object x, object y) {
            throw new NotImplementedException();
        }

        public int GetHashCode(object x) {
            throw new NotImplementedException();
        }

        public object NullSafeGet(IDataReader rs, string[] names, object owner) {
            throw new NotImplementedException();
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index) {
            throw new NotImplementedException();
        }

        public object DeepCopy(object value) {
            throw new NotImplementedException();
        }

        public object Replace(object original, object target, object owner) {
            throw new NotImplementedException();
        }

        public object Assemble(object cached, object owner) {
            throw new NotImplementedException();
        }

        public object Disassemble(object value) {
            throw new NotImplementedException();
        }

        public SqlType[] SqlTypes {
            get {
                throw new NotImplementedException();
            }
        }

        public Type ReturnedType {
            get {
                throw new NotImplementedException();
            }
        }
        public bool IsMutable {
            get {
                throw new NotImplementedException();
            }
        }
    }
}
