﻿using System;
using System.Collections.Generic;
using softwrench.sw4.batch.api.entities;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action.dto;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data;
using softWrench.sW4.Security.Services;
using BT = softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action.dto.FirstSolarBatchType;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action.util {
    class FirstSolarDatamapConverterUtil {


        public static BatchItem BuildBatchItem(KeyValuePair<string, BatchSpecificData> specificDataEntry, BatchSubmissionData submissionData, FirstSolarBatchType batchType) {
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
            fields["orgid"] = sharedData.OrgId;
            fields["reportedby"] = SecurityFacade.CurrentUser().MaximoPersonId;

            fields["classstructureid"] = sharedData.Classificationid;
            fields["ld_.ldtext"] = sharedData.Details;
            fields["worktype"] = sharedData.WorkType;

            var keyFieldName = batchType.GetUserIdName();

            fields[keyFieldName] = specificDataEntry.Key;

            item.Fields = fields;

            var specificData = specificDataEntry.Value;
            if (specificData != null) {
                //if there´s specific data being passed, let´s use it
                fields["DESCRIPTION"] = specificData.Summary ?? sharedData.Summary;
                fields["siteid"] = specificData.SiteId ?? sharedData.SiteId;
                fields["orgid"] = specificData.OrgId ?? sharedData.OrgId;
                fields["DESCRIPTION_LONGDESCRIPTION"] = specificData.Details ?? sharedData.Details;
                if (specificData.Classification != null) {
                    fields["classstructureid"] = specificData.Classificationid;
                }
                fields["worktype"] = specificData.WorkType ?? sharedData.WorkType;
                //asset batches will also specify the location of the item (which should be the same location as the asset itself)
                var assetBatchSpecificData = specificData as AssetBatchSpecificData;
                if (assetBatchSpecificData != null) {
                    fields["location"] = assetBatchSpecificData.Location;
                }

            }

            return item;
        }


        public static DataMap DoGetDataMap(MultiValueAssociationOption item, BatchSharedData batchSharedData, IDictionary<string, List<string>> warningIds, int transientId, string userIdFieldName) {
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

            fields["summary"] = batchSharedData.Summary;
            fields["siteid"] = batchSharedData.SiteId;
            fields["details"] = batchSharedData.Details;
            fields["classificationid"] = batchSharedData.Classification.Value;
            fields["classification"] = batchSharedData.Classification.Label;
            fields[userIdFieldName] = item.Value;
            fields["userIdFieldName"] = userIdFieldName;
            fields["specificLabel"] = string.IsNullOrEmpty(item.Label) ? item.Value : item.Label;
            fields["worktype"] = batchSharedData.WorkType;
            var multiValueOption = item;

            if (multiValueOption.Extrafields != null) {
                if (multiValueOption.Extrafields.ContainsKey("siteid")) {
                    fields["siteid"] = multiValueOption.Extrafields["siteid"];
                }
                if (multiValueOption.Extrafields.ContainsKey("orgid")) {
                    fields["orgid"] = multiValueOption.Extrafields["orgid"];
                }
                if (multiValueOption.Extrafields.ContainsKey("location")) {
                    fields["location"] = multiValueOption.Extrafields["location"];
                }
            }
            return new DataMap("workorder", fields);
        }

    }
}