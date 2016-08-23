using System;
using cts.commons.persistence;
using cts.commons.portable.Util;
using log4net;

namespace softwrench.sw4.problem.classes {
    public class ProblemManager : IProblemManager {

        private readonly ISWDBHibernateDAO _swdbHibernateDAO;
        private readonly ProblemHandlerLookuper _problemHandlerLookuper;
        private readonly ILog _log = LogManager.GetLogger(typeof (ProblemManager));

        public ProblemManager(ISWDBHibernateDAO swdbHibernateDAO, ProblemHandlerLookuper problemHandlerLookuper) {
            _log.DebugFormat("init");
            _swdbHibernateDAO = swdbHibernateDAO;
            _problemHandlerLookuper = problemHandlerLookuper;
        }

        public Problem Register(string recordType, string recordId, string recordUserId, string datamap, int? createdBy, string stackTrace,
            string message, string handlerName, string assignee = null, int priority = 1, string profiles = null) {
            var problem = new Problem(recordType, recordId, recordUserId, datamap, DateTime.Now,
                createdBy, assignee, priority, stackTrace, message,
                profiles, handlerName, ProblemStatus.Open.ToString());
            var resultProblem = _swdbHibernateDAO.Save(problem);
            var handler = _problemHandlerLookuper.FindHandler(handlerName, recordType);
            if (handler != null) {
                handler.OnProblemRegister(resultProblem);
            }
            _log.WarnFormat("registering new problem {0} for entry {1}:{2}",handlerName,recordType, recordId);
            return resultProblem;
        }

        public Problem RegisterOrUpdateProblem(int? currentUser, Problem problem, Func<string> queryToUse) {
            Problem existingProblem;
            if (queryToUse == null) {
                existingProblem = _swdbHibernateDAO.FindSingleByQuery<Problem>(Problem.ByEntryAndType.Fmt(problem.RecordId, problem.RecordType, problem.ProblemType));
            } else {
                existingProblem = _swdbHibernateDAO.FindSingleByQuery<Problem>(queryToUse());
            }

            if (existingProblem != null) {
                problem.Id = existingProblem.Id;
            }
            problem.CreatedBy = currentUser;
            var resultingProblem = _swdbHibernateDAO.Save(problem);
            var handler = _problemHandlerLookuper.FindHandler(problem.ProblemType, problem.RecordType);
            if (handler != null) {
                handler.OnProblemRegister(resultingProblem);
            }
            _log.WarnFormat("registering new problem {0} for entry {1}:{2}", problem.ProblemType, problem.RecordType, problem.RecordId);
            return resultingProblem;
        }


        public void DeleteProblems(string recordType, string recordId, string problemType) {
            _swdbHibernateDAO.ExecuteSql(
                "delete from PROB_PROBLEM where recordtype = ? and recordid = ? and problemType = ?", recordType, recordId,
                problemType);
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
