var app = angular.module('sw_layout');

app.factory('commlogService', function ($http, contextService, restService) {

    return {
        updatereadflag: function (parameters) {

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
        },

        formatCommTemplate: function (parameters) {
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

            
            restService.invokePost("CommTemplate", "MergeTemplateDefinition", null, angular.toJson(httpParameters), function (data) {
                parameters.fields['subject'] = data.resultObject.subject;
                parameters.fields['message'] = data.resultObject.message;
            }, null);
        }
    };

});