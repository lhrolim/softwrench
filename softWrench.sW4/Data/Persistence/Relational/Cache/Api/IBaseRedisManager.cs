
using System.Threading.Tasks;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Data.Persistence.Relational.Cache.Api {

    public interface IBaseRedisManager : IComponent {

        bool IsAvailable();


        
    }
}