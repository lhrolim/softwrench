﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.Util;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Security.Services;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.dataset {
    class ChicagoServiceRequestDataSet : BaseServiceRequestDataSet {


        public override async Task<TargetResult> Execute(ApplicationMetadata application, JObject json, string id, string operation, bool isBatch,
            Tuple<string, string> userIdSite, IDictionary<string,object>customParameters) {
            var result = await base.Execute(application, json, id, operation, isBatch, userIdSite, customParameters);

            if ("servicerequest".Equals(application.Name) && "crud_update".Equals(operation)) {
                var user = SecurityFacade.CurrentUser();
                var detail = AsyncHelper.RunSync(() => GetApplicationDetail(application, user, new DetailRequest(id, application.Schema.GetSchemaKey())));

                if (detail != null) {
                    result.ResultObject = detail.ResultObject;
                }
            }

            return result;
        }



        /// <summary>
        /// Filtering owners based upon ownergroup
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public SearchRequestDto FilterValidOwners(AssociationPreFilterFunctionParameters parameters) {
            var dto = parameters.BASEDto;

            var ownergroup = parameters.OriginalEntity.GetStringAttribute("ownergroup");
            if (ownergroup == null) {
                //no actions needed
                return dto;
            }

            Log.DebugFormat("constraining owners of ownergroup {0}", ownergroup);

            dto.AppendWhereClauseFormat("personid in (select respparty from persongroupteam where persongroup='{0}')", ownergroup);



            return dto;
        }


        protected override string ClassificationIdToUse() {
            return "classificationid";
        }


        protected override string BuildClassificationQuery(OptionFieldProviderParameters parameters, string ticketclass, string searchString = null) {
            var baseQuery = base.BuildClassificationQuery(parameters, ticketclass, searchString);
            baseQuery +=
                " and (c.classificationid in (select classstructure.classificationid from classstructure inner join pluspcustassoc on pluspcustassoc.ownertable = 'CLASSSTRUCTURE' and pluspcustassoc.ownerid = classstructure.classstructureuid and pluspcustassoc.customer = 'CPS-00'))";
            return baseQuery;
        }



        public SearchRequestDto FilterQSRWorklogs(CompositionPreFilterFunctionParameters parameter) {
            parameter.BASEDto.AppendWhereClause(" clientviewable = 1 ");
            return parameter.BASEDto;
        }

        public override string ApplicationName() {
            return "servicerequest,quickservicerequest";
        }

        public override string ClientFilter() {
            return "chicago";
        }
    }
}
