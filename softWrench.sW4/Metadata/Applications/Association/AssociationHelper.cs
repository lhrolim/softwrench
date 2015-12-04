using Iesi.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Util;
using System.Collections.Generic;
using JetBrains.Annotations;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;

namespace softWrench.sW4.Metadata.Applications.Association {
    public class AssociationHelper {
        internal const string None = "#none";
        internal const string All = "#all";
        //all associations should be resolved except for the ones marked on the schema property
        internal const string AllButSchema = "#allbutschema";

        public static AssociationHelperResult BuildAssociationsToPrefetch(IAssociationPrefetcherRequest request, ApplicationSchemaDefinition schema) {
            var schemaAssociations = schema.GetProperty(ApplicationSchemaPropertiesCatalog.PreFetchAssociations);
            var requestToFetch = request.AssociationsToFetch;
            if (request is ListOptionsPrefetchRequest) {
                // if we´re on list schema lets simply prefetch all the associations of it
                requestToFetch= "#all";
            }

            var result = new AssociationHelperResult();
            var toFetch = new HashedSet<string>();
            var toAvoid = new HashedSet<string>();

            result.ToFetch = toFetch;
            result.ToAvoid = toAvoid;

            if (schemaAssociations == null && requestToFetch == null) {
                toFetch.AddReturn(None);
                return result;
            }
            if (All.Equals(schemaAssociations)) {
                toFetch.Add(None.Equals(requestToFetch) ? None : All);
                return result;
            }
            if (All.Equals(requestToFetch)) {
                toFetch.AddReturn(All);
                return result;
            }

            if (schemaAssociations != null) {
                toFetch.AddAll(schemaAssociations.Split(','));
                if (AllButSchema.Equals(requestToFetch)) {
                    toAvoid.AddAll(schemaAssociations.Split(','));
                }
            }

            if (requestToFetch != null) {
                toFetch.AddAll(requestToFetch.Split(','));
            }

            return result;
        }

        /// <summary>
        /// Returns a precompiled attribute.Query. 
        /// Since the attribute.Query can lead to a service invocation or runtime string processing this helper takes care of that.
        /// The query will be precompiled against the entityName (i.e. '!@' -> 'entityName'.)
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static string PrecompiledAssociationAttributeQuery([NotNull]string entityName, [NotNull]EntityAssociationAttribute attribute) {
            if (attribute.Query == null) return null;
            var query = attribute.GetQueryReplacingMarkers(entityName);
            if (query.StartsWith("@")) {
                query = BaseQueryBuilder.GetServiceQuery(query);
            }
            return query;
        }

        public class AssociationHelperResult {
            public Set<string> ToFetch { get; set; }
            public Set<string> ToAvoid { get; set; }

            public List<string> ToFetchList { get { return new List<string>(ToFetch); } }
            public List<string> ToAvoidList { get { return new List<string>(ToAvoid); } }

            public bool IsAll { get { return ToFetch.Contains(All) || ToFetch.Contains(AllButSchema); } }
            public bool IsNone { get { return ToFetch.Contains(None); } }

            public bool ShouldResolve(string associationKey) {
                return ((ToFetch.Contains(associationKey) || IsAll) && !ToAvoid.Contains(associationKey));
            }
        }

    }
}
