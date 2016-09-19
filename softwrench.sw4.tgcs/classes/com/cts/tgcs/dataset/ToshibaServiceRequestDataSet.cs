using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using cts.commons.persistence;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Util;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.dataset {
    public class ToshibaServiceRequestDataSet : BaseServiceRequestDataSet {

        private readonly ToshibaRestCompositionsResolver _restCompositionsResolver;

        public ToshibaServiceRequestDataSet(ISWDBHibernateDAO swdbDao, ToshibaRestCompositionsResolver restCompositionsResolver) : base(swdbDao) {
            _restCompositionsResolver = restCompositionsResolver;
        }

        public SearchRequestDto FilterClassification(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            // Only show classifications with classstructures that have pluspcustassoc's to 'CPS-00'
            filter.AppendWhereClause("classificationid in (select classstructure.classificationid from classstructure inner join pluspcustassoc on pluspcustassoc.ownertable = 'CLASSSTRUCTURE' and pluspcustassoc.ownerid = classstructure.classstructureuid and pluspcustassoc.customer = 'TTC-00')");
            return filter;
        }

        public SearchRequestDto FilterQSRWorklogs(CompositionPreFilterFunctionParameters parameter) {
            parameter.BASEDto.AppendSearchEntry("clientviewable", "1");
            return parameter.BASEDto;
        }

        protected override async Task<IDictionary<string, EntityRepository.SearchEntityResult>> ResolveCompositionResult(SlicedEntityMetadata parentEntityMetadata, IDictionary<string, ApplicationCompositionSchema> compositionsToResolve, Entity parentData, PaginatedSearchRequestDto search) {
            var requestedWorklogs = compositionsToResolve.ContainsKey("worklog_");
            var requestedAttachments = compositionsToResolve.ContainsKey("attachment_");
            if (!requestedWorklogs && !requestedAttachments) {
                return await base.ResolveCompositionResult(parentEntityMetadata, compositionsToResolve, parentData, search);
            }
            var restCompositions = new Dictionary<string, ApplicationCompositionSchema>();
            if (requestedWorklogs) {
                var worklogComposition = compositionsToResolve["worklog_"];
                compositionsToResolve.Remove("worklog_");
                restCompositions.Add("worklog_", worklogComposition);
            }
            if (requestedAttachments) {
                var attachmentComposition = compositionsToResolve["attachment_"];
                compositionsToResolve.Remove("attachment_");
                restCompositions.Add("attachment_", attachmentComposition);
            }

            // every composition except worklogs and attachments
            var baseResult = await base.ResolveCompositionResult(parentEntityMetadata, compositionsToResolve, parentData, search);
            
            // only worklogs and/or attachments
            var restResult = await _restCompositionsResolver.ResolveRestCompositions(parentEntityMetadata, restCompositions, parentData, search);
            
            return baseResult.AddRange(restResult);
        }
        

        protected override void HandleAttachments(CompositionFetchResult data) {
            var attachments = data.ResultObject["attachment_"].ResultList;
            
            foreach (var attachment in attachments) {
                if (!attachment.ContainsKey("docinfo_.urlname") && !attachment.ContainsKey("urlname")) continue;

                if (attachment.ContainsKey("weburl")){
                    // download url from REST
                    attachment["download_url"] = attachment["weburl"];
                } else if (attachment.ContainsKey("docinfo_.urlname")) {
                    // download url from 'local'
                    var docInfoURL = (string) attachment["docinfo_.urlname"];
                    attachment["download_url"] = AttachmentHandler.GetFileUrl(docInfoURL);
                }
                AttachmentHandler.BuildParsedURLName(attachment);
            }
        }

        public override SearchRequestDto BuildRelatedAttachmentsWhereClause(CompositionPreFilterFunctionParameters parameter) {
            parameter.BASEDto.GetParameters();
            return base.BuildRelatedAttachmentsWhereClause(parameter);
        }

        protected override string ClassificationIdToUse() {
            return "classificationid";
        }


        public override string ApplicationName() {
            return "servicerequest,quickservicerequest";
        }

        public override string ClientFilter() {
            return "tgcs";
        }
        
    }
}
