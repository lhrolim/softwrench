(function (angular) {
    'use strict';

    function commlogService($http, contextService, restService, richTextService, crudContextHolderService, applicationService, printService) {
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
            var templateId = parameters.fields['#templateid'];

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

        var service = {
            formatCommTemplate: formatCommTemplate,
            updatereadflag: updatereadflag,
            addSignature: addSignature,
            send: send
        };

        return service;
    }

    angular.module("sw_layout").factory("commlogService", ["$http", "contextService", "restService", "richTextService", "crudContextHolderService", "applicationService", "printService", commlogService]);


})(angular);