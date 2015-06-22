using System;
using cts.commons.persistence;

namespace softwrench.sw4.problem.classes {
    public class ProblemManager : IProblemManager {

        private readonly ISWDBHibernateDAO _swdbHibernateDAO;

        public ProblemManager(ISWDBHibernateDAO swdbHibernateDAO) {
            _swdbHibernateDAO = swdbHibernateDAO;
        }



        public Problem Register(string recordType, string recordId,string stackTrace,string data,
            string createdBy, string assignee, int priority, string description,
            string profiles, string problemHandler) {
            var problem = new Problem(recordType, recordId, data, DateTime.Now,
                createdBy, assignee, priority, stackTrace, description,
                profiles, problemHandler, ProblemStatus.Open.ToString());
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
