using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;

namespace softWrench.sW4.Data.Persistence.Relational {
    internal class EntityPopulator {
        private static String PopulateId(IEnumerable<KeyValuePair<string, object>> row, EntityAttribute idAttribute) {
            return row.Select(r => r.Key == idAttribute.Name).ToString();
        }

        private static IDictionary<string, object> PopulateAttributes(IEnumerable<KeyValuePair<string, object>> dictionary) {
            IDictionary<string, object> attributes = new Dictionary<string, object>();

            foreach (var pair in dictionary) {
                attributes[pair.Key] = pair.Value;
            }

            return attributes;
        }

        public Entity Populate(EntityMetadata entityMetadata, IEnumerable<KeyValuePair<string, object>> row) {
            var rowAsList = row as IList<KeyValuePair<string, object>> ?? row.ToList();

            var id = PopulateId(rowAsList, entityMetadata.Schema.IdAttribute);
            var attributes = PopulateAttributes(rowAsList);

            return new Entity(id, attributes, new Dictionary<string, object>(), entityMetadata);
        }
    }
}