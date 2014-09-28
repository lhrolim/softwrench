using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.SimpleInjector.Core.Order;

namespace softWrench.sW4.Data.Entities.SyncManagers {
    public interface IUserSyncManager :IOrdered,IComponent{
        void Sync();
    }
}
