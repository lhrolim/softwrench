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
            var param = {};
            

            datamap['#assetnum'] = "11250";
            datamap['#issueto'] = "SINCLAIR";
            datamap['#issuetype'] = "RETURN";
            datamap['#location'] = "BR200";
            datamap['#refwo'] = "43079";
            datamap['#storeloc'] = "CENTRAL";
            datamap['#test'] = "test";
            param.resultObject = datamap;
            
            redirectService.goToApplicationView('invissuewo', 'viewdetail', null, null, null, datamap);
        }

    };

});