using JetBrains.Annotations;
using log4net;
using softwrench.sw4.Shared2.Util;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Data.Sync;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace softWrench.sW4.Data.Persistence.Relational {
    public sealed class EntityRepository : ISingletonComponent {

        private readonly MaximoHibernateDAO _dao = new MaximoHibernateDAO();

        private static readonly Lazy<string> ConnectionString = new Lazy<string>(InitializeConnectionString);

        private static ILog _log = LogManager.GetLogger(typeof(EntityRepository));

        private static string InitializeConnectionString() {
            return ApplicationConfiguration.DBConnectionString(ApplicationConfiguration.DBType.Maximo);
        }

        public IReadOnlyList<AttributeHolder> Get([NotNull] EntityMetadata entityMetadata, [NotNull] SearchRequestDto searchDto) {
            if (entityMetadata == null) throw new ArgumentNullException("entityMetadata");
            if (searchDto == null) throw new ArgumentNullException("searchDto");
            var query = new EntityQueryBuilder().AllRows(entityMetadata, searchDto);
            var rows = Query(entityMetadata, query, searchDto);
            return rows.Cast<IEnumerable<KeyValuePair<string, object>>>()
               .Select(r => new DataMap(entityMetadata.Name, r.ToDictionary(pair => FixKey(pair.Key), pair => pair.Value, StringComparer.OrdinalIgnoreCase)))
               .ToList();

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
               .Select(r => new DataMap(entityMetadata.Name, r.ToDictionary(pair => FixKey(pair.Key), pair => pair.Value, StringComparer.OrdinalIgnoreCase)))
               .ToList();

        }

        //needed to avoid "Fields" nesting in collectionData
        public IList<Dictionary<string, object>> GetAsRawDictionary([NotNull] EntityMetadata entityMetadata, [NotNull] SearchRequestDto searchDto) {
            if (entityMetadata == null) throw new ArgumentNullException("entityMetadata");
            if (searchDto == null) throw new ArgumentNullException("searchDto");
            var query = new EntityQueryBuilder().AllRows(entityMetadata, searchDto);
            var rows = Query(entityMetadata, query, searchDto);
            _log.DebugFormat("returning {0} rows", rows.Count());
            return rows.Cast<IEnumerable<KeyValuePair<string, object>>>()
               .Select(r => r.ToDictionary(pair => FixKey(pair.Key), pair => pair.Value)).ToList();

        }

        private IEnumerable<dynamic> Query(EntityMetadata entityMetadata, BindedEntityQuery query, SearchRequestDto searchDTO) {
            //TODO: hack to avoid garbage data and limit size of list queries.
            var paginationData = PaginationData.GetInstance(searchDTO, entityMetadata);
            var rows = _dao.FindByNativeQuery(query.Sql, query.Parameters, paginationData);
            return rows;
        }

        private IEnumerable<dynamic> Query(EntityMetadata entityMetadata, BindedEntityQuery query, long rowstamp, SearchRequestDto searchDto) {
            var sqlAux = query.Sql.Replace("1=1", RowStampUtil.RowstampWhereCondition(entityMetadata, rowstamp, searchDto));
            var rows = _dao.FindByNativeQuery(sqlAux, query.Parameters);
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
              .Select(r => new DataMap(entityMetadata.Name, r.ToDictionary(pair => FixKey(pair.Key), pair => pair.Value)))
              .ToList().FirstOrDefault();
        }

        public IList<IEnumerable<KeyValuePair<string, object>>> GetSynchronizationData(SlicedEntityMetadata entityMetadata, Rowstamps rowstamps) {

            var query = new EntityQueryBuilder().AllRowsForSync(entityMetadata, rowstamps);
            //TODO: hack to avoid garbage data and limit size of list queries.
            var sql = query.Sql;
            var queryResult = _dao.FindByNativeQuery(sql, query.Parameters);
            var rows = queryResult.Cast<IEnumerable<KeyValuePair<string, object>>>();
            return rows as IList<IEnumerable<KeyValuePair<string, object>>> ?? rows.ToList();

        }

        public IList<object> FindByQuery(String queryst, params object[] parameters) {
            return _dao.FindByNativeQuery(queryst, parameters);
        }

        public IList<object> FindByQuery(String queryst) {
            return _dao.FindByNativeQuery(queryst);
        }

        [NotNull]


        public int Count([NotNull] EntityMetadata entityMetadata, [NotNull] SearchRequestDto searchDto) {
            if (entityMetadata == null) throw new ArgumentNullException("entityMetadata");
            if (searchDto == null) throw new ArgumentNullException("searchDto");
            var query = new EntityQueryBuilder().CountRows(entityMetadata, searchDto);

            return _dao.CountByNativeQuery(query.Sql, query.Parameters);

        }

        public void Dispose(bool disposing) {
        }

        public void Dispose() {
            Dispose(true);
        }


    }
}
