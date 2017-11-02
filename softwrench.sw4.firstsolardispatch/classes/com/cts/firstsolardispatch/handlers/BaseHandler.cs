using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using softWrench.sW4.Data.Persistence.Operation;

namespace softwrench.sw4.firstsolardispatch.classes.com.cts.firstsolardispatch.handlers {
    public abstract class BaseHandler<T>: ISingletonComponent where T: IBaseEntity {

        protected abstract T NewInstance();

        protected T GetOrCreate(CrudOperationData crudoperationData, IList<T> existing) {
            var id = crudoperationData.GetIntAttribute("id");
            var newInstance = NewInstance();
            if (id == null || existing == null) {
                return newInstance;
            }
            var found = existing.FirstOrDefault(e => e.Id == id);
            if (found == null) {
                newInstance.Id = id;
                return newInstance;
            }
            existing.Remove(found);
            return found;
        }
    }
}
