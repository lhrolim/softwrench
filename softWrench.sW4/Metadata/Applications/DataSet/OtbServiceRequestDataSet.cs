using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Relationship.Composition;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SimpleInjector;

namespace softWrench.sW4.Metadata.Applications.DataSet
{
    class OtbServiceRequestDataSet : BaseApplicationDataSet
    {
        /* Need to add this prefilter function for the problem codes !! 
        public SearchRequestDto FilterProblemCodes(AssociationPreFilterFunctionParameters parameters)
        {

        }*/

        private static SWDBHibernateDAO _swdbDao;
        private const string application = "SR";

        private SWDBHibernateDAO GetSWDBDAO() {
            if (_swdbDao == null) {
                _swdbDao = SimpleInjectorGenericFactory.Instance.GetObject<SWDBHibernateDAO>(typeof(SWDBHibernateDAO));
            }
            return _swdbDao;
        }

        protected override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = base.GetApplicationDetail(application, user, request);
            var datamap = result.ResultObject;
            var idFieldName = result.Schema.IdFieldName;
            var applicationName = result.ApplicationName;
            JoinCommLogData(datamap, idFieldName, applicationName);
            return result;
        }

        private void JoinCommLogData(DataMap resultObject, string parentIdFieldName, string applicationName) {
            var applicationItemID = resultObject.GetAttribute(parentIdFieldName);
            var user = SecurityFacade.CurrentUser();

            if (applicationItemID == null || user == null) {
                return;
            }

            var commData = GetSWDBDAO().FindByQuery<MaxCommReadFlag>(MaxCommReadFlag.ByItemIdAndUserId, applicationName, applicationItemID, user.DBId);

            var commlogs = (IList<Dictionary<string, object>>)resultObject.Attributes["commlog_"];

            foreach (var commlog in commlogs)
            {
                var readFlag = (from c in commData
                    where c.CommlogId.ToString() == commlog["commloguid"].ToString()
                    select c.ReadFlag).FirstOrDefault();

                commlog["read"] = readFlag;
            }
        }


        public SearchRequestDto FilterAssets(AssociationPreFilterFunctionParameters parameters)
        {
            return AssetFilterBySiteFunction(parameters);
        }


        public SearchRequestDto AssetFilterBySiteFunction(AssociationPreFilterFunctionParameters parameters)
        {
            var filter = parameters.BASEDto;
            var location = (string)parameters.OriginalEntity.GetAttribute("location");
            if (location == null)
            {
                return filter;
            }
            filter.AppendSearchEntry("asset.location", location.ToUpper());
            return filter;
        }

        public override string ApplicationName()
        {
            return "servicerequest";
        }

        public override string ClientFilter()
        {
            return "otb,kongsberg";
        }
    }
}