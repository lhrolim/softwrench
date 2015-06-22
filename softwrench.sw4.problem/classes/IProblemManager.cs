using System;
using System.ComponentModel;
using cts.commons.simpleinjector;

namespace softwrench.sw4.problem.classes {
    public interface IProblemManager : ISingletonComponent {
        /// <summary>
        /// Registers a new problem record
        /// </summary>
        /// <param name="recordType"></param>
        /// <param name="recordId"></param>
        /// <param name="createdBy"></param>
        /// <param name="handler"></param>
        /// <param name="assignee"></param>
        /// <param name="priority"></param>
        /// <param name="datamap"></param>
        /// <param name="stackTrace"></param>
        /// <param name="message"></param>
        /// <param name="profiles"></param>
        Problem Register(string recordType, string recordId, string datamap, int? createdBy, string stackTrace,
            string message,string handler=null, string assignee=null,int priority= 1,string profiles=null);


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
