using System;
using cts.commons.simpleinjector;
using Newtonsoft.Json.Linq;
using softwrench.sw4.api.classes.application;

namespace softwrench.sw4.batch.api.services {
    public interface IBatchSubmissionConverter<T,R> : IApplicationFiltereable,IComponent {

        JArray BreakIntoRows(JObject mainDatamap);

        bool ShouldSubmit(JObject row);

        /// <summary>
        /// Temporaraly returning Object, but it should be OperationWrapper instead, but that would cause an unresolved circular reference.
        /// 
        /// Need more modularization, and implementations need to make sure to return a CrudOperationData as for now
        /// </summary>
        /// <param name="row"></param>
        /// <param name="applicationMetadata">same problem, will be of type ApplicationMetadata</param>
        /// <returns></returns>
        R Convert(JObject row, T applicationMetadata);
      
    }
}