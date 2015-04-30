﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.persistence;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Connectors;
using softWrench.sW4.Metadata.Entities.Schema;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Offline {

    class RowStampUtil {

        public const string RowstampColumnName = "rowstamp";


        public static long? Convert(object dbstamp) {
            if (ApplicationConfiguration.IsMSSQL(DBType.Maximo)) {
                if (BitConverter.IsLittleEndian)
                    Array.Reverse((Array)dbstamp);
                var value = (byte[])dbstamp;
                if (!value.Any()) {
                    return null;
                }
                return BitConverter.ToInt64(value, 0);
            } else if (ApplicationConfiguration.IsDB2(DBType.Maximo)) {
                return System.Convert.ToInt64(dbstamp);
            }
            throw new NotImplementedException("not implemented for oracle database yet");
        }

        public static IList<object> TryToGetDeletedRecordsId(EntityMetadata entityMetadata, string rowstamp) {
            throw new NotImplementedException();
        }

        public static string DeletedRecordsIdQuery(EntityMetadata entityMetadata) {
            string audittable = null;
            if (!entityMetadata.ConnectorParameters.Parameters.TryGetValue("audittable", out audittable)) {
                return null;
            }
            return String.Format("select {0} from {1} where rowstamp >= @rowstamp", entityMetadata.Schema.IdAttribute.Name, audittable);
        }

        public static void UpdateMaximoRowstamp(AttributeHolder firstAttributeHolder, Persistence.Relational.EntityRepository.EntityRepository.SearchEntityResult listOfCollections) {
            if (!firstAttributeHolder.Attributes.ContainsKey(RowstampColumnName)) {
                firstAttributeHolder.Attributes[RowstampColumnName] =
                    listOfCollections.MaxRowstampReturned;
            } else {
                if (listOfCollections.MaxRowstampReturned >
                    (long?)firstAttributeHolder.GetAttribute(RowstampColumnName)) {
                    firstAttributeHolder.Attributes[RowstampColumnName] = listOfCollections.MaxRowstampReturned;
                }
            }
        }

        public static string RowstampWhereCondition(EntityMetadata entityMetadata, long rowstamp, SearchRequestDto searchDto) {
            var extraRowstamps = entityMetadata.Schema.Attributes.Where(s => s.Name.StartsWith("rowstamp") && !s.Name.Equals("rowstamp"));
            var sb = new StringBuilder(entityMetadata.Name + ".rowstamp > " + rowstamp);
            foreach (var extraRowstamp in extraRowstamps) {
                sb.Append(" or ").Append(entityMetadata.Name).Append("." + extraRowstamp.Name + " > ").Append(rowstamp);
            }
            foreach (var association in entityMetadata.Associations) {
                if (searchDto == null || searchDto.ProjectionFields == null || HasProjection(searchDto.ProjectionFields, association)) {
                    sb.Append(" or ").Append(association.Qualifier).Append(".rowstamp > ").Append(rowstamp);
                }
            }
            return sb.ToString();
        }

        private static bool HasProjection(IEnumerable<ProjectionField> projectionFields, EntityAssociation association) {
            return projectionFields.Any(a => a.Name.StartsWith(association.Qualifier));
        }

        public static EntityAttribute RowstampEntityAttribute() {
            return new EntityAttribute(RowstampColumnName, TimestampAttributeType(), false, true, ConnectorParameters.DefaultInstance(), null);
        }

        private static string TimestampAttributeType() {
            if (ApplicationConfiguration.IsMSSQL(DBType.Maximo)) {
                return "bigint";
            }
            if (ApplicationConfiguration.IsDB2(DBType.Maximo)) {
                return "bigint";
            }
            throw new NotImplementedException("not implemented for oracle database yet");
        }



    }
}
