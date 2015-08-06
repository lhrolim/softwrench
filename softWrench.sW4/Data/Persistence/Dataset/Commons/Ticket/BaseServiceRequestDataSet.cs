using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket {
    class BaseServiceRequestDataSet : BaseTicketDataSet {
        private readonly SWDBHibernateDAO _swdbDao;
        /* Need to add this prefilter function for the problem codes !! 
        public SearchRequestDto FilterProblemCodes(AssociationPreFilterFunctionParameters parameters)
        {

        }*/


        public BaseServiceRequestDataSet(SWDBHibernateDAO swdbDao) {
            _swdbDao = swdbDao;
        }




       


        public override CompositionFetchResult GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request,
            JObject currentData) {
            var compList = base.GetCompositionData(application, request, currentData);
            var user = SecurityFacade.CurrentUser();

            if (user == null) {
                return compList;
            }

            var commData = _swdbDao.FindByQuery<MaxCommReadFlag>(MaxCommReadFlag.ByItemIdAndUserId, application.Name, request.Id, user.DBId);

            if (!compList.ResultObject.ContainsKey("commlog_")) {
                return compList;
            }

            var commlogs = compList.ResultObject["commlog_"].ResultList;

            foreach (var commlog in commlogs) {
                var readFlag = (from c in commData
                                where c.CommlogId.ToString() == commlog["commloguid"].ToString()
                                select c.ReadFlag).FirstOrDefault();

                commlog["read"] = readFlag;
            }
            return compList;
        }

        public SearchRequestDto AppendCommlogDoclinks(CompositionPreFilterFunctionParameters preFilter) {
            var dto = preFilter.BASEDto;
            dto.SearchValues = null;
            dto.SearchParams = null;
            var ticketuid = preFilter.OriginalEntity.GetAttribute("ticketuid");
            dto.AppendWhereClauseFormat("(( DOCLINKS.ownerid = '{0}' ) AND ( UPPER(COALESCE(DOCLINKS.ownertable,'')) = 'SR' )  or (ownerid in (select commloguid from commlog where ownerid = '{0}' and ownertable like 'SR') and ownertable like 'COMMLOG')) and ( 1=1 )", ticketuid);
            return dto;
        }

        public IEnumerable<IAssociationOption> GetSRClassStructureType(OptionFieldProviderParameters parameters) {
            return GetClassStructureType(parameters, "SR");
        }

        public override string ApplicationName() {
            return "servicerequest";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}