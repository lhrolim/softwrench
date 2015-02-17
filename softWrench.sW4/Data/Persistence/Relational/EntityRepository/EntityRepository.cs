using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using log4net;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Sliced;
using softwrench.sW4.Shared2.Data;
using softwrench.sw4.Shared2.Util;
using cts.commons.simpleinjector;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Relational.EntityRepository {
    public class EntityRepository : ISingletonComponent {

        private static readonly ILog Log = LogManager.GetLogger(typeof(EntityRepository));

        private readonly SWDBHibernateDAO _swdbDao;
        private readonly MaximoHibernateDAO _maximoHibernateDao;

        public EntityRepository(SWDBHibernateDAO swdbDao, MaximoHibernateDAO maximoHibernateDao) {
            _swdbDao = swdbDao;
            _maximoHibernateDao = maximoHibernateDao;
        }


        private static string InitializeConnectionString() {
            return ApplicationConfiguration.DBConnectionString(DBType.Maximo);
        }

        public IReadOnlyList<AttributeHolder> Get([NotNull] EntityMetadata entityMetadata, [NotNull] SearchRequestDto searchDto) {
            if (entityMetadata == null) throw new ArgumentNullException("entityMetadata");
            if (searchDto == null) throw new ArgumentNullException("searchDto");
            var query = new EntityQueryBuilder().AllRows(entityMetadata, searchDto);
            var rows = Query(entityMetadata, query, searchDto);
            return rows.Cast<IEnumerable<KeyValuePair<string, object>>>()
               .Select(r => BuildDataMap(entityMetadata, r))
               .ToList();

        }

        private DataMap BuildDataMap(EntityMetadata entityMetadata, IEnumerable<KeyValuePair<string, object>> r) {

            return new DataMap(entityMetadata.Name, r.ToDictionary(pair => FixKey(pair.Key), pair => pair.Value, StringComparer.OrdinalIgnoreCase), entityMetadata.Schema.MappingType);
        }

        public IEnumerable<dynamic> RawGet([NotNull] EntityMetadata entityMetadata, [NotNull] SearchRequestDto searchDto) {
            if (entityMetadata == null) throw new ArgumentNullException("entityMetadata");
            if (searchDto == null) throw new ArgumentNullException("searchDto");
            var query = new EntityQueryBuilder().AllRows(entityMetadata, searchDto);
            var rows = Query(entityMetadata, query, searchDto);
            return rows;
        }

        public IReadOnlyList<AttributeHolder> Get([NotNull] EntityMetadata entityMetadata, long rowstamp, SearchRequestDto searchDto = null) {
            if (entityMetadata == null) {
                throw new ArgumentNullException("entityMetadata");
            }
            if (searchDto == null) {
                searchDto = new SearchRequestDto();
            }
            var query = new EntityQueryBuilder().AllRows(entityMetadata, searchDto);
            var rows = Query(entityMetadata, query, rowstamp, searchDto);
            return rows.Cast<IEnumerable<KeyValuePair<string, object>>>()
               .Select(r => BuildDataMap(entityMetadata, r))
               .ToList();

        }

        //needed to avoid "Fields" nesting in collectionData
        public IList<Dictionary<string, object>> GetAsRawDictionary([NotNull] EntityMetadata entityMetadata, [NotNull] SearchRequestDto searchDto) {
            if (entityMetadata == null) throw new ArgumentNullException("entityMetadata");
            if (searchDto == null) throw new ArgumentNullException("searchDto");
            var query = new EntityQueryBuilder().AllRows(entityMetadata, searchDto);
            var rows = Query(entityMetadata, query, searchDto);
            Log.DebugFormat("returning {0} rows", rows.Count());
            return rows.Cast<IEnumerable<KeyValuePair<string, object>>>()
               .Select(r => r.ToDictionary(pair => FixKey(pair.Key), pair => pair.Value)).ToList();

        }

        private IEnumerable<dynamic> Query(EntityMetadata entityMetadata, BindedEntityQuery query, SearchRequestDto searchDTO) {
            //TODO: hack to avoid garbage data and limit size of list queries.
            var paginationData = PaginationData.GetInstance(searchDTO, entityMetadata);
            var rows = GetDao(entityMetadata).FindByNativeQuery(query.Sql, query.Parameters, paginationData);
            return rows;
        }

        private IEnumerable<dynamic> Query(EntityMetadata entityMetadata, BindedEntityQuery query, long rowstamp, SearchRequestDto searchDto) {
            var sqlAux = query.Sql.Replace("1=1", RowStampUtil.RowstampWhereCondition(entityMetadata, rowstamp, searchDto));
            var rows = GetDao(entityMetadata).FindByNativeQuery(sqlAux, query.Parameters);
            return rows;
        }

        //_ used in db2
        private string FixKey(string key) {
            if (key.Contains("_") && !key.Contains(".")) {
                if (key.IndexOf("_", System.StringComparison.Ordinal) !=
                    key.LastIndexOf("_", System.StringComparison.Ordinal)) {
                    //more then one _ ==> replace only last
                    return key.ReplaceLastOccurrence("_", "_.").ToLower();
                }

                return key.Replace("_", "_.").ToLower();
            }
            return key.ToLower();
        }



        [CanBeNull]
        public AttributeHolder Get([NotNull] EntityMetadata entityMetadata, [NotNull] string id) {
            //TODO: we're always handling the entity ID as a string.
            //Maybe we should leverage the entity attribute type.
            if (entityMetadata == null) throw new ArgumentNullException("entityMetadata");
            if (id == null) throw new ArgumentNullException("id");

            var query = new EntityQueryBuilder().ById(entityMetadata, id);


            var rows = Query(entityMetadata, query, new SearchRequestDto());
            return rows.Cast<IEnumerable<KeyValuePair<string, object>>>()
              .Select(r => BuildDataMap(entityMetadata, r))
              .ToList().FirstOrDefault();
        }

        public IList<IEnumerable<KeyValuePair<string, object>>> GetSynchronizationData(SlicedEntityMetadata entityMetadata, Rowstamps rowstamps) {

            var query = new EntityQueryBuilder().AllRowsForSync(entityMetadata, rowstamps);
            //TODO: hack to avoid garbage data and limit size of list queries.
            var sql = query.Sql;
            var queryResult = GetDao(entityMetadata).FindByNativeQuery(sql, query.Parameters);
            var rows = queryResult.Cast<IEnumerable<KeyValuePair<string, object>>>();
            return rows as IList<IEnumerable<KeyValuePair<string, object>>> ?? rows.ToList();

        }

        //     

        [NotNull]


        public int Count([NotNull] EntityMetadata entityMetadata, [NotNull] SearchRequestDto searchDto) {
            if (entityMetadata == null) throw new ArgumentNullException("entityMetadata");
            if (searchDto == null) throw new ArgumentNullException("searchDto");
            var query = new EntityQueryBuilder().CountRows(entityMetadata, searchDto);

            return GetDao(entityMetadata).CountByNativeQuery(query.Sql, query.Parameters);

        }

        private BaseHibernateDAO GetDao(EntityMetadata metadata) {
            if (metadata.Name.StartsWith("_")) {
                return _swdbDao;
            }
            return _maximoHibernateDao;
        }


    }
}
