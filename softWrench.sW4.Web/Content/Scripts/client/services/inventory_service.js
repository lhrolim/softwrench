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
            var location = parameters.fields['workorder_.location'];
            parameters.fields['location'] = location;
            var assetnum = parameters.fields['workorder_.assetnum'];
            parameters.fields['assetnum'] = assetnum;
        },
        editinvissuewo: function(parameters) {
            var datamap = {};
            datamap['#assetnum'] = "11250";
            datamap['#issueto'] = "SINCLAIR";
            datamap['#issuetype'] = "RETURN";
            datamap['#location'] = "BR200";
            datamap['#refwo'] = "43079";
            datamap['#storeloc'] = "CENTRAL";
            datamap['invissue_'] = [];

            var param = {};
            param.id = "43079";
            redirectService.goToApplicationView('invissuewo', 'viewdetail', null, null, param, datamap);
        }

    };

});