using System.Collections.Generic;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using softWrench.sW4.Data.Persistence.Relational.Cache.Core;

namespace softWrench.sW4.Data.Persistence.Relational.Cache.Api {
    public interface IRedisManager : ISingletonComponent {

        bool IsAvailable();

        Task<RedisLookupResult<T>> Lookup<T>(RedisLookupDTO lookupDTO) where T : DataMap;

        Task<long> InsertIntoCache<T>(RedisLookupDTO lookupDTO, RedisInputDTO<T> redisInput) where T : DataMap;

        Task<List<RedisChunkMetadataDescriptor>> GetDescriptors(RedisLookupDTO lookupDTO);
    }
}