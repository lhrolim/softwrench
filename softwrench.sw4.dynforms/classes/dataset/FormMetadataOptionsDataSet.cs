using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.portable.Util;
using softwrench.sw4.dynforms.classes.model.entity;
using softwrench.sw4.dynforms.classes.model.metadata;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Parsing;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Util;

namespace softwrench.sw4.dynforms.classes.dataset {

    public class FormMetadataOptionsDataSet : SWDBApplicationDataset {


        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var detail = await base.GetApplicationDetail(application, user, request);
            var options = new FormMetadataOptions();
            options = EntityBuilder.PopulateTypedEntity(detail.ResultObject, options);
            detail.ResultObject.SetAttribute("#jsonList", options.ListDefinitionStringValue);

            return detail;
        }


        /// <summary>
        /// Method for saving a form definition
        /// </summary>
        /// <param name="operationWrapper"></param>
        /// <returns></returns>
        [Transactional(DBType.Swdb)]
        public override async Task<TargetResult> DoExecute(OperationWrapper operationWrapper) {

            var options = GetOrCreate<FormMetadataOptions>(operationWrapper, true);
            options.ListDefinitionStringValue = operationWrapper.GetStringAttribute("#jsonList");
            options = await SWDAO.SaveAsync(options);

            return new TargetResult(options.Id.ToString(), options.Alias, null);
        }

        public IEnumerable<IAssociationOption> GetAvailableOptions(OptionFieldProviderParameters parameters) {
            var items = SWDAO.FindByNativeQuery(FormMetadataOptions.AliasQuery);
            return items.Select(i => new AssociationOption(i["id"], i["alias"]));
        }




        public override string ApplicationName() {
            return "_formmetadataoptions";
        }
    }
}
