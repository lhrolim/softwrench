using System.Collections.Generic;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using softWrench.sW4.Data.Persistence.Relational.Cache.Core;

namespace softWrench.sW4.Data.Persistence.Relational.Cache.Api {
    public interface IDatamapRedisManager : IBaseRedisManager, ISingletonComponent {

        Task<RedisLookupResult<T>> Lookup<T>(RedisLookupDTO lookupDTO) where T : DataMap;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lookupDTO"></param>
        /// <param name="redisInput"></param>
        /// <returns>The maximum rowstamp that was inserted on this operation</returns>
        Task<long> InsertIntoCache<T>(RedisLookupDTO lookupDTO, RedisInputDTO<T> redisInput) where T : DataMap;

        Task<List<RedisChunkMetadataDescriptor>> GetDescriptors(RedisLookupDTO lookupDTO);
    }
}