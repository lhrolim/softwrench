var app = angular.module('sw_layout');

app.factory('inventoryService', function ($http, contextService, redirectService) {
    var createTransaction = function (schema, issueType) {
        var matusetrans = {};
        matusetrans.issueType = issueType;
        contextService.insertIntoContext("matusetrans", matusetrans, false);
        redirectService.goToApplicationView(schema.applicationName, "detail", "Input", null, null, null);
    };
    return {
        createIssue: function (schema) {
            if (schema === undefined) {
                return;
            }

            createTransaction(schema, "ISSUE");
        },
        createReturn: function (schema) {
            if (schema === undefined) {
                return;
            }

            createTransaction(schema, "RETURN");
        },
        afterchangeworkorder: function (parameters) {
            var fields = parameters.fields;
            parameters.scope.datamap["location"] = fields["workorder_.location"];
            parameters.scope.datamap["assetnum"] = fields["workorder_.assetnum"];
        }

    };

});