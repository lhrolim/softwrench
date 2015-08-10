using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class BaseServiceRequestDataSet : BaseTicketDataSet {
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

        public SearchRequestDto BuildRelatedAttachmentsWhereClause(CompositionPreFilterFunctionParameters parameter) {
            var originalEntity = parameter.OriginalEntity;

            var origrecordid = parameter.BASEDto.ValuesDictionary["ownerid"].Value;
            var origrecordclass = parameter.BASEDto.ValuesDictionary["ownertable"].Value as string;

            var ticketuid = originalEntity.GetAttribute("ticketuid");


            var siteId = originalEntity.GetAttribute("siteid");
            var assetsiteid = originalEntity.GetAttribute("assetsiteid");

            var assetnum = originalEntity.GetAttribute("assetnum");
            var location = originalEntity.GetAttribute("location");
            var solution = originalEntity.GetAttribute("solution");



            var sb = new StringBuilder();
            //base section
            sb.AppendFormat(
                @"(ownertable = 'SR' and ownerid = {0})
                or (  ownertable='PROBLEM' and ownerid in (select ticketuid from problem where ticketid={1} and class={2} )) 
                or (  ownertable='INCIDENT' and ownerid in (select ticketuid from incident where ticketid={1} and class={2}))
                or (  ownertable='WOCHANGE' and ownerid in (select workorderid from wochange where wonum={1} and woclass={2}))
                or (  ownertable='WORELEASE' and ownerid in (select workorderid from worelease where wonum={1} and woclass={2}))
                or (ownertable='WOACTIVITY' and ownerid in (select workorderid from woactivity where origrecordid={1} and origrecordclass={2}))
                or (ownertable='JOBPLAN' and ownerid in (select jobplanid from jobplan where jpnum in (select jpnum from woactivity where origrecordid={1} and origrecordclass={2})))
            ", "'" + ticketuid + "'", "'" + origrecordid + "'", "'" + origrecordclass + "'");

            if (assetnum != null) {
                sb.AppendFormat(
                    @" or(ownertable = 'ASSET' and ownerid in (select assetuid from asset where assetnum ='{0}' and siteid ='{1}'))",
                    assetnum, siteId);
            }

            if (location != null) {
                sb.AppendFormat(@" or (ownertable = 'LOCATIONS' and ownerid in (select locationsid from locations where location ='{0}' and assetsiteid ='{1}'))",
                    location, siteId);
            }

            if (solution != null) {
                sb.AppendFormat(@" or (ownertable='SOLUTION' and ownerid in (select solutionid from solution where solution='{0}'))", solution);
            }

            sb.AppendFormat(
                @" or (ownertable='COMMLOG' and ownerid in (select commloguid from commlog where ownertable='SR' and ownerid='{0}')) ", ticketuid);


            parameter.BASEDto.SearchValues = null;
            parameter.BASEDto.SearchParams = null;
            parameter.BASEDto.AppendWhereClause(sb.ToString());

            return parameter.BASEDto;
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