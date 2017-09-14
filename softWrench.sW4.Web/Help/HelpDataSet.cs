using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using cts.commons.Util;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Help;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;

namespace softWrench.sW4.Web.Help {
    public class HelpDataSet : SWDBApplicationDataset {
        public override async Task<TargetResult> DoExecute(OperationWrapper operationWrapper) {
            var crudData = (CrudOperationData)operationWrapper.GetOperationData;
            var entry = EntityBuilder.PopulateTypedEntity(crudData, new HelpEntry());
            var base64Data = crudData.GetUnMappedAttribute("newattachment");
            if (base64Data != null) {
                var bytes = AttachmentHandler.FromBase64ToByteArray(base64Data);
                entry.Data = CompressionUtil.Compress(bytes);
                entry.DocumentName = crudData.GetUnMappedAttribute("newattachment_path");
                if (entry.EntryLabel == null) {
                    entry.EntryLabel = entry.DocumentName;
                }
            }
            var item = await SWDAO.SaveAsync(entry);
            var targetResult = new TargetResult(item.Id.ToString(), null, null);
            return targetResult;
        }

        public override string ApplicationName() {
            return "_SoftwrenchHelp";
        }
    }
}