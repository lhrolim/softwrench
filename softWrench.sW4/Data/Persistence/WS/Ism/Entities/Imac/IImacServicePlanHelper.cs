using System.Collections.Generic;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Data.Persistence.WS.Ism.Entities.Imac {
    public interface IImacServicePlanHelper : ISingletonComponent {
        IEnumerable<Activity> LoadFromServicePlan(string schemaid, CrudOperationData jsonObject);
    }
}