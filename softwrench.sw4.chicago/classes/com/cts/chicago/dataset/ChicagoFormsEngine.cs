using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using softwrench.sw4.chicago.classes.com.cts.chicago.configuration;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.dataset {
    public class ChicagoFormsEngine : IConnectorEngine {

        private readonly IConfigurationFacade _configurationFacade;

        public ChicagoFormsEngine(IConfigurationFacade configurationFacade) {
            _configurationFacade = configurationFacade;
        }

        public Task<int> Count(EntityMetadata entityMetadata, SearchRequestDto searchDto) {
            return Task.FromResult(GetPdfs(searchDto).Count());
        }

        public Task<AttributeHolder> FindById(SlicedEntityMetadata entityMetadata, string id, Tuple<string, string> userIdSitetuple) {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<AttributeHolder>> Find(SlicedEntityMetadata entityMetadata, PaginatedSearchRequestDto searchDto,
            IDictionary<string, ApplicationCompositionSchema> applicationCompositionSchemata) {
            return Task.FromResult(InnerFind(searchDto));
        }

        public TargetResult Execute(OperationWrapper operationWrapper) {
            throw new NotImplementedException();
        }

        public string GetFormsDir() {
            return GetFormsDir(ChicagoConfigurationSetup.FormsDir);
        }

        public string GetIbmFormsDir() {
            return GetFormsDir(ChicagoConfigurationSetup.IbmFormsDir);
        }

        public bool IsIbmUser() {
            var user = SecurityFacade.CurrentUser();
            return user.Login.EqualsIc("swadmin") || user.Login.ToLower().EndsWith(".ibm.com") || user.Login.ToLower().EndsWith("@ibm.com");
        }

        private string GetFormsDir(string dirKey) {
            var formsDir = _configurationFacade.Lookup<string>(dirKey);
            if (string.IsNullOrEmpty(formsDir)) {
                throw new Exception("Forms directory configuration not set. Please setup the configuration with key " + dirKey + " on configuration page.");
            }

            if (!Directory.Exists(formsDir)) {
                throw new Exception("The directory \"" + formsDir + "\" does not exist. Please fix the configuration with key " + dirKey + " on configuration page.");
            }
            return formsDir;
        }

        private IReadOnlyList<AttributeHolder> InnerFind(SearchRequestDto searchDto) {
            var result = new List<AttributeHolder>();
            GetSortedPdfs(searchDto).ForEach(form => {
                var dict = new Dictionary<string, string> { { "name", form.Name }, { "download", form.Name }, { "isibm", form.IsIbm.ToString() } };
                result.Add(DataMap.GetInstanceFromStringDictionary("forms", dict));
            });
            return ImmutableList.CreateRange(result);
        }

        private static List<Form> GetPdfs(string dir, bool isIbm) {
            return Directory.EnumerateFiles(dir, "*.pdf").Select(Path.GetFileNameWithoutExtension).Select(name => new Form(name, isIbm)).ToList();
        }

        private IEnumerable<Form> GetPdfs(SearchRequestDto searchDto) {
            var formsDir = GetFormsDir();
            var pdfs = GetPdfs(formsDir, false);

            // ibm user
            if (IsIbmUser()) {
                formsDir = GetIbmFormsDir();
                pdfs.AddRange(GetPdfs(formsDir, true));
            }

            var quickSearch = searchDto.QuickSearchDTO;
            if (quickSearch == null || string.IsNullOrEmpty(quickSearch.QuickSearchData)) {
                return pdfs;
            }

            return pdfs.Where(form => form.Name.ContainsIgnoreCase(quickSearch.QuickSearchData));
        }

        private List<Form> GetSortedPdfs(SearchRequestDto searchDto) {
            var pdfs = GetPdfs(searchDto).ToList();
            pdfs.Sort((formA, formB) => string.Compare(formA.Name, formB.Name, StringComparison.Ordinal));
            var asc = !"name".Equals(searchDto.SearchSort) || searchDto.SearchAscending;
            if (!asc) {
                pdfs.Reverse();
            }
            return pdfs;
        }

        private class Form {
            public Form(string name, bool isIbm) {
                Name = name;
                IsIbm = isIbm;
            }

            public string Name { get; private set; }
            public bool IsIbm { get; private set; }
        }
    }
}
