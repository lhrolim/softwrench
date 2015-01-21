using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.Search;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.Util;
using System;
using softWrench.sW4.Security.Services;
namespace softWrench.sW4.Metadata.Applications.DataSet
{
    class GLComponentDataSet : MaximoApplicationDataSet
    {
        public IEnumerable<IAssociationOption> GetGLAccountPartN(OptionFieldProviderParameters parameters)
        {
            if (parameters.OptionField.ExtraParameter != null)
            {
                var user = SecurityFacade.CurrentUser();
                var orgid = parameters.OriginalEntity.Attributes.Any() ? parameters.OriginalEntity.Attributes["orgid"] : null;
                
                // default orgid if none was found. this is usually the case if it is a new entry.
                if (orgid == null) orgid = user.OrgId;
                var query = string.Format(@"SELECT compvalue, compvalue + ' - ' + comptext AS comptext FROM glcomponents
                                            where glorder = {0} and active = 1 and orgid = '{1}'", parameters.OptionField.ExtraParameter, orgid);

                var results = MaxDAO.FindByNativeQuery(query, null);

                if (results != null && results.Any())
                {
                    var gllengthquery = string.Format(@"SELECT gllength FROM glconfigure
                                                        where glorder = {0} and orgid = '{1}'", parameters.OptionField.ExtraParameter, orgid);

                    var gllengthresult = MaxDAO.FindSingleByNativeQuery<object>(gllengthquery, null);

                    var ITEMs = new List<IAssociationOption>(); 
                    
                    ITEMs.Add(new AssociationOption(new String('?', (int)gllengthresult), String.Format(" {0}", new String('?', (int)gllengthresult))));
                    foreach(var result in results) {
                        ITEMs.Add(new AssociationOption(result["compvalue"], result["comptext"]));
                    }

                    return ITEMs; 
                }
            }

            return new List<IAssociationOption>();
        }

        public override string ApplicationName()
        {
            return "glcomponents";
        }

        public override string ClientFilter()
        {
            return null;
        }
    }
}