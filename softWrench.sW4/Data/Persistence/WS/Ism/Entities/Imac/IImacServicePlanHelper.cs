using System.Collections.Generic;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.SimpleInjector;

namespace softWrench.sW4.Data.Persistence.WS.Ism.Entities.Imac {
    public interface IImacServicePlanHelper : ISingletonComponent {
        IEnumerable<Activity> LoadFromServicePlan(string schemaid, CrudOperationData jsonObject);
    }
}