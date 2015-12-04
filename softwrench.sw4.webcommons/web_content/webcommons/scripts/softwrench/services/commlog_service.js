(function(angular) {
    'use strict';

    function commlogService($http, contextService, restService, richTextService) {
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

            restService.invokePost("Commlog", "UpdateReadFlag", httpParameters, null, null, function() {
                parameters.compositionItemData["read"] = false;
            });
        };

        function formatCommTemplate(parameters) {
            var parentSchema = parameters.scope.parentschema;

            var parentIdFieldName = parentSchema.idFieldName;
            var parentData = parameters.parentdata["fields"];

            var httpParameters = {
                templateId: parameters.fields['#templateid'],
                json: parentData,
                schemaId: parentSchema.schemaId,
                applicationName: parentSchema.applicationName,
                applicationItemId: parentData[parentIdFieldName]
            };


            restService.invokePost("CommTemplate", "MergeTemplateDefinition", null, angular.toJson(httpParameters), function(data) {
                parameters.fields['subject'] = data.resultObject.subject;
                parameters.fields['message'] = richTextService.getDecodedValue(data.resultObject.message);
                parameters.fields['sendto'] = [parameters.parentdata['fields']['reportedemail']];
            }, null);
        };

        var addSignature = function(parameters) {
            if (parameters.datamap.message == undefined) {
                var signature = contextService.getUserData().signature;
                parameters.datamap['message'] = signature != "" ? "<br/><br/>" + signature : signature;
            }
        }

        var getSubjectDefaultExpression = function (datamap, schema, displayable) {
            var client = contextService.client();
            if (client.equalIc("kongsberg") && datamap['ownertable'].equalIc("SR")) {
                return "'##' + $.parentdata.fields['ticketid'] + '## ' + $.parentdata.fields['description']";
            }
            return "";
        }

        var service = {
            formatCommTemplate: formatCommTemplate,
            updatereadflag: updatereadflag,
            addSignature: addSignature,
            getSubjectDefaultExpression: getSubjectDefaultExpression
        };

        return service;
    }

    angular.module("sw_layout").factory('commlogService', ['$http', 'contextService', 'restService', 'richTextService', commlogService]);


})(angular);