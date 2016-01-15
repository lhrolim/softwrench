using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action.dto;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action.util {
    class FirstSolarDatamapConverterUtil {


        public static BatchItem BuildBatchItem(KeyValuePair<string, BatchData> location, LocationBatchSubmissionData submissionData) {
            var item = new BatchItem {
                Application = "workorder",
                Operation = "crud_create",
                Status = BatchStatus.SUBMITTING,
                UpdateDate = DateTime.Now,
                Schema = "newdetail"
            };

            var sharedData = submissionData.SharedData;

            var fields = new Dictionary<string, object>();
            fields["DESCRIPTION"] = sharedData.Summary;
            fields["siteid"] = sharedData.SiteId;
            fields["classstructureid"] = sharedData.Classification;
            fields["ld_.ldtext"] = sharedData.Details;
            fields["location"] = location.Key;
            item.Fields = fields;

            var specificData = location.Value;
            if (specificData != null) {
                //if there´s specific data being passed, let´s use it
                fields["DESCRIPTION"] = specificData.Summary ?? sharedData.Summary;
                fields["siteid"] = specificData.SiteId ?? sharedData.SiteId;
                fields["DESCRIPTION_LONGDESCRIPTION"] = specificData.Details ?? sharedData.Details;
                fields["classstructureid"] = specificData.Classification ?? sharedData.Classification;
            }

            return item;
        }


        public static DataMap DoGetDataMap(IAssociationOption item, BatchData batchData, IDictionary<string, List<string>> warningIds, int transientId, Tuple<string, string> fieldNames) {
            var selected = !warningIds.ContainsKey(item.Value);
            var fields = new Dictionary<string, object>();
            fields["_#selected"] = selected;
            if (!selected) {
                fields["#wonums"] = string.Join(",", warningIds[item.Value]);
            }
            //if not selected, let´s put a warning for the user
            fields["#warning"] = !selected;

            //this id is needed in order for the buffer to work properly
            fields["workorderid"] = transientId;

            fields["summary"] = batchData.Summary;
            fields["siteid"] = batchData.SiteId;
            fields["details"] = batchData.Details;
            fields[fieldNames.Item1] = item.Value;
            fields[fieldNames.Item2] = item.Label;
            return new DataMap("workorder", fields);
        }

    }
}
