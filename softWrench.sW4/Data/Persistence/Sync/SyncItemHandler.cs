using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.SimpleInjector;

namespace softWrench.sW4.Data.Persistence.Sync {
    public class SyncItemHandler : ISingletonComponent {

        private EntityRepository _repository;
        private MaximoHibernateDAO _dao;

        public SyncItemHandler(EntityRepository repository, MaximoHibernateDAO dao) {
            _repository = repository;
            _dao = dao;
        }



        /// <summary>
        /// This delegate may be used to perform extra handling to a synchronized item
        /// </summary>
        /// <param name="item"></param>
        public delegate void SyncedItemHandlerDelegate(KeyValuePair<object, DataMap> item, ApplicationMetadata metadata);


        public SynchronizationApplicationData Sync(ApplicationMetadata appMetadata, SynchronizationRequestDto.ApplicationSyncData applicationSyncData,
            SyncedItemHandlerDelegate syncItemHandlerDelegate) {
            var entityMetadata = MetadataProvider.SlicedEntityMetadata(appMetadata);
            var rowstamps = new Rowstamps(applicationSyncData.LowerLimitRowstamp, applicationSyncData.UpperLimitRowstamp);
            var deletedRecordsId = DeletedRecordsId(rowstamps, entityMetadata);
            IList<DataMap> dataMaps = new List<DataMap>();
            var rowStamps = new SortedSet<long>();
            var enumerable = _repository.GetSynchronizationData(entityMetadata, rowstamps);
            if (!enumerable.Any()) {
                return SynchronizationApplicationData.NoRecords(appMetadata);
            }
            var ids = new HashSet<object>();
            //            IDictionary<object, DataMap> entitiesDictionary = new Dictionary<object, DataMap>();
            foreach (var row in enumerable) {
                var dataMap = DataMap.Populate(appMetadata, row);
                dataMaps.Add(dataMap);
                if (dataMap.Approwstamp != null) {
                    rowStamps.Add((long)dataMap.Approwstamp);
                }
                var id = dataMap.Fields[appMetadata.IdFieldName];
                ids.Add(id);
                var result = new KeyValuePair<object, DataMap>(id, dataMap);
                if (syncItemHandlerDelegate != null) {
                    syncItemHandlerDelegate(result, appMetadata);
                }
                //                entitiesDictionary.Add(result);
            }
            //not yet implemented
            //            HandleRelationships(appMetadata, entitiesDictionary);
            var resultingRowstamps = new Rowstamps(rowStamps.First().ToString(CultureInfo.InvariantCulture), rowStamps.Last().ToString(CultureInfo.InvariantCulture));
            var synchronizationData = new SynchronizationApplicationData(appMetadata, dataMaps, entityMetadata.ListAssociations(), resultingRowstamps);
            synchronizationData.DeletedRecordIds = deletedRecordsId;
            return synchronizationData;
        }

        private IList<object> DeletedRecordsId(Rowstamps rowstamps, EntityMetadata entityMetadata) {
            if (rowstamps.NotBound() || rowstamps.BothLimits()) {
                return new List<object>();
            }
            var deletedRecordsIdQuery = RowStampUtil.DeletedRecordsIdQuery(entityMetadata);
            IList<object> deletedRecordsId = new List<object>();
            if (deletedRecordsIdQuery != null) {
                deletedRecordsId = _dao.FindByNativeQuery(deletedRecordsIdQuery, rowstamps.Lowerlimit);
            }
            return deletedRecordsId;
        }

    }
}
