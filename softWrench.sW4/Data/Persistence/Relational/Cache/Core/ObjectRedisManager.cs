using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.Relational.Cache.Api;

namespace softWrench.sW4.Data.Persistence.Relational.Cache.Core {

    public class ObjectRedisManager : BaseRedisManager, IObjectRedisManager {

        public ObjectRedisManager(IConfigurationFacade configFacade) : base(configFacade) {
        }

        public async Task<T> LookupAsync<T>(string key) {
            if (!IsAvailable()) {
                return default(T);
            }
            return await CacheClient.GetAsync<T>(key);
        }

        public async Task<bool> InsertAsync<T>(BaseRedisInsertKey cacheKey, T value) {
            if (!IsAvailable()) {
                return false;
            }

            if (cacheKey.ReplaceExisting) {
                if (cacheKey.ExpiresIn.HasValue) {
                    return await CacheClient.ReplaceAsync(cacheKey.Key, value, cacheKey.ExpiresIn.Value);
                }
                return await CacheClient.ReplaceAsync(cacheKey.Key, value);
            }

            if (await CacheClient.ExistsAsync(cacheKey.Key)) {
                return false;
            }

            if (cacheKey.ExpiresIn.HasValue) {
                return await CacheClient.AddAsync(cacheKey.Key, value, cacheKey.ExpiresIn.Value);
            }
            return await CacheClient.AddAsync(cacheKey.Key, value);
        }

        public T Lookup<T>(string key) {
            if (!IsAvailable()) {
                return default(T);
            }
            return CacheClient.Get<T>(key);
        }

        public bool Insert<T>(BaseRedisInsertKey cacheKey, T value) {
            if (!IsAvailable()) {
                return false;
            }

            if (cacheKey.ReplaceExisting) {
                if (cacheKey.ExpiresIn.HasValue) {
                    return CacheClient.Replace(cacheKey.Key, value, cacheKey.ExpiresIn.Value);
                }
                return CacheClient.Replace(cacheKey.Key, value);
            }

            if (CacheClient.Exists(cacheKey.Key)) {
                return false;
            }

            if (cacheKey.ExpiresIn.HasValue) {
                return CacheClient.Add(cacheKey.Key, value, cacheKey.ExpiresIn.Value);
            }
            return CacheClient.Add(cacheKey.Key, value);
        }
    }
}
