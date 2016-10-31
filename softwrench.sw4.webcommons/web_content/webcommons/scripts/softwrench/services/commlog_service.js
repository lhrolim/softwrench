(function (angular) {
    'use strict';
    const module = angular.module('sw_layout');
    module.constant("commlog_messagheader",
        //above this limit framework shall no longer produce the full rowstamp map, but rather just pass the maxrowstamp to the server
        "<br/><br/>{0}" +
        "________________________________________________________________________________________________________" +
        "<br/><b>From:</b> {1}" +
        "<br/><b>To:</b> {2}" +
        "<br/><b>Cc:</b> {3}" +
        "<br/><b>Subject: </b> {4}" +
        "<br/><br/>{5}"
    );



    function commlogService($rootScope, $http, $q, contextService, restService, richTextService, crudContextHolderService, applicationService, printService, fieldService, commlogMessageheader, alertService, redirectService) {
       
        function updatereadflag(parameters) {
            if (parameters.compositionItemData["read"]) {
                return;
            }
            const user = contextService.getUserData();
            const parentIdFieldName = parameters.parentSchema.idFieldName;
            const httpParameters = {
                application: parameters.parentSchema.applicationName,
                applicationItemId: parameters.parentData[parentIdFieldName],
                userId: user.dbId,
                commlogId: parameters.compositionItemId
            };
            parameters.compositionItemData["read"] = true;

            restService.invokePost("Commlog", "UpdateReadFlag", httpParameters, null, null, function () {
                parameters.compositionItemData["read"] = false;
            });
        };

        function formatCommTemplate(parameters) {
            const parentSchema = parameters.scope.parentschema || /* when commlog being edited in modal */ crudContextHolderService.currentSchema();
            const parentIdFieldName = parentSchema.idFieldName;
            const parentData = parameters.parentdata;
            const templateId = parameters.fields['#templateid'];
            if (templateId == null) {
                return;
            }
            const httpParameters = {
                templateId: templateId,
                json: parentData,
                schemaId: parentSchema.schemaId,
                applicationName: parentSchema.applicationName,
                applicationItemId: parentData[parentIdFieldName]
            };
            restService.invokePost("CommTemplate", "MergeTemplateDefinition", null, angular.toJson(httpParameters), function (data) {
                parameters.fields['subject'] = data.resultObject.subject;
                parameters.fields['message'] = richTextService.getDecodedValue(data.resultObject.message);
                parameters.fields['sendto'] = [parameters.parentdata['reportedemail']];
            }, null);
        };

        const addSignature = function () {
            const datamap = crudContextHolderService.rootDataMap("#modal");
            if (datamap.message == undefined) {
                const signature = contextService.getUserData().signature || "";
                datamap['message'] = signature !== "" ? "<br/><br/>" + signature : signature;
            }
        };
        var buildDetailsHtml = function (commlogDatamap) {
            const root = $("<html></html>");
            var head = $("<head></head>");
            const styles = $("style, link[rel='stylesheet']").clone();
            styles.each(function (index, el) {
                head.append(el);
            });
            root.append(head);
            const body = $("<body></body>");
            body.addClass("pdf-root");
            body.append($("#printsectionform")[0].outerHTML);
            root.append(body);

            commlogDatamap["detailsHtml"] = root[0].outerHTML;
            applicationService.save();
        }
        const send = function (commLogDatamap, commLogSchema) {
            var safeCommLogDatamap = commLogDatamap;
            const extraAtach = safeCommLogDatamap["extraattachments"];
            if (!extraAtach || extraAtach !== "details") {
                applicationService.save();
                return;
            }
            const schema = crudContextHolderService.currentSchema();
            const datamap = crudContextHolderService.rootDataMap();
            const printCallback = function () {
                buildDetailsHtml(safeCommLogDatamap);
            };
            const printOptions = {
                shouldPageBreak: false,
                shouldPrintMain: true,
                printCallback: printCallback
            };
            printService.printDetail(schema, datamap, printOptions);
        };

        function buildReplyAllSendTo(origTo, origFrom, newFrom) {
            const transFrom = nullOrCommaSplit(origFrom);
            const transTo = nullOrCommaSplit(origTo);
            const newTo = transFrom.concat(transTo);
            const userAddressIndex = newTo.indexOf(newFrom);
            if (userAddressIndex > -1) {
                newTo.splice(userAddressIndex, 1);
            }
            return newTo;
        };

        function normalizeOriginal(originalItem, subject) {
            originalItem["sendfrom"] = emptyIfNull(originalItem["sendfrom"]);
            originalItem["sendto"] = emptyIfNull(originalItem["sendto"]);
            originalItem["cc"] = emptyIfNull(originalItem["cc"]);
            originalItem["subject"] = emptyIfNull(originalItem['subject']);
            originalItem["message"] = emptyIfNull(originalItem['message']);
            return originalItem;
        };

        function buildMessage(originalItem) {
            const preferences = contextService.getUserData().userPreferences;
            const signature = preferences == null ? "" : preferences.signature;
            return commlogMessageheader.format(signature, originalItem.sendfrom, originalItem.sendto, emptyIfNull(originalItem.cc), originalItem.subject, originalItem.message);
        };

        function commonstransform(originalItem, replyMode) {
            normalizeOriginal(originalItem);
            const displayables = originalItem.schema.displayables;
            const clonedItem = fieldService.fillDefaultValues(displayables, { commloguid: null }, null);

            // If KOGT, set subject and from to null so that the default subject from metadata will populate
            const client = contextService.client();
            if (client != null && client.equalIc("kongsberg") && originalItem['ownertable'].equalIc("SR")) {
                clonedItem['subject'] = null;
                clonedItem['sendfrom'] = null;
            } else {
                const subjectPrefix = replyMode ? "Re: " : "Fw: ";
                clonedItem['subject'] = subjectPrefix + originalItem.subject;

                // if there was a default value marked for the sendfrom it shall be used, otherwise fallinback to user default email
                clonedItem['sendfrom'] = clonedItem['sendfrom'] ? clonedItem['sendfrom'] : contextService.getUserData().email;
            }

            clonedItem['cc'] = nullOrCommaSplit(originalItem.cc);
            clonedItem['message'] = buildMessage(originalItem);
            clonedItem['createdate'] = fieldService.currentDate();
            clonedItem['newattachment_path'] = "";
            clonedItem['attachments'] = originalItem.attachments;
            return clonedItem;
        };

        function getServerData(commloglistitem, schema) {
            return applicationService.getApplicationDataPromise("commlog", "detail", { id: commloglistitem["commloguid"] })
                .then(function (result) {
                    result.data.resultObject.schema = schema;
                    return result.data.resultObject;
                });
        };

        function transformReply(originalItem) {
            const detailItem = commonstransform(originalItem, true);
            detailItem['sendto'] = originalItem.sendfrom.indexOf(",") > -1 ? originalItem.sendfrom.split(',') : [originalItem.sendfrom];            
            return detailItem;
        };

        function transformReplyAll(originalItem) {
            const detailItem = commonstransform(originalItem, true);
            detailItem['sendto'] = buildReplyAllSendTo(originalItem.sendto, originalItem.sendfrom, detailItem['sendfrom']);
            return detailItem;
        };

        function transformForward(originalItem) {
            const detailItem = commonstransform(originalItem, false);
            detailItem['sendto'] = detailItem['cc'] = null;
            return detailItem;
        };

        function eventDispatcher(title) {
            return function(clonedItem) {
                return dispatchEvent(clonedItem, title);
            }
        };

        function dispatchEvent(clonedItem, title) {
            $rootScope.$broadcast("sw.composition.edit", "commlog", clonedItem, title, true);
            return clonedItem;
        };

        function reply(schema, commLogDatamap) {
            return getServerData(commLogDatamap, schema)
                .then(transformReply)
               .then(eventDispatcher("Reply"));
        };

        function replyAll(schema, commLogDatamap) {
            return getServerData(commLogDatamap, schema)
               .then(transformReplyAll)
               .then(eventDispatcher("Reply All"));
        };

        function forward(schema, commLogDatamap) {
            return getServerData(commLogDatamap, schema)
                .then(transformForward)
                .then(eventDispatcher("Forward"));
        };

        const addCommlog = function (parameters) {
            const datamap = {
                _iscreation: true,
                attachment: [],
                newattachment_path: ''
            };
            if (contextService.getUserData().email) {
                return dispatchEvent(datamap, "Communication Details");
            }

            //not on client side user
            $q.when(restService.getPromise("UserProfile", "GetPrimaryEmail")).then(function (result) {
                const email = isString(result) ? result : result.data;
                if (!email || "null".equalIc(email)) {
                    alertService.confirm('The current user does not have an email registered. Do you want define it now?')
                        .then(() => redirectService.goToApplicationView("Person", 'myprofiledetail', 'input', null, { userid: contextService.getUserData().maximoPersonId }, null))
                        .then(() => redirectService.redirectToTab('email_'));
                } else {
                    //update client side user with returned email
                    contextService.getUserData().email = email;
                    dispatchEvent(datamap, "Communication Details");
                }
            });
        };
        const service = {
            addCommlog,
            formatCommTemplate,
            updatereadflag,
            addSignature,
            send,
            reply,
            replyAll,
            forward
        };

        return service;
    }

    angular.module("sw_layout").factory("commlogService", ["$rootScope", "$http", "$q", "contextService", "restService", "richTextService", "crudContextHolderService", "applicationService", "printService", "fieldService", "commlog_messagheader", "alertService", "redirectService", commlogService]);


})(angular);