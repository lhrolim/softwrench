using System;

using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sw4.Shared2.Metadata.Modules;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Configuration.Util {

    public class CategoryTreeCache : ISingletonComponent {

        private readonly IDictionary<ConfigTypes, SortedSet<CategoryDTO>> _categoryCache = new Dictionary<ConfigTypes, SortedSet<CategoryDTO>>();
        private readonly SortedSet<ModuleDefinition> _cachedConfigModules = new SortedSet<ModuleDefinition>();
        private readonly SWDBHibernateDAO _dao;

        public CategoryTreeCache(SWDBHibernateDAO dao) {
            _dao = dao;
        }

        public SortedSet<CategoryDTO> GetCache(ConfigTypes type) {
            if (!_categoryCache.ContainsKey(type)) {
                var propertyDefinitions = _dao.FindAll<PropertyDefinition>(typeof(PropertyDefinition));
                _categoryCache[type] = BuildCache(propertyDefinitions, type);
            }
            return _categoryCache[type];
        }

        public SortedSet<CategoryDTO> Update(CategoryDTO category) {
            var values = GetCacheToUse(category);
            DoUpdate(category, values);
            return GetCacheToUse(category);
        }

        private SortedSet<CategoryDTO> GetCacheToUse(CategoryDTO category) {
            var fullKey = category.FullKey;
            var type = ConfigTypes.Global;
            if (fullKey.StartsWith("/" + ConfigTypes.WhereClauses.GetRootLevel())) {
                type = ConfigTypes.WhereClauses;
            } else if (fullKey.StartsWith("/" + ConfigTypes.DashBoards.GetRootLevel())) {
                type = ConfigTypes.DashBoards;
            }
            SortedSet<CategoryDTO> values;
            _categoryCache.TryGetValue(type, out values);
            values = values == null ? new SortedSet<CategoryDTO>() : new SortedSet<CategoryDTO>(values);
            return values;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <param name="tolookup"></param>
        /// <returns>true if the cache was updated, false otherwise</returns>
        public static bool DoUpdate(CategoryDTO category, SortedSet<CategoryDTO> tolookup) {
            var categories = category.FullKey.Split('/');
            CategoryDTO cat = null;
            foreach (var categoryKey in categories) {
                if (string.IsNullOrEmpty(categoryKey)) {
                    continue;
                }
                cat = tolookup.First(c => c.Key == categoryKey);
                tolookup = cat.Children;
            }
            if (cat == null) {
                return false;
            }
            cat.Definitions.Clear();
            cat.Definitions = category.Definitions;
            return true;
        }

        internal static SortedSet<CategoryDTO> BuildCache(IEnumerable<PropertyDefinition> definitions, ConfigTypes type) {
            IDictionary<string, CategoryDTO> catDict = new Dictionary<string, CategoryDTO>();
            foreach (var definition in definitions) {
                if (!definition.Visible) {
                    continue;
                }
                var catFullKey = CategoryUtil.GetCategoryFullKey(definition.FullKey);
                if (!catFullKey.StartsWith("/" + type.GetRootLevel())) {
                    continue;
                }
                if (!catDict.ContainsKey(catFullKey)) {
                    BuildCategoryEntries(catDict, catFullKey, null);
                }
                var dto = catDict[catFullKey];
                dto.Definitions.Add(definition);
            }
            var categoryDtos = catDict.Values.Where(c => c.Parent == null);
            return new SortedSet<CategoryDTO>(categoryDtos);
        }

        private static void BuildCategoryEntries(IDictionary<string, CategoryDTO> catDict, string catFullKey, CategoryDTO child) {

            CategoryDTO dto;
            if (!catDict.TryGetValue(catFullKey, out dto)) {
                dto = new CategoryDTO(catFullKey);
                catDict.Add(catFullKey, dto);
            }
            if (child != null) {
                dto.Children.Add(child);
                child.Parent = dto;
            }
            var categoryParentKey = CategoryUtil.GetCategoryParentKey(catFullKey);
            if (categoryParentKey != null) {
                BuildCategoryEntries(catDict, categoryParentKey, dto);
            }
        }


        public IEnumerable<ModuleDefinition> GetConfigModules() {
            if (_cachedConfigModules.Any()) {
                return _cachedConfigModules;
            }
            var menuModules = MetadataProvider.Modules(ClientPlatform.Web);
            var configModules = _dao.FindByQuery<String>(PropertyValue.DistinctModules);
            foreach (var moduleDefinition in menuModules) {
                _cachedConfigModules.Add(moduleDefinition);
            }
            foreach (var configModule in configModules) {
                if (!string.IsNullOrEmpty(configModule) && _cachedConfigModules.All(f => !f.Id.Equals(configModule, StringComparison.CurrentCultureIgnoreCase))) {
                    _cachedConfigModules.Add(new ModuleDefinition(configModule, String.Format("{0} -- not declared yet --", configModule)));
                }
            }
            return _cachedConfigModules;
        }
    }
}
