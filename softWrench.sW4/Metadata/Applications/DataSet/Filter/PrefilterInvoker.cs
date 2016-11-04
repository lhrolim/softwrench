using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using cts.commons.portable.Util;
using softwrench.sw4.api.classes.fwk.dataset;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Util;

namespace softWrench.sW4.Metadata.Applications.DataSet.Filter {
    public class PrefilterInvoker {

        protected const string MethodNotFound = "filterFunction {0} not found on DataSet {1}";

        private const string WrongPreFilterMethod = "PrefilterFunction {0} of dataset {1} was implemented with wrong signature. See IDataSet documentation";

        private static IDictionary<DataSetRelationshipCacheKey, MethodInfo> PrefilterAttributeCache = new Dictionary<DataSetRelationshipCacheKey, MethodInfo>();

        public static SearchRequestDto ApplyPreFilterFunction<T>(IDataSet dataSet, BasePreFilterParameters<T> preFilterParam, string prefilterFunctionName)  {
            var mi = dataSet.GetType().GetMethod(prefilterFunctionName);
            if (mi == null) {
                throw new InvalidOperationException(String.Format(MethodNotFound, prefilterFunctionName, dataSet.GetType().Name));
            }
            if (mi.GetParameters().Count() != 1 || mi.ReturnType != typeof(SearchRequestDto) || typeof(BasePreFilterParameters<>).IsAssignableFrom(mi.GetParameters()[0].ParameterType)) {
                throw new InvalidOperationException(String.Format(WrongPreFilterMethod, prefilterFunctionName, dataSet.GetType().Name));
            }
            return (SearchRequestDto)mi.Invoke(dataSet, new object[] { preFilterParam });
        }

        public static SearchRequestDto ApplyAnnotatedPreFilterFunctionIfExists<T>(IDataSet dataSet, BasePreFilterParameters<T> preFilterParam, string relationshipName)  {
            var cacheKey = new DataSetRelationshipCacheKey(dataSet.GetType(),relationshipName);


            if (PrefilterAttributeCache.ContainsKey(cacheKey)) {
                var mi = PrefilterAttributeCache[cacheKey];
                return DoInvokeMethod(dataSet, preFilterParam, mi);
            }

            var methods = dataSet.GetType().GetMethods();
            foreach (var mi in methods) {
                var attr = mi.GetCustomAttribute(typeof(PreFilterAttribute));
                if (attr != null) {
                    var prAttr = (PreFilterAttribute)attr;
                    if (EntityUtil.IsRelationshipNameEquals(prAttr.RelationshipName, relationshipName)) {
                        PrefilterAttributeCache.Add(cacheKey, mi);
                        return DoInvokeMethod(dataSet, preFilterParam, mi);
                    }
                }
            }
            PrefilterAttributeCache.Add(cacheKey, null);
            return preFilterParam.BASEDto;

        }

        private static SearchRequestDto DoInvokeMethod<T>(IDataSet dataSet, BasePreFilterParameters<T> preFilterParam,
            MethodInfo mi)  {
            if (mi == null) {
                return preFilterParam.BASEDto;
            }

            if (mi.GetParameters().Count() != 1 || mi.ReturnType != typeof(SearchRequestDto) ||
                typeof(BasePreFilterParameters<>).IsAssignableFrom(mi.GetParameters()[0].ParameterType)) {
                throw new InvalidOperationException(String.Format(WrongPreFilterMethod, mi.Name, dataSet.GetType().Name));
            }
            return (SearchRequestDto)mi.Invoke(dataSet, new object[] { preFilterParam });
        }

        class DataSetRelationshipCacheKey {
            private readonly Type _datasetType;
            private readonly string _relationshipType;

            public DataSetRelationshipCacheKey(Type datasetType, string relationshipType) {
                _datasetType = datasetType;
                _relationshipType = relationshipType;
            }

            private bool Equals(DataSetRelationshipCacheKey other) {
                return _datasetType == other._datasetType && string.Equals(_relationshipType, other._relationshipType);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != this.GetType())
                    return false;
                return Equals((DataSetRelationshipCacheKey)obj);
            }

            public override int GetHashCode() {
                unchecked {
                    return ((_datasetType != null ? _datasetType.GetHashCode() : 0) * 397) ^ (_relationshipType != null ? _relationshipType.GetHashCode() : 0);
                }
            }
        }


    }
}
