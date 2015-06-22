using System;
using cts.commons.persistence;

namespace softwrench.sw4.problem.classes {
    public class ProblemManager : IProblemManager {

        private readonly ISWDBHibernateDAO _swdbHibernateDAO;

        public ProblemManager(ISWDBHibernateDAO swdbHibernateDAO) {
            _swdbHibernateDAO = swdbHibernateDAO;
        }





        public Problem Register(string recordType, string recordId, string datamap, int? createdBy,string stackTrace,
            string message, string handler = null, string assignee = null, int priority = 1, string profiles = null) {
            var problem = new Problem(recordType, recordId, datamap, DateTime.Now,
                createdBy, assignee, priority, stackTrace, message,
                profiles, handler, ProblemStatus.Open.ToString());
            return _swdbHibernateDAO.Save(problem);
        }

        public void List() {
            throw new NotImplementedException();
        }

        public void ListAssigned() {
            throw new NotImplementedException();
        }

        public void Submit() {
            throw new NotImplementedException();
        }

        public void Resolve() {
            throw new NotImplementedException();
        }

        public void Detail() {
            throw new NotImplementedException();
        }
    }
}
