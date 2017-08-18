using System.Collections.Generic;
using System.Linq;
using System.Text;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.Persistence.Relational.Cache.Api {
    public class RedisLookupDTO {

        public ApplicationSchemaDefinition Schema { get; set; }

        public IDictionary<string, object> ExtraKeys { get; set; } = new SortedDictionary<string, object>();

        public long? MaxUid { get; set; }

        /// <summary>
        /// The status of the cache fetching process, assuming several consective fetches from client to server
        /// </summary>
        public IDictionary<string, CacheRoundtripStatus> CacheRoundtripStatuses { get; set; } = new Dictionary<string, CacheRoundtripStatus>();

        public bool IsOffline { get; set; }

        public int GlobalLimit { get; set; } = int.MaxValue;


        public LinkedList<string> FormatAtLevel(int index, IEnumerable<string> baseStrings, IEnumerable<string> values) {
            var sortedValues = new SortedSet<string>(values);

            var results = new LinkedList<string>();
            foreach (var baseString in baseStrings) {
                foreach (var valueString in sortedValues) {
                    results.AddLast(baseString.Replace("${" + index + "}", valueString));
                }
            }
            return results;
        }

        public ICollection<string> BuildKeys() {

            var results = new LinkedList<string>();

            var sorted = new SortedDictionary<string, string>();
            if (Schema != null) {
                sorted.Add("application", Schema.ApplicationName);
                sorted.Add("schemaid", Schema.SchemaId);
            }

            if (IsOffline) {
                sorted.Add("offline", "true");
            }

            var nonEnumerables = ExtraKeys.Where(a => a.Value is string);

            var enumerableKeys = ExtraKeys.Where(a => a.Value is IEnumerable<string>);

            foreach (var nonEnumerable in nonEnumerables) {
                sorted.Add(nonEnumerable.Key, nonEnumerable.Value.ToString());
            }
            var keysasList = enumerableKeys as IList<KeyValuePair<string, object>> ?? enumerableKeys.ToList();

            if (!keysasList.Any()) {
                return new List<string>() { ConvertToKeys(sorted) };
            }

            var i = 0;
            foreach (var key in keysasList) {
                sorted.Add(key.Key, "${" + i++ + "}");
            }

            var baseString = ConvertToKeys(sorted);

            i = 0;
            results.AddLast(baseString);
            foreach (var key in keysasList) {
                results = FormatAtLevel(i++, results, (IEnumerable<string>)key.Value);
            }

            i = 0;
            //adding all linear combinations scenario
            foreach (var key in keysasList) {
                var elements = new SortedSet<string>((IEnumerable<string>)key.Value);
                baseString = baseString.Replace("${" + i++ + "}", string.Join(",", elements));
            }

            results.AddFirst(baseString);


            return new HashSet<string>(results);
        }

        private string ConvertToKeys(SortedDictionary<string, string> sortedItems) {
            if (!sortedItems.Any()) {
                return null;
            }

            var sb = new StringBuilder();
            foreach (var sorted in sortedItems) {
                sb.AppendFormat("{0}={1}", sorted.Key.ToLower(), sorted.Value).Append(";");
            }
            return sb.ToString(0, sb.Length - 1);
        }

        protected bool Equals(RedisLookupDTO other) {
            return Equals(Schema, other.Schema) && Equals(ExtraKeys, other.ExtraKeys) && MaxUid == other.MaxUid && Equals(CacheRoundtripStatuses, other.CacheRoundtripStatuses) && IsOffline == other.IsOffline && GlobalLimit == other.GlobalLimit;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RedisLookupDTO)obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (Schema != null ? Schema.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ExtraKeys != null ? ExtraKeys.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ MaxUid.GetHashCode();
                hashCode = (hashCode * 397) ^ (CacheRoundtripStatuses != null ? CacheRoundtripStatuses.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsOffline.GetHashCode();
                hashCode = (hashCode * 397) ^ GlobalLimit;
                return hashCode;
            }
        }
    }
}
