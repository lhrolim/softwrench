using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using cts.commons.web.Controller;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using System.Collections.Async;
using cts.commons.portable.Util;
using log4net;
using Quartz.Util;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action {


    //TODO: make FirstSolar agnostic...
    [System.Web.Mvc.Authorize]
    public class FileExplorerController : FileDownloadController {

        private const int MaxThreads = 5;


        private readonly AttachmentHandler _attachmentHandler;
        private readonly EntityRepository _repository;

        private readonly ILog Log = LogManager.GetLogger(typeof(FileExplorerController));


        public FileExplorerController(AttachmentHandler attachmentHandler, EntityRepository repository) {
            _attachmentHandler = attachmentHandler;
            _repository = repository;
        }

        public async Task<FileContentResult> DownloadAll(string userId, string ownerId, string ownerTable, string relationship) {

            if (ownerId.NullOrEmpty() || ownerTable.NullOrEmpty()) {
                return null;
            }

            if (relationship == null) {
                //bringing it all for a given workorder
                relationship = "swwpkg:";
            } else if (!relationship.StartsWith("swwpkg:")) {
                //relationships are set on the form #xxxfileexplorer_... TODO: make it generic
                relationship = relationship.Substring(1);
                relationship = relationship.Replace("fileexplorer_", "");
                relationship = "swwpkg:" + relationship;
            }

            var dto = new SearchRequestDto();
            dto.AppendSearchEntry("ownertable", ownerTable);
            //NO Idea why, but this was triggering a bug... the parameter was never replaced at the query. tracked down to hibernate level which seemed correct... very weird
            dto.AppendSearchEntry("ownerid", ownerId);
            dto.AppendSearchEntry("docinfo_.urlparam1", relationship + "%");
            //            dto.AppendWhereClauseFormat("ownerid = '{0}' ", ownerId);

            dto.AppendProjectionFields("docinfo_.docinfoid");

            var dict = await _repository.GetAsRawDictionary(MetadataProvider.Entity("DOCLINKS"), dto);


            var results = new List<Tuple<byte[], string>>();

            await dict.ResultList.ParallelForEachAsync(
             async attachment => {
                 var fileTuple = await _attachmentHandler.DownloadViaHttpById(attachment["docinfo_.docinfoid"].ToString());
                 results.Add(fileTuple);
             }, MaxThreads, false);

            if (!results.Any()) {
                Log.InfoFormat("no files found for section {0}, nothing downloaded", relationship);
                return null;
            }


            Log.InfoFormat("generating zip file containing {0} files for section {1}", results.Count, relationship);
            var zipStream = FileUtils.GenerateZip(results);

            var result = new FileContentResult(zipStream, System.Net.Mime.MediaTypeNames.Application.Zip) {
                FileDownloadName = "{0}-{1}.zip".Fmt(relationship, userId)
            };
            return result;

        }

    }
}
