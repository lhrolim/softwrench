using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.portable.Util;
using softWrench.sW4.Util;

namespace softWrench.sW4.Mapping {

    public class MappingResolver : IMappingResolver {

        private readonly ISWDBHibernateDAO _dao;

        public MappingResolver(ISWDBHibernateDAO dao) {
            _dao = dao;
        }

        private readonly IDictionary<string, InternalMappingStructure> _parsedStructureCache = new ConcurrentDictionary<string, InternalMappingStructure>();


        public IReadOnlyList<string> Resolve(string key, IEnumerable<string> originalValues) {


            var result = new List<string>();
            if (originalValues == null) {
                return result;
            }

            var parsedStructure = BuildParsedStructure(key);

            var enumerable = originalValues as IList<string> ?? originalValues.ToList();

            foreach (var originalValue in enumerable) {
                if (parsedStructure.SimpleAssociations.ContainsKey(originalValue)) {
                    result.AddRange(parsedStructure.SimpleAssociations[originalValue]);
                }
            }

            foreach (var andCondition in parsedStructure.AndValues.Values) {
                var keysToMach = andCondition.Keys;
                if (enumerable.ContainsAll(keysToMach)) {
                    result.AddRange(andCondition.Values);
                }
            }


            result.AddRange(parsedStructure.DefaultValues);

            return result;
        }

        private InternalMappingStructure BuildParsedStructure(string key) {
            if (_parsedStructureCache.ContainsKey(key)) {
                return _parsedStructureCache[key];
            }
            var mappings = _dao.FindByQuery<Mapping>(Mapping.ByKey, key);
            var internalMapping = new InternalMappingStructure();
            foreach (var mapping in mappings) {
                var destinationValue = new HashSet<string>(mapping.DestinationValue.Split(','));
                if (mapping.OriginValue.EqualsIc("@default")) {
                    internalMapping.DefaultValues.AddAll(destinationValue);
                } else if (mapping.OriginValue.Contains("&")) {
                    var originValues = new HashSet<string>(mapping.OriginValue.Split('&'));
                    var andValuesHolder = new InternalMappingStructure.AndValuesHolder();
                    internalMapping.AndValues.Add(mapping.Id.Value, andValuesHolder);
                    andValuesHolder.Keys.AddAll(originValues);
                    andValuesHolder.Values.AddAll(destinationValue);
                } else {
                    var originValues = mapping.OriginValue.Split(',');
                    foreach (var originValue in originValues) {
                        internalMapping.SimpleAssociations.Add(originValue, destinationValue);
                    }
                }
            }
            return internalMapping;

        }

        public void UpdateCache(string key) {
            _parsedStructureCache.Remove(key);
            BuildParsedStructure(key);
        }

    }
}
