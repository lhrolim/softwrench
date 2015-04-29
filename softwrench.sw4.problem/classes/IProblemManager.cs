using System;
using System.ComponentModel;
using cts.commons.simpleinjector;

namespace softwrench.sw4.problem.classes {
    public interface IProblemManager : ISingletonComponent
    {
        /// <summary>
        /// Registers a new problem record
        /// </summary>
        /// <param name="recordType"></param>
        /// <param name="recordId"></param>
        /// <param name="data"></param>
        /// <param name="createdDate"></param>
        /// <param name="createdBy"></param>
        /// <param name="assignee"></param>
        /// <param name="priority"></param>
        /// <param name="stackTrace"></param>
        /// <param name="description"></param>
        /// <param name="profiles"></param>
        /// <param name="problemHandler"></param>
        /// <param name="status"></param>
        Problem Register(string recordType, string recordId, string data, 
            DateTime createdDate, string createdBy, string assignee, 
            int priority, string stackTrace, string description, 
            string profiles, string problemHandler, string status);


        void List();


        void ListAssigned();


        void Submit();

        /// <summary>
        /// Updates a problem to a resolved state
        /// </summary>
        void Resolve();


        void Detail();
    }
}
