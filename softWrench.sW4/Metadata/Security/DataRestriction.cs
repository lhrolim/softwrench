using System;

namespace softWrench.sW4.Metadata.Security
{
    public class DataRestriction
    {
        private readonly int _id;
        private readonly int _roleid;
        private readonly String _entityName;
        private readonly String _whereClause;

        public DataRestriction(int id, int roleid, string entityName, string whereClause)
        {
            _id = id;
            _roleid = roleid;
            _entityName = entityName;
            _whereClause = whereClause;
        }

        public int Id
        {
            get { return _id; }
        }

        public int Roleid
        {
            get { return _roleid; }
        }

        public string EntityName
        {
            get { return _entityName; }
        }

        public string WhereClause
        {
            get { return _whereClause; }
        }
    }
}
