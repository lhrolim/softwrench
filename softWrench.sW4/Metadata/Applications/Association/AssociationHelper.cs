using Iesi.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Association;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;

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
