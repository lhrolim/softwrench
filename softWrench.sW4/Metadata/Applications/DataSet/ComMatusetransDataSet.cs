using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Windows.Controls;
using DocumentFormat.OpenXml.Spreadsheet;
using NHibernate.Hql.Ast.ANTLR;
using NHibernate.Linq;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Data;
using softwrench.sw4.Shared2.Data.Association;
using cts.commons.simpleinjector;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;

namespace softWrench.sW4.Metadata.Applications.DataSet
{
    class ComMatusetransDataSet : MaximoApplicationDataSet
    {        
        private IEnumerable<IAssociationOption> filterMaterials(AssociationPostFilterFunctionParameters postParams) {
            return (from item in postParams.Options where item.Label != null && item.Value.Equals(postParams.OriginalEntity.Attributes["itemnum"]) select new AssociationOption(item.Label, item.Label)).Cast<IAssociationOption>().ToList();
        }

        public SearchRequestDto filterPlannedMaterials(AssociationPreFilterFunctionParameters parameters)
        {
            var orgid = parameters.OriginalEntity.GetAttribute("orgid");
            var wonum = parameters.OriginalEntity.GetAttribute("refwo");
            var filter = parameters.BASEDto;
            var query = string.Format(@"select itemnum
                                        from wpmaterial
                                        where wonum = '{0}' and orgid = '{1}'",
            wonum, orgid);
            var result = MaxDAO.FindByNativeQuery(query, null);
            if (result != null && result.Any())
            {
                filter.AppendWhereClauseFormat("( itemnum in ({0}) )", query);
            }

            filter.AppendWhereClause(String.Format("(ITEMNUM IN (SELECT ITEMNUM FROM invbalances WHERE siteid =  '{0}') )", parameters.OriginalEntity.GetAttribute("siteid")));

            return filter;
        }

        public IEnumerable<IAssociationOption> filterStoreLoc(AssociationPostFilterFunctionParameters postParams)
        {
            return filterMaterials(postParams);
        }

        public IEnumerable<IAssociationOption> filterLotnum(AssociationPostFilterFunctionParameters postParams)
        {
            return filterMaterials(postParams);
        }

        public IEnumerable<IAssociationOption> filterBinnum(AssociationPostFilterFunctionParameters postParams)
        {
            return filterMaterials(postParams);
        }

        public override string ApplicationName()
        {
            return "matusetrans";
        }

        public override string ClientFilter()
        {
            return "manchester";
        }
    }
}
