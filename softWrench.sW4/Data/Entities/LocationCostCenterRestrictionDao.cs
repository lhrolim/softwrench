using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softwrench.sW4.Shared2.Data;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Data.Entities {
    public class LocationCostCenterRestrictionDao {
        private const string ChartOfAccountsEntity = "chartofaccounts";
        private const string PluspCustomerEntity = "pluspcustomer";

        private EntityRepository EntityRepository {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<EntityRepository>(typeof(EntityRepository));
            }
        }

        public async void GetCostCenterDescription(Dictionary<string, string> costCenters) {
            var entityMetadata = MetadataProvider.Entity(ChartOfAccountsEntity);
            const string glAccount = "glaccount";
            var searchRequestDto = new SearchRequestDto {
                WhereClause = BuildIWhereIn(ChartOfAccountsEntity, glAccount, new List<string>(costCenters.Keys))
            };
            var attributeHolders = await EntityRepository.Get(entityMetadata, searchRequestDto);
            
            if (!attributeHolders.Any()) {
                return;
            }
            foreach (var attributeHolder in attributeHolders) {
                costCenters[(string)attributeHolder.GetAttribute(glAccount)] =
                    (string)attributeHolder.GetAttribute("accountname");
            }
        }

        public async void GetLocationDescription(Dictionary<string, string> customers) {
            var entityMetadata = MetadataProvider.Entity(PluspCustomerEntity);
            const string customer = "customer";
            var searchRequestDto = new SearchRequestDto {
                WhereClause = BuildIWhereIn(PluspCustomerEntity, customer, new List<string>(customers.Keys))
            };
            var attributeHolders = await EntityRepository.Get(entityMetadata, searchRequestDto);
            if (!attributeHolders.Any()) {
                return;
            }
            foreach (var attributeHolder in attributeHolders) {
                customers[(string)attributeHolder.GetAttribute(customer)] =
                    (string)attributeHolder.GetAttribute("name");
            }
        }

        private static string BuildIWhereIn(string entityName, string attributeName, IReadOnlyCollection<string> valuesList) {
            var @where = entityName + "." + attributeName + " IN";
            var values = string.Empty;
            var i = 1;
            foreach (var valueFromList in valuesList) {
                if (i == valuesList.Count) {
                    values += "'" + valueFromList + "'";
                } else {
                    values += "'" + valueFromList + "'" + ",";
                }
                i++;
            }
            var query = where + " (" + values + ")";
            return query;
        }
    }
}
