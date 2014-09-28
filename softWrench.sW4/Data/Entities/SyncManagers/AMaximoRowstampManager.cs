using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using log4net;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.SimpleInjector;

namespace softWrench.sW4.Data.Entities.SyncManagers {
    public abstract class AMaximoRowstampManager {

        protected static readonly ILog Log = LogManager.GetLogger(typeof(AMaximoRowstampManager));

        protected readonly SWDBHibernateDAO DAO;
        protected readonly IConfigurationFacade ConfigFacade;
        public readonly EntityRepository EntityRepository;


        protected AMaximoRowstampManager(SWDBHibernateDAO dao, IConfigurationFacade facade) {
            DAO = dao;
            ConfigFacade = facade;
            EntityRepository = new EntityRepository();
        }

        protected IEnumerable<AttributeHolder> FetchNew(long rowstamp, string entityName, SearchRequestDto searchDto = null) {
            var entityMetadata = MetadataProvider.Entity(entityName);
            var result = EntityRepository.Get(entityMetadata, rowstamp, searchDto);
            return result;
        }


        protected void SetRowstampIfBigger(String key, long? rowstamp, long? oldRowstamp) {
            if (rowstamp == null || (oldRowstamp!=null && rowstamp < oldRowstamp)) {
                return;
            }
            ConfigFacade.SetValue(key, rowstamp);
        }



        [CanBeNull]
        protected long? GetLastRowstamp(IEnumerable<AttributeHolder> attributeHolders) {
            Boolean hasRowstamp1 = false;
            var enumerable = attributeHolders as AttributeHolder[] ?? attributeHolders.ToArray();
            if (!enumerable.Any()) {
                return null;
            }
            var firstElement = enumerable.First();
            if (firstElement.Attributes.ContainsKey("rowstamp1")) {
                hasRowstamp1 = true;
            }
            var lastRowstampString = enumerable.Max(x => x.GetAttribute("rowstamp"));
            var lastRowstamp = Convert.ToInt64(lastRowstampString);
            if (hasRowstamp1) {
                var lastRowtampString1 = enumerable.Max(x => x.GetAttribute("rowstamp1"));
                var lastRowstamp1 = Convert.ToInt64(lastRowtampString1);
                if (lastRowstamp == 0 && lastRowstamp1 == 0) {
                    return null;
                }
                return Math.Max(lastRowstamp, lastRowstamp1);
            }
            return (lastRowstamp == 0 ? (long?)null : lastRowstamp);
        }
    }
}
