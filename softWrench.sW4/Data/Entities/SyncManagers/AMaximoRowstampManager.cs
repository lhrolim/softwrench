using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using log4net;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softwrench.sW4.Shared2.Data;

namespace softWrench.sW4.Data.Entities.SyncManagers {
    public abstract class AMaximoRowstampManager {

        protected static readonly ILog Log = LogManager.GetLogger(typeof(AMaximoRowstampManager));

        protected readonly SWDBHibernateDAO DAO;
        protected readonly IConfigurationFacade ConfigFacade;
        public static EntityRepository EntityRepository;


        protected AMaximoRowstampManager(SWDBHibernateDAO dao, IConfigurationFacade facade,EntityRepository repository) {
            DAO = dao;
            ConfigFacade = facade;
            EntityRepository = repository;
        }

        protected async Task<IEnumerable<AttributeHolder>> FetchNew(long rowstamp, string entityName, SearchRequestDto searchDto = null) {
            var entityMetadata = MetadataProvider.Entity(entityName);
            var result = await EntityRepository.Get(entityMetadata, rowstamp, searchDto);
            return result;
        }


        protected async Task SetRowstampIfBigger(String key, long? rowstamp, long? oldRowstamp) {
            if (rowstamp == null || (oldRowstamp!=null && rowstamp < oldRowstamp)) {
                return;
            }
            await ConfigFacade.SetValue(key, rowstamp);
        }



        [CanBeNull]
        protected long? GetLastRowstamp(IEnumerable<AttributeHolder> attributeHolders,String[] rowstampFields) {
            
            var enumerable = attributeHolders as AttributeHolder[] ?? attributeHolders.ToArray();
            if (!enumerable.Any()) {
                return null;
            }
            var maxArray = new List<Int64>();
            foreach (var rowstampField in rowstampFields){
                var maxRowstampSt =enumerable.Max(x => x.GetAttribute(rowstampField));
                maxArray.Add(Convert.ToInt64(maxRowstampSt));
            }
            var maxValue =maxArray.Max();
            return maxValue == 0 ? (long?)null : maxArray.Max();
            
        }
    }
}
