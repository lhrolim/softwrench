using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Data.Persistence.Relational.Cache.Api {

    public interface IObjectRedisManager : IBaseRedisManager, ISingletonComponent {

        Task<T> LookupAsync<T>(string key);

        Task<bool> InsertAsync<T>(BaseRedisInsertKey cacheKey, T value);


        T Lookup<T>(string key);

        bool Insert<T>(BaseRedisInsertKey cacheKey, T value);

    }
}
