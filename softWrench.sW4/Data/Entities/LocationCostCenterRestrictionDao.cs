﻿using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softwrench.sW4.Shared2.Data;

namespace softWrench.sW4.Data.Entities {
    public class LocationCostCenterRestrictionDao {
        private const string ChartOfAccountsEntity = "chartofaccounts";
        private const string PluspCustomerEntity = "pluspcustomer";

        private readonly EntityRepository _entityRepository = new EntityRepository();

        public void GetCostCenterDescription(Dictionary<string, string> costCenters) {
            var entityMetadata = MetadataProvider.Entity(ChartOfAccountsEntity);
            const string glAccount = "glaccount";
            var searchRequestDto = new SearchRequestDto {
                WhereClause = BuildIWhereIn(ChartOfAccountsEntity, glAccount, new List<string>(costCenters.Keys))
            };
            var result = _entityRepository.Get(entityMetadata, searchRequestDto);
            var attributeHolders = result as AttributeHolder[] ?? result.ToArray();
            if (!attributeHolders.Any()) {
                return;
            }
            foreach (var attributeHolder in attributeHolders) {
                costCenters[(string)attributeHolder.GetAttribute(glAccount)] =
                    (string)attributeHolder.GetAttribute("accountname");
            }
        }

        public void GetLocationDescription(Dictionary<string, string> customers) {
            var entityMetadata = MetadataProvider.Entity(PluspCustomerEntity);
            const string customer = "customer";
            var searchRequestDto = new SearchRequestDto {
                WhereClause = BuildIWhereIn(PluspCustomerEntity, customer, new List<string>(customers.Keys))
            };
            var result = _entityRepository.Get(entityMetadata, searchRequestDto);
            var attributeHolders = result as AttributeHolder[] ?? result.ToArray();
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
