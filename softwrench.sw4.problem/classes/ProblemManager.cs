using System;
using cts.commons.persistence;

namespace softwrench.sw4.problem.classes {
    public class ProblemManager : IProblemManager {

        public ProblemManager(ISWDBHibernateDAO SWDBHibernateDAO)
        {
            this.SWDBHibernateDAO = SWDBHibernateDAO;
        }

        private ISWDBHibernateDAO SWDBHibernateDAO;

        public Problem Register(string recordType, string recordId, string data, 
            DateTime createdDate, string createdBy, string assignee,
            int priority, string stackTrace, string description, 
            string profiles, string problemHandler, string status = null)
        {
            var problem = new Problem(recordType, recordId, data, createdDate, 
                createdBy, assignee, priority, stackTrace, description, 
                profiles, problemHandler, status);
            return SWDBHibernateDAO.Save(problem);
        }

        public void List()
        {
            throw new NotImplementedException();
        }

        public void ListAssigned()
        {
            throw new NotImplementedException();
        }

        public void Submit()
        {
            throw new NotImplementedException();
        }

        public void Resolve()
        {
            throw new NotImplementedException();
        }

        public void Detail()
        {
            throw new NotImplementedException();
        }
    }
}
