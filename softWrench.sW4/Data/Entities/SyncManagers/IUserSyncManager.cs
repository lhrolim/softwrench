using System.Threading.Tasks;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Core.Order;

namespace softWrench.sW4.Data.Entities.SyncManagers {
    public interface IUserSyncManager :IOrdered,IComponent{
        Task Sync();
    }
}
