﻿using System;
using cts.commons.persistence;

namespace softwrench.sw4.problem.classes {
    public class ProblemManager : IProblemManager {

        private readonly ISWDBHibernateDAO _swdbHibernateDAO;
        private readonly ProblemHandlerLookuper _problemHandlerLookuper;

        public ProblemManager(ISWDBHibernateDAO swdbHibernateDAO, ProblemHandlerLookuper problemHandlerLookuper) {
            _swdbHibernateDAO = swdbHibernateDAO;
            _problemHandlerLookuper = problemHandlerLookuper;
        }

        public Problem Register(string recordType, string recordId, string recordUserId, string datamap, int? createdBy, string stackTrace,
            string message, string handlerName = null, string assignee = null, int priority = 1, string profiles = null) {
            var problem = new Problem(recordType, recordId, recordUserId, datamap, DateTime.Now,
                createdBy, assignee, priority, stackTrace, message,
                profiles, handlerName, ProblemStatus.Open.ToString());
            var resultProblem = _swdbHibernateDAO.Save(problem);
            var handler = _problemHandlerLookuper.FindHandler(handlerName, recordType);
            if (handler != null) {
                handler.OnProblemRegister(resultProblem);
            }

            return resultProblem;
        }

        public Problem RegisterOrUpdateProblem(int currentUser, Problem problem, Func<string> queryToUse) {
            var existingProblem = _swdbHibernateDAO.FindSingleByQuery<Problem>(queryToUse());
            Problem resultingProblem = null;
            IProblemHandler handler = null;
            if (existingProblem != null) {
                existingProblem.CreatedBy = currentUser;
                existingProblem.CreatedDate = DateTime.Now;
                resultingProblem = _swdbHibernateDAO.Save(existingProblem);
                handler = _problemHandlerLookuper.FindHandler(problem.ProblemType, problem.RecordType);
                if (handler != null) {
                    handler.OnProblemRegister(resultingProblem);
                }
                return resultingProblem;
            }
            problem.CreatedBy = currentUser;
            resultingProblem  = _swdbHibernateDAO.Save(problem);
            handler = _problemHandlerLookuper.FindHandler(problem.ProblemType, problem.RecordType);
            if (handler != null) {
                handler.OnProblemRegister(resultingProblem);
            }
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
