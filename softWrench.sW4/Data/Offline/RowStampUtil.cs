using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.persistence;
using cts.commons.portable.Util;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Connectors;
using softWrench.sW4.Metadata.Entities.Schema;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Offline {
    public class RowStampUtil {

        public const string RowstampColumnName = "rowstamp";

        private static readonly string MSSQLRowstampQuery = "Cast ({0}.{1} as Bigint) > {2}";
        private static readonly string RowstampQuery = "{0}.{1} > {2}";


        public static long? Convert(object dbstamp) {
            if (ApplicationConfiguration.IsMSSQL(DBType.Maximo)) {

                return ConvertByteArrayToLong(dbstamp);
            } else if (ApplicationConfiguration.IsDB2(DBType.Maximo)) {
                return System.Convert.ToInt64(dbstamp);
            }
            return 1;
            //throw new NotImplementedException("not implemented for oracle database yet");
        }

        private static long? ConvertByteArrayToLong(object dbstamp) {
            // cloning so it doesn't alter parameter
            // this was necessary because it is being called multiple times on the same dbstamp
            // TODO: investigate how it is being called multiple times on same dbstamp (and possibly remove the clone)
            var array = ((byte[])dbstamp);
            var result = new byte[array.Length];
            Array.Copy(array, result, array.Length);
            // Array.Copy is way faster than Array.Clone (fastest method besides buffer copy)

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(result);
            }
            if (!result.Any()) {
                return null;
            }
            var convert = BitConverter.ToInt64(result, 0);
            return convert;
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
            if (!firstAttributeHolder.ContainsKey(RowstampColumnName)) {
                firstAttributeHolder[RowstampColumnName] =
                    listOfCollections.MaxRowstampReturned;
            } else {
                if (listOfCollections.MaxRowstampReturned >
                    (long?)firstAttributeHolder.GetAttribute(RowstampColumnName)) {
                    firstAttributeHolder[RowstampColumnName] = listOfCollections.MaxRowstampReturned;
                }
            }
        }

        public static string RowstampWhereCondition(EntityMetadata entityMetadata, long rowstamp, SearchRequestDto searchDto) {
            var extraRowstamps = entityMetadata.Schema.Attributes.Where(s => s.Name.StartsWith("rowstamp") && !s.Name.Equals("rowstamp"));
            var patternToUse = ApplicationConfiguration.IsMSSQL(DBType.Maximo) ? MSSQLRowstampQuery : RowstampQuery;

            var sb = new StringBuilder(patternToUse.Fmt(entityMetadata.Name, "rowstamp", rowstamp));
            foreach (var extraRowstamp in extraRowstamps) {
                sb.Append(" or ").Append(patternToUse.Fmt(entityMetadata.Name, extraRowstamp.Name, rowstamp));
            }
            foreach (var association in entityMetadata.Associations) {
                if (searchDto == null || searchDto.ProjectionFields == null || HasProjection(searchDto.ProjectionFields, association)) {
                    sb.Append(" or ").Append(patternToUse.Fmt(association.Qualifier, "rowstamp", rowstamp));
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
            return "bigint";
            //throw new NotImplementedException("not implemented for oracle database yet");
        }

        /// <summary>
        /// [0 0 0 0 2 16 -10 -66]
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long? FromStringRepresentation(string value) {
            var bytes = new byte[8];
            var byteChunks = value.Substring(1, value.Length - 2).Split(' ');
            for (int index = 0; index < byteChunks.Length; index++) {
                var byteChunk = byteChunks[index];
                bytes[index] = (byte)Int32.Parse(byteChunk);
            }
            return ConvertByteArrayToLong(bytes);
        }
    }
}
