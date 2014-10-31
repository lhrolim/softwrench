var app = angular.module('sw_layout');

app.factory('inventoryService', function ($http, contextService, redirectService, modalService) {
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
        editinvissuewo: function (schema, datamap) {
            var newDatamap = {};
            newDatamap['#assetnum'] = datamap['assetnum'];
            newDatamap['#issueto'] = datamap['issueto'];
            newDatamap['#issuetype'] = datamap['issuetype'];
            newDatamap['#location'] = datamap['location'];
            newDatamap['#refwo'] = datamap['refwo'];
            newDatamap['#storeloc'] = datamap['storeloc'];
            newDatamap['invissue_'] = [];

            var param = {};
            param.id = datamap['refwo'];
            redirectService.goToApplicationView('invissuewo', 'newdetail', null, null, param, newDatamap);
        },
        submitNewInvIssue: function (schema, datamap, saveFn) {
            modalService.show(schema.compositiondetailschema, datamap, saveFn);
        },
        cancelNewInvIssue: function () {
            redirectService.goToApplicationView("invissue", "invissuelist", null, null, null, null);
        },
        displayPopupModal: function (datamap, saveFn) {
            
            var restParameters = {
                key: {
                    schemaId: "newitem",
                    mode: "input",
                    platform: "web"
                }
            }

            var urlToUse = url("/api/Data/invissue?" + $.param(restParameters));
            $http.get(urlToUse).success(function (data) {
                var schema = data.schema;
                modalService.show(schema, datamap, saveFn);
            });
        },
        cancelNewInvIssueItem: function () {
            redirectService.goToApplicationView("invissue", "invissuelist", null, null, null, null);
        },

    };
});