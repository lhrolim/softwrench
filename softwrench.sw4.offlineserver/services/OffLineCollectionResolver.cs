﻿using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Data.Persistence.Relational.Collection;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Entity.Association;

namespace softwrench.sw4.offlineserver.services {
    public class OffLineCollectionResolver : CollectionResolver {
        //TODO: this will work on MSSQL Maximos, but need to review for DB2/ Oracle
        //TODO:(2) this won´t bring compositions whose joined tables were updated, should be a minor bug, since compositions are rarely updated after all.
        private const string BothQueryTemplate = "({0} in ({1}) and Cast({4}.rowstamp AS BIGINT)  > {2}) or ({0} in ({3}))";
        private const string AllNewTemplate = "{0} in ({1})";


        protected override SearchRequestDto BuildSearchRequestDto(InternalCollectionResolverParameter parameter,
          CollectionMatchingResultWrapper matchingResultWrapper) {
            var dto = base.BuildSearchRequestDto(parameter, matchingResultWrapper);

            return dto;
        }

        protected override void BuildParentQueryConstraint(CollectionMatchingResultWrapper matchingResultWrapper,
            InternalCollectionResolverParameter parameter, EntityAssociationAttribute lookupAttribute, SearchRequestDto searchRequestDto,string relationshipName) {
            if (!lookupAttribute.Primary) {
                base.BuildParentQueryConstraint(matchingResultWrapper, parameter, lookupAttribute, searchRequestDto,relationshipName);
                return;
            }


            var offParameter = (OfflineCollectionResolverParameters)parameter.ExternalParameters;

            if (!offParameter.NewEntities.Any()) {
                base.BuildParentQueryConstraint(matchingResultWrapper, parameter, lookupAttribute, searchRequestDto, relationshipName);
                //if no new parent entities were returned, we just need to bring these who have a bigger rowstamp than the client data.
                if (parameter.Rowstamp != null) {
                    searchRequestDto.AppendSearchEntry(RowStampUtil.RowstampColumnName, ">{0}".Fmt(parameter.Rowstamp));
                }
                return;
            }
            var newIdsForQuery = BaseQueryUtil.GenerateInString(offParameter.NewEntities,lookupAttribute.From);
            var columnName = lookupAttribute.To;

            if (!offParameter.ExistingEntities.Any()){
                //first sync scenario
                searchRequestDto.AppendWhereClauseFormat(AllNewTemplate,columnName, newIdsForQuery);
                return;
            }

            
            var rowstamp = parameter.Rowstamp;


            var updateIdsForQuery = BaseQueryUtil.GenerateInString(offParameter.ExistingEntities, lookupAttribute.From);


            searchRequestDto.AppendWhereClauseFormat(BothQueryTemplate, columnName, updateIdsForQuery, rowstamp, newIdsForQuery, relationshipName);
        }

        protected override CollectionMatchingResultWrapper GetResultWrapper() {
            return new OffLineMatchResultWrapper();
        }


        public class OfflineCollectionResolverParameters : CollectionResolverParameters {
            public OfflineCollectionResolverParameters(ApplicationMetadata applicationMetadata, IEnumerable<DataMap> parentEntities, IDictionary<string, long?> rowstampMap, IEnumerable<DataMap> newEntities, IEnumerable<DataMap> alreadyExisting)
                : base(applicationMetadata, parentEntities, rowstampMap) {
                NewEntities = newEntities;
                ExistingEntities = alreadyExisting;
            }

            public IEnumerable<DataMap> NewEntities { get; set; }
            public IEnumerable<DataMap> ExistingEntities { get; set; }
        }


        protected class OffLineMatchResultWrapper : CollectionMatchingResultWrapper {
            internal CollectionMatchingResultKey FetchKey(AttributeHolder entity) {
                //doesnt matter for offline
                return new CollectionMatchingResultKey();
            }
        }
    }
}
