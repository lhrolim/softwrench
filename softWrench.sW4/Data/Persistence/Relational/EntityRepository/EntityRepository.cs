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
using cts.commons.simpleinjector;
using Quartz.Util;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Relational.EntityRepository {
    public class EntityRepository : ISingletonComponent {

        private static readonly ILog Log = LogManager.GetLogger(typeof(EntityRepository));

        private readonly SWDBHibernateDAO _swdbDao;
        private readonly MaximoHibernateDAO _maximoHibernateDao;
        private readonly EntityQueryBuilder _entityQueryBuilder; 

        public EntityRepository(SWDBHibernateDAO swdbDao, MaximoHibernateDAO maximoHibernateDao) {
            _swdbDao = swdbDao;
            _maximoHibernateDao = maximoHibernateDao;
            _entityQueryBuilder = new EntityQueryBuilder();
        }


        private static string InitializeConnectionString() {
            return ApplicationConfiguration.DBConnectionString(DBType.Maximo);
        }

        public IReadOnlyList<AttributeHolder> Get([NotNull] EntityMetadata entityMetadata, [NotNull] SearchRequestDto searchDto) {
            if (entityMetadata == null) throw new ArgumentNullException("entityMetadata");
            if (searchDto == null) throw new ArgumentNullException("searchDto");
            var query = _entityQueryBuilder.AllRows(entityMetadata, searchDto);
            var rows = Query(entityMetadata, query, searchDto);
            return rows.Cast<IEnumerable<KeyValuePair<string, object>>>()
               .Select(r => BuildDataMap(entityMetadata, r))
               .ToList();

        }

        private DataMap BuildDataMap(EntityMetadata entityMetadata, IEnumerable<KeyValuePair<string, object>> r) {


            return new DataMap(entityMetadata.Name, r.ToDictionary(pair => FixKey(pair.Key, entityMetadata), pair => HandleValue(pair.Key, entityMetadata, pair.Value), StringComparer.OrdinalIgnoreCase), entityMetadata.Schema.MappingType);
        }

        private object HandleValue(string key, EntityMetadata entityMetadata, object value) {
            if (!(value is DateTime)) {
                return value;
            }
            var attributeDeclaration = entityMetadata.Schema.Attributes.FirstOrDefault(f => f.Name.EqualsIc(key));
            if (attributeDeclaration == null) {
                return value;
            }
            if (attributeDeclaration.ConnectorParameters.Parameters.ContainsKey("utcdate")) {
                var date = (DateTime)value;
                date=DateTime.SpecifyKind(date, DateTimeKind.Utc);
                return date;
            }
            return value;
        }

        public IEnumerable<dynamic> RawGet([NotNull] EntityMetadata entityMetadata, [NotNull] SearchRequestDto searchDto) {
            if (entityMetadata == null) throw new ArgumentNullException("entityMetadata");
            if (searchDto == null) throw new ArgumentNullException("searchDto");
            var query = _entityQueryBuilder.AllRows(entityMetadata, searchDto);
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
            var query = _entityQueryBuilder.AllRows(entityMetadata, searchDto);
            var rows = Query(entityMetadata, query, rowstamp, searchDto);
            return rows.Cast<IEnumerable<KeyValuePair<string, object>>>()
               .Select(r => BuildDataMap(entityMetadata, r))
               .ToList();

        }


        public class SearchEntityResult {
            public IList<Dictionary<string, object>> ResultList;
            public long? MaxRowstampReturned;
            public string IdFieldName;
            /// <summary>
            /// Holds the PaginationData for the result
            /// </summary>
            [CanBeNull]
            public PaginatedSearchRequestDto PaginationData {
                get; set;
            }
        }



        //needed to avoid "Fields" nesting in collectionData
        public SearchEntityResult GetAsRawDictionary([NotNull] EntityMetadata entityMetadata, [NotNull] SearchRequestDto searchDto, Boolean fetchMaxRowstamp = false) {
            if (entityMetadata == null) throw new ArgumentNullException("entityMetadata");
            if (searchDto == null) throw new ArgumentNullException("searchDto");
            var query = _entityQueryBuilder.AllRows(entityMetadata, searchDto);
            var rows = Query(entityMetadata, query, searchDto);
            var enumerable = rows as dynamic[] ?? rows.ToArray();
            Log.DebugFormat("returning {0} rows", enumerable.Count());
            long? maxRowstamp = 0;

            IList<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

            foreach (var row in enumerable) {
                var dict = (IDictionary<string, object>)row;
                var item = new Dictionary<string, object>();
                if (fetchMaxRowstamp) {

                    if (dict.ContainsKey(RowStampUtil.RowstampColumnName)) {
                        var rowstamp = RowStampUtil.Convert(dict[RowStampUtil.RowstampColumnName]);
                        if (rowstamp > maxRowstamp) {
                            maxRowstamp = rowstamp;
                        }
                    }
                }

                foreach (var column in dict) {
                    item[FixKey(column.Key, entityMetadata)] = column.Value;
                }
                list.Add(item);
            }

            return new SearchEntityResult {
                MaxRowstampReturned = maxRowstamp == 0 ? null : maxRowstamp,
                ResultList = list,
                IdFieldName = entityMetadata.IdFieldName
            };



        }


        private IEnumerable<dynamic> Query(EntityMetadata entityMetadata, BindedEntityQuery query, SearchRequestDto searchDTO) {
            //TODO: hack to avoid garbage data and limit size of list queries.
            var paginationData = PaginationData.GetInstance(searchDTO, entityMetadata);
            var rows = GetDao(entityMetadata).FindByNativeQuery(query.Sql, query.Parameters, paginationData, searchDTO.QueryAlias);
            return rows;
        }

        private IEnumerable<dynamic> Query(EntityMetadata entityMetadata, BindedEntityQuery query, long rowstamp, SearchRequestDto searchDto) {
            var sqlAux = query.Sql.Replace("1=1", RowStampUtil.RowstampWhereCondition(entityMetadata, rowstamp, searchDto));
            var rows = GetDao(entityMetadata).FindByNativeQuery(sqlAux, query.Parameters, null, searchDto.QueryAlias);
            return rows;
        }

        //_ used in db2
        private string FixKey(string key, EntityMetadata entityMetadata) {
            if (entityMetadata.Attributes(EntityMetadata.AttributesMode.NoCollections).Any(a => a.Name.EqualsIc(key))) {
                //IF the entity has a non collection attribute declared containing _, let´s keep it, cause it could be the name of the column actually
                return key.ToLower();
            }

            // TODO: This needs to be revisited when we integrate any DB2 customer. 
            // TODO: Not working for current customer because they could have attributes that are with underscore like feature_request - KSW-104
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

            var query = _entityQueryBuilder.ById(entityMetadata, id);


            var rows = Query(entityMetadata, query, new SearchRequestDto());
            return rows.Cast<IEnumerable<KeyValuePair<string, object>>>()
              .Select(r => BuildDataMap(entityMetadata, r))
              .ToList().FirstOrDefault();
        }

        [CanBeNull]
        public AttributeHolder ByUserIdSite([NotNull] EntityMetadata entityMetadata, [NotNull]Tuple<string,string>userIdSiteTuple ) {
            //TODO: we're always handling the entity ID as a string.
            //Maybe we should leverage the entity attribute type.
            if (entityMetadata == null) throw new ArgumentNullException("entityMetadata");
            if (userIdSiteTuple == null) throw new ArgumentNullException("userIdSiteTuple");
            var query = _entityQueryBuilder.ByUserIdSite(entityMetadata, userIdSiteTuple);


            var rows = Query(entityMetadata, query, new SearchRequestDto());
            return rows.Cast<IEnumerable<KeyValuePair<string, object>>>()
              .Select(r => BuildDataMap(entityMetadata, r))
              .ToList().FirstOrDefault();
        }

        public IList<IEnumerable<KeyValuePair<string, object>>> GetSynchronizationData(SlicedEntityMetadata entityMetadata, Rowstamps rowstamps) {

            var query = _entityQueryBuilder.AllRowsForSync(entityMetadata, rowstamps);
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
            var query = _entityQueryBuilder.CountRows(entityMetadata, searchDto);

            return GetDao(entityMetadata).CountByNativeQuery(query.Sql, query.Parameters, searchDto.QueryAlias);

        }

        private BaseHibernateDAO GetDao(EntityMetadata metadata) {
            if (metadata.Name.EndsWith("_")) {
                return _swdbDao;
            }
            return _maximoHibernateDao;
        }

        public static int GetNextEntityId([NotNull] EntityMetadata entityMetadata) {
            if (entityMetadata == null) throw new ArgumentNullException("entityMetadata");
            var query = "Select MAX({0}) from {1}".FormatInvariant(entityMetadata.IdFieldName, entityMetadata.GetTableName());
            var id = MaximoHibernateDAO.GetInstance().FindSingleByNativeQuery<object>(query, null);
            var rnd = new Random();
            // To avoid concurrency issues with maximo we will use a range of 5-15 higher than the max.
            // Using the range also lowers the chance of two users in SW submitting new records at the same time and causing a similar issue.
            var newId = Convert.ToInt32(id) + rnd.Next(5, 15);
            return newId;
        }

    }
}
