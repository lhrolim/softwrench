using System.Collections.Generic;
using JetBrains.Annotations;
using NHibernate.Linq;
using NHibernate.Util;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.API.Association.SchemaLoading {

    /// <summary>
    /// Result for the main schema composition loading.
    /// 
    /// It could also bring alongside the associations of any eager compositions present on the main schema
    /// 
    /// </summary>
    public class AssociationMainSchemaLoadResult : BaseAssociationSchemaLoadResult<IEnumerable<IAssociationOption>> {

        [CanBeNull]
        public IDictionary<string, CompositionSchemaLoadResult> EagerCompositionAssociations {
            get; set;
        }

        public AssociationMainSchemaLoadResult(IDictionary<string, CompositionSchemaLoadResult> eagerCompositionAssociations = null) {
            EagerCompositionAssociations = eagerCompositionAssociations;
            if (eagerCompositionAssociations != null) {
                eagerCompositionAssociations.ForEach(MergeCompositionAssociationsToRoot);
            }
        }

        private void MergeCompositionAssociationsToRoot(KeyValuePair<string, CompositionSchemaLoadResult> compositionItem) {
            compositionItem.Value.PreFetchLazyOptions.ForEach(MergeAssociation);
            //avoid duplications on the JSON
            compositionItem.Value.PreFetchLazyOptions.Clear();
        }

        private void MergeAssociation(KeyValuePair<string, IDictionary<string, IAssociationOption>> item) {
            var associationKey = item.Key;
            if (!PreFetchLazyOptions.ContainsKey(associationKey)) {
                PreFetchLazyOptions[associationKey] = new Dictionary<string, IAssociationOption>();
            }
            PreFetchLazyOptions[associationKey].AddRange(item.Value);
        }

        public void MergeWithOtherSchemas(List<AssociationMainSchemaLoadResult> innerCompositionsResult) {
            //TODO: this should be replaced when we switch to CompositionSchemaLoadResult
            foreach (var innerResult in innerCompositionsResult) {
                innerResult.PreFetchLazyOptions.ForEach(MergeAssociation);
            }
        }
    }
}
