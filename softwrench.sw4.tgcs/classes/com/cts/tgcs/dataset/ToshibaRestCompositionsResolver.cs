using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using log4net;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.Relational.Collection;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities.Sliced;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.dataset {
    public class ToshibaRestCompositionsResolver : ISingletonComponent {

        private static readonly ILog Log = LogManager.GetLogger(typeof(ToshibaRestCompositionsResolver));

        private readonly ToshibaRestCollectionResolver _restCollectionResolver;
        private readonly CollectionResolver _collectionResolver;
        private readonly EntityRepository _entityRepository;

        public ToshibaRestCompositionsResolver(ToshibaRestCollectionResolver restCollectionResolver, CollectionResolver collectionResolver, EntityRepository entityRepository) {
            _restCollectionResolver = restCollectionResolver;
            _collectionResolver = collectionResolver;
            _entityRepository = entityRepository;
        }
        
        /// <summary>
        /// Fetches the results of compositions that should be fetched through the rest interface.
        /// </summary>
        /// <param name="parentEntityMetadata"></param>
        /// <param name="restCompositions"></param>
        /// <param name="parentData"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<IDictionary<string, EntityRepository.SearchEntityResult>> ResolveRestCompositions(
            SlicedEntityMetadata parentEntityMetadata, 
            IDictionary<string, ApplicationCompositionSchema> restCompositions, 
            Entity parentData, 
            PaginatedSearchRequestDto search) {

            var ismticketid = parentData.GetStringAttribute("ismticketid");
            var ismticketuid = parentData.GetStringAttribute("ismticketuid");
            var ticketid = parentData.GetStringAttribute(parentEntityMetadata.UserIdFieldName);
            var ticketuid = parentData.GetStringAttribute(parentEntityMetadata.IdFieldName);

            if (string.IsNullOrEmpty(ismticketid) || string.IsNullOrEmpty(ismticketuid)) {
                Log.WarnFormat("SR with ticketid='{0}' and ticketuid='{1}' has no ism data. Fetching compositions from database.", ticketid, ticketuid);
                return await _collectionResolver.ResolveCollections(parentEntityMetadata, restCompositions, parentData, search);
            }

            // use ism identities in rest query
            parentData[parentEntityMetadata.UserIdFieldName] = ismticketid;
            parentData[parentEntityMetadata.IdFieldName] = ismticketuid;

            // only worklogs and/or attachments
            var restResult = await _restCollectionResolver.ResolveCollections(parentEntityMetadata, restCompositions, parentData, search);

            // restore entity
            parentData[parentEntityMetadata.UserIdFieldName] = ticketid;
            parentData[parentEntityMetadata.IdFieldName] = ticketuid;

            if (!restCompositions.ContainsKey("attachment_")) {
                return restResult;
            }
            
            // include related attachments data
            var attachmentComposition = restCompositions["attachment_"];
            var restAttachmentResult = restResult["attachment_"];
            MergeRelatedAttachments(parentEntityMetadata, attachmentComposition, parentData, search, restAttachmentResult);

            return restResult;
        }


        /// <summary>
        /// Will include the related attachments (list and pagination information) to the results.
        /// Related attachments are fetched locally and added the 'tail' of the result (that's ok since sort+filter are disabled).
        /// </summary>
        /// <param name="parentEntityMetadata"></param>
        /// <param name="attachmentCompositions"></param>
        /// <param name="parentData"></param>
        /// <param name="search"></param>
        /// <param name="restAttachmentResult"></param>
        private async void MergeRelatedAttachments(
            SlicedEntityMetadata parentEntityMetadata, 
            ApplicationCompositionSchema attachmentCompositions, 
            Entity parentData, 
            PaginatedSearchRequestDto search, 
            EntityRepository.SearchEntityResult restAttachmentResult) {
            
            // concatenate related attachments (database query)
            var attachmentComposition = new Dictionary<string, ApplicationCompositionSchema>() { { "attachment_", attachmentCompositions } };
            var restAttachmentPageResult = restAttachmentResult.PaginationData;
            var ticketuid = parentData.GetStringAttribute(parentEntityMetadata.IdFieldName);

            // trick to not include self in the related
            parentData[parentEntityMetadata.IdFieldName] = int.MinValue.ToString();

            if (restAttachmentPageResult == null) {
                // not a paginated request: just concat the lists
                var relatedAttachmentResult = await _collectionResolver.ResolveCollections(parentEntityMetadata, attachmentComposition, parentData, search);
                restAttachmentResult.ResultList = restAttachmentResult.ResultList.Concat(relatedAttachmentResult["attachment_"].ResultList).ToList();
            } else {
                // paginated request: needs to include related and/or update pagination
                var relatedAttachmentSearchDto = (PaginatedSearchRequestDto)search.ShallowCopy();
                var relatedAttachmentsCount = 0;

                if (restAttachmentResult.ResultList.Count < restAttachmentPageResult.PageSize) {
                    // still has room to add (pagesize - restresult.length) related attachments
                    relatedAttachmentSearchDto.PageSize = relatedAttachmentSearchDto.PageSize - restAttachmentResult.ResultList.Count;

                    var relatedAttachmentResult =await _collectionResolver.ResolveCollections(parentEntityMetadata, attachmentComposition, parentData, relatedAttachmentSearchDto);

                    var relatedAttachments = relatedAttachmentResult["attachment_"];
                    restAttachmentResult.ResultList = restAttachmentResult.ResultList.Concat(relatedAttachments.ResultList).ToList();

                    if (relatedAttachments.PaginationData != null) { // always true
                        relatedAttachmentsCount = relatedAttachments.PaginationData.TotalCount;
                    }

                } else if (restAttachmentResult.ResultList.Count == restAttachmentPageResult.PageSize) {
                    // no room to add related attachments but needs to update pagination
                    // whereclause configured to not include owned by current sr (see #IncludeSelfInRelatedAttachments)
                    var whereClause = ServiceRequestWhereClauseProvider.RelatedAttachmentsWhereClause(parentData, false);
                    relatedAttachmentSearchDto.SearchValues = null;
                    relatedAttachmentSearchDto.SearchParams = null;
                    relatedAttachmentSearchDto.AppendWhereClause(whereClause);
                    relatedAttachmentsCount = await _entityRepository.Count(MetadataProvider.Entity("DOCLINKS"), relatedAttachmentSearchDto);
                }

                // recalculate pagination given new total
                restAttachmentResult.PaginationData = new PaginatedSearchRequestDto(
                    restAttachmentPageResult.TotalCount + relatedAttachmentsCount,
                    restAttachmentPageResult.PageNumber,
                    restAttachmentPageResult.PageSize,
                    restAttachmentPageResult.SearchValues,
                    restAttachmentPageResult.PaginationOptions
                );
            }

            // restoring id
            parentData[parentEntityMetadata.IdFieldName] = ticketuid;
        }
    }

}