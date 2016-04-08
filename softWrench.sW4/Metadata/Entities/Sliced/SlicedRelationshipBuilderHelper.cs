using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using log4net;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Entities.Sliced {
    class SlicedRelationshipBuilderHelper {

        private static readonly ILog Log = LogManager.GetLogger(typeof(SlicedRelationshipBuilderHelper));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relationshipAttributes">fields that are declared on the application schema containing '.' </param>
        /// <param name="entityMetadata"></param>
        /// <returns></returns>
        public static RelationshipResult HandleRelationshipFields(IEnumerable<String> relationshipAttributes,
            EntityMetadata entityMetadata) {
            var result = new RelationshipResult();
            var attributes = relationshipAttributes as string[] ?? relationshipAttributes.ToArray();
            if (!attributes.Any()) {
                return result;
            }

            var innerFields = new Dictionary<string, HashSet<string>>();
            var usedRelationships = new HashSet<SlicedEntityAssociation>();
            foreach (var attribute in attributes) {
                var indexOf = attribute.IndexOf(".", System.StringComparison.InvariantCulture);
                var qualifier = attribute.Substring(0, indexOf);
                qualifier = qualifier.EndsWith("_") ? qualifier : qualifier + "_";
                //                if (qualifier.LastIndexOf("_", StringComparison.Ordinal) !=
                //                    qualifier.IndexOf("_", StringComparison.Ordinal)) {
                //if we have more then 1 _ it means that this is non a direct relationship...
                //so we need to add Inner Sliced Data to perform a nested join, besisdes of the "used relationship"
                // ex: location_shipto_.address1 or location_billto_.address2
                PopulateInnerFieldsEntry(entityMetadata, attribute, usedRelationships, innerFields);
                //                    result.InnerEntityMetadatas.Add(BuildInnerSliced());
                //                } else {
                //                    //otherwise, just add as a simple relationship
                //                    //ex: asset_.description on application sr
                //                    var sliced =SlicedEntityAssociation.GetInstance(entityMetadata.Associations.First(a => a.Qualifier == qualifier));
                //                    usedRelationships.Add(sliced);
                //                }
            }
            foreach (var innerFieldEntry in innerFields) {
                result.InnerEntityMetadatas.Add(BuildInnerSliced(entityMetadata, innerFieldEntry));
            }

            result.DirectRelationships = usedRelationships;
            return result;
        }

        private static void PopulateInnerFieldsEntry(EntityMetadata entityMetadata, string attribute,
            ICollection<SlicedEntityAssociation> usedRelationships, Dictionary<string, HashSet<string>> innerFields) {
            var firstIdx = attribute.IndexOf("_", StringComparison.Ordinal);
            if (firstIdx == -1) {
                //if the user forgets to declare the _
                firstIdx = attribute.IndexOf(".", StringComparison.Ordinal);
            }

            var firstAttribute = attribute.Substring(0, firstIdx) + "_";
            if (Char.IsNumber(firstAttribute[0])) {
                firstAttribute = firstAttribute.Substring(1);
            }
            var seccondAttribute = attribute.Substring(firstIdx + 1);
            try {
                var relatedEntityAssociation = entityMetadata.Associations.FirstOrDefault(a => a.Qualifier == firstAttribute);
                if (relatedEntityAssociation == null) {
                    throw new InvalidOperationException("missing relationship {0} of entity {1}".Fmt(firstAttribute, entityMetadata.Name));
                }


                if (!innerFields.ContainsKey(firstAttribute)) {
                    innerFields.Add(firstAttribute, new HashSet<string>());
                }

                var parsedSecondAttribute = seccondAttribute;
                if (seccondAttribute.StartsWith(".")) {
                    //remove leading .
                    parsedSecondAttribute = seccondAttribute.Substring(1);

                }
                innerFields[firstAttribute].Add(parsedSecondAttribute);
                var slicedEntityAssociation = new SlicedEntityAssociation(relatedEntityAssociation, new HashSet<string>() { parsedSecondAttribute }, relatedEntityAssociation.Qualifier);
                if (usedRelationships.Contains(slicedEntityAssociation)) {
                    var existingEntry = usedRelationships.First(f => Equals(f, slicedEntityAssociation));
                    existingEntry.SlicedAttributes.AddAll(slicedEntityAssociation.SlicedAttributes);
                } else {
                    usedRelationships.Add(slicedEntityAssociation);
                }


            } catch (Exception e) {
                Log.Error(firstAttribute + " not found on associations of " + entityMetadata, e);
                throw;
            }

        }

        private static SlicedEntityMetadata BuildInnerSliced(EntityMetadata entityMetadata, KeyValuePair<string, HashSet<string>> innerFieldEntry) {
            var relatedEntityAssociation = entityMetadata.Associations.First(a => a.Qualifier == innerFieldEntry.Key);
            var relatedEntity = MetadataProvider.Entity(relatedEntityAssociation.To);
            var innerInstance = SlicedEntityMetadataBuilder.GetInnerInstance(relatedEntity, innerFieldEntry.Value);
            innerInstance.ContextAlias = innerFieldEntry.Key;
            return innerInstance;
        }

        internal class RelationshipResult {
            internal ISet<SlicedEntityAssociation> DirectRelationships = new HashSet<SlicedEntityAssociation>();
            internal IList<SlicedEntityMetadata> InnerEntityMetadatas = new List<SlicedEntityMetadata>();
        }


    }
}
