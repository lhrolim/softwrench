(function (angular) {
    'use strict';

    var module = angular.module('sw_layout');

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

            var user = contextService.getUserData();
            var parentIdFieldName = parameters.parentSchema.idFieldName;

            var httpParameters = {
                application: parameters.parentSchema.applicationName,
                applicationItemId: parameters.parentData["fields"][parentIdFieldName],
                userId: user.dbId,
                commlogId: parameters.compositionItemId
            };

            parameters.compositionItemData["read"] = true;

            restService.invokePost("Commlog", "UpdateReadFlag", httpParameters, null, null, function () {
                parameters.compositionItemData["read"] = false;
            });
        };

        function formatCommTemplate(parameters) {
            var parentSchema = parameters.scope.parentschema || /* when commlog being edited in modal */ crudContextHolderService.currentSchema();

            var parentIdFieldName = parentSchema.idFieldName;
            var parentData = parameters.parentdata["fields"];
            let templateId = parameters.fields['#templateid'];

            if (templateId == null) {
                return;
            }

            var httpParameters = {
                templateId: templateId,
                json: parentData,
                schemaId: parentSchema.schemaId,
                applicationName: parentSchema.applicationName,
                applicationItemId: parentData[parentIdFieldName]
            };


            restService.invokePost("CommTemplate", "MergeTemplateDefinition", null, angular.toJson(httpParameters), function (data) {
                parameters.fields['subject'] = data.resultObject.subject;
                parameters.fields['message'] = richTextService.getDecodedValue(data.resultObject.message);
                parameters.fields['sendto'] = [parameters.parentdata['fields']['reportedemail']];
            }, null);
        };

        var addSignature = function (parameters) {
            var datamap = crudContextHolderService.rootDataMap("#modal");
            if (datamap.message == undefined) {
                var signature = contextService.getUserData().signature;
                datamap['message'] = signature !== "" ? "<br/><br/>" + signature : signature;
            }
        }

        var buildDetailsHtml = function (commlogDatamap) {

            var root = $("<html></html>");
            var head = $("<head></head>");
            var styles = $("style, link[rel='stylesheet']").clone();
            styles.each(function (index, el) {
                head.append(el);
            });
            root.append(head);

            var body = $("<body></body>");
            body.addClass("pdf-root");
            body.append($("#printsectionform")[0].outerHTML);
            root.append(body);

            commlogDatamap["detailsHtml"] = root[0].outerHTML;
            applicationService.save();
        }

        var send = function (commLogDatamap, commLogSchema) {
            var safeCommLogDatamap = commLogDatamap.fields ? commLogDatamap.fields : commLogDatamap;
            var extraAtach = safeCommLogDatamap["extraattachments"];
            if (!extraAtach || extraAtach !== "details") {
                applicationService.save();
                return;
            }

            var schema = crudContextHolderService.currentSchema();
            var datamap = crudContextHolderService.rootDataMap();

            var printCallback = function () {
                buildDetailsHtml(safeCommLogDatamap);
            }

            var printOptions = {
                shouldPageBreak: false,
                shouldPrintMain: true,
                printCallback: printCallback
            };
            printService.printDetail(schema, datamap, printOptions);
        }

        function buildReplyAllSendTo(origTo, origFrom, newFrom) {
            var transFrom = nullOrCommaSplit(origFrom);
            var transTo = nullOrCommaSplit(origTo);
            var newTo = transFrom.concat(transTo);
            var userAddressIndex = newTo.indexOf(newFrom);
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
            var preferences = contextService.getUserData().userPreferences;
            var signature = preferences == null ? "" : preferences.signature;
            return commlogMessageheader.format(signature, originalItem.sendfrom, originalItem.sendto, emptyIfNull(originalItem.cc), originalItem.subject, originalItem.message);
        };

        function commonstransform(originalItem, replyMode) {
            normalizeOriginal(originalItem);
            var displayables = originalItem.schema.displayables;
            var clonedItem = fieldService.fillDefaultValues(displayables, { commloguid: null }, null);

            // If KOGT, set subject to null so that the default subject from metadata will populate
            var client = contextService.client();
            if (client != null && client.equalIc("kongsberg") && originalItem['ownertable'].equalIc("SR")) {
                clonedItem['subject'] = null;
            } else {
                var subjectPrefix = replyMode ? "Re: " : "Fw: ";
                clonedItem['subject'] = subjectPrefix + originalItem.subject;
            }

            // if there was a default value marked for the sendfrom it shall be used, otherwise fallinback to user default email
            clonedItem['sendfrom'] = clonedItem['sendfrom'] ? clonedItem['sendfrom'] : contextService.getUserData().email;
            clonedItem['cc'] = nullOrCommaSplit(originalItem.cc);
            clonedItem['message'] = buildMessage(originalItem);
            clonedItem['createdate'] = fieldService.currentDate();
            return clonedItem;
        };

        function getServerData(commloglistitem, schema) {
            return applicationService.getApplicationDataPromise("commlog", "detail", { id: commloglistitem["commloguid"] })
                .then(function (result) {
                    result.data.resultObject.fields.schema = schema;
                    return result.data.resultObject.fields;
                });
        };

        function transformReply(originalItem) {
            var detailItem = commonstransform(originalItem, true);
            detailItem['sendto'] = originalItem.sendfrom.indexOf(",") > -1 ? originalItem.sendfrom.split(',') : [originalItem.sendfrom];
            return detailItem;
        };

        function transformReplyAll(originalItem) {
            var detailItem = commonstransform(originalItem, true);
            detailItem['sendto'] = buildReplyAllSendTo(originalItem.sendto, originalItem.sendfrom, detailItem['sendfrom']);
            return detailItem;
        };

        function transformForward(originalItem) {
            var detailItem = commonstransform(originalItem, false);
            detailItem['sendto'] = detailItem['cc'] = null;
            return detailItem;
        };

        function eventDispatcher(title) {
            return function(clonedItem) {
                return dispatchEvent(clonedItem, title);
            }
        };

        function dispatchEvent(clonedItem, title) {
            $rootScope.$broadcast("sw.composition.edit", clonedItem, title, true)
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

        var addCommlog = function (parameters) {
            $q.when(contextService.getUserData().email || restService.getPromise("User", "GetPrimaryEmail")).then(function (result) {
                var email = isString(result) ? result : result.data;

                if (!email || "null".equalIc(email)) {
                    alertService.confirm2('The current user does not have an email registered. Do you want define it now?')
                        .then(() => redirectService.goToApplicationView("Person", 'myprofiledetail', 'input', null, { userid: contextService.getUserData().maximoPersonId }, null))
                        .then(() => redirectService.redirectToTab('email_'));
                } else {
                    var datamap = {
                        _iscreation: true
                    };

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