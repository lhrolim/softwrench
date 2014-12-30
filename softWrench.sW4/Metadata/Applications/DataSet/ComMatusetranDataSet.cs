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
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;


namespace softWrench.sW4.Metadata.Applications.DataSet
{
    class ComMatusetranDataSet : MaximoApplicationDataSet
    {
        public SearchRequestDto FilterMaterials(AssociationPreFilterFunctionParameters parameters)
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

            return filter;
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
