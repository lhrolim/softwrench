using System.Collections.Generic;
using System.Linq;
using System.Text;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic {
    class QueryFromBuilder {

        public static string Build(EntityMetadata entityMetadata, SearchRequestDto dto = null) {
            var buffer = new StringBuilder();
            buffer.AppendFormat("from {0} ", BaseQueryUtil.AliasEntity(entityMetadata.Name, entityMetadata.Name));
            IEnumerable<EntityAssociation> usedAssociations;
            var fieldsToIncludeInRelationship = new List<string>();
            if (dto != null) {
                fieldsToIncludeInRelationship = dto.GetNestedFieldsToConsiderInRelationships;
            }
            if (fieldsToIncludeInRelationship.Any()) {
                //as we have a filter, let´s give a chance to use collection relationships, as they might be filtered
                usedAssociations = entityMetadata.Associations;
                usedAssociations = FilterByProjectionAndRestrictions(usedAssociations, fieldsToIncludeInRelationship);
            } else {
                usedAssociations = entityMetadata.NonListAssociations().Where(a=> !a.IsTransient);
            }

            foreach (var association in usedAssociations) {
                buffer.Append(QueryJoinBuilder.Build(entityMetadata, association));
            }

            if (dto != null && dto.ExtraLeftJoinSection != null) {
                buffer.Append(dto.ExtraLeftJoinSection);
            }


            return buffer.ToString();
        }

        private static IEnumerable<EntityAssociation> FilterByProjectionAndRestrictions(IEnumerable<EntityAssociation> usedAssociations,
            IEnumerable<string> fieldsToConsider, string prefix = null) {
            ISet<EntityAssociation> associations = new HashSet<EntityAssociation>();
            var entityAssociations = usedAssociations as EntityAssociation[] ?? usedAssociations.ToArray();
            foreach (var @alias in fieldsToConsider) {
                if (!@alias.Contains('.')) {
                    //this won´t impact the relationships, so just continue...
                    continue;
                }

                var relName = EntityUtil.GetRelationshipName(@alias);
                if (relName.IndexOf('_') == relName.LastIndexOf('_')) {
                    var association = entityAssociations.FirstOrDefault(a => a.Qualifier == relName);
                    if (association != null) {
                        associations.Add(association);
                    }
                } else {
                    var clonedRelName = @alias;
                    var firstIdx = clonedRelName.IndexOf('_');
                    var before = EntityUtil.GetRelationshipName(clonedRelName.Substring(0, firstIdx));
                    var after = clonedRelName.Substring(firstIdx + 1);
                    var association = entityAssociations.FirstOrDefault(a => a.Qualifier == before);
                    if (association != null) {
                        var firstEntity = MetadataProvider.Entity(association.To);
                        var innerAssociations = FilterByProjectionAndRestrictions(firstEntity.Associations, new List<string>()
                        {
                            after
                        });
                        associations.Add(association);
                        foreach (var innerAssociation in innerAssociations) {
                            associations.Add(innerAssociation.CloneWithContext(before));
                        }
                    }


                }
            }
            return associations;
        }
    }
}
