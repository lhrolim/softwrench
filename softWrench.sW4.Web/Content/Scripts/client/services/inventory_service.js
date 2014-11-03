var app = angular.module('sw_layout');

app.factory('inventoryService', function ($http, contextService, redirectService, searchService, submitService) {
    var createTransaction = function (schema, issueType) {
        var matusetrans = {};
        matusetrans.issueType = issueType;
        contextService.insertIntoContext("matusetrans", matusetrans, false);
        redirectService.goToApplicationView(schema.applicationName, "detail", "Input", null, null, null);
    };
    var createInvUse = function(schema, useType) {
        var invuse = {};
        invuse.usetype = useType;
        contextService.insertIntoContext("invuse", invuse, false);
        redirectService.goToApplicationView("invuse", "newdetail", "Input", null, null, null);
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
        editinvissuewo: function (parameters) {
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
        },
        createTransfer: function(schema) {
            if (schema === undefined) {
                return;
            }
            createInvUse(schema, "TRANSFER");
        },
        afterChangeLocation: function (parameters) {
            var searchData = {
                itemnum: parameters['fields']['invuseline_.itemnum'],
                location: parameters['fields']['fromstoreloc']
            }
            var searchDTO = searchService.buildSearchDTO(searchData, {}, {}, null);
            searchDTO.pageNumber = 1;
            searchDTO.totalCount = 0;
            searchDTO.pageSize = 30;
            var restParameters = {
                key: {
                    schemaId: "list",
                    mode: "none",
                    platform: "web"
                },
                SearchDTO: searchDTO
            }
            var urlToUse = url("/api/Data/invcost?" + $.param(restParameters));
            $http.get(urlToUse).success(function (data) {
                var resultObject = data.resultObject;
                var fields = resultObject[0].fields;
                var costtype = parameters['fields']['inventory_.costtype'];
                if (costtype === 'STANDARD')
                {
                    parameters.fields['invuseline_.unitcost'] = fields.stdcost;
                }
                else if (costtype === 'AVERAGE') {
                    parameters.fields['invuseline_.unitcost'] = fields.avgcost;
                }
            });
        },
        getBinQuantity: function (parameters) {
            var searchData = {
                itemnum: parameters['fields']['invuseline_.itemnum'],
                siteid: parameters['fields']['inventory_.siteid'],
                itemsetid: parameters['fields']['inventory_.itemsetid'],
                location: parameters['fields']['fromstoreloc'],
                binnum: parameters['fields']['invuseline_.frombin']
            }
            var searchDTO = searchService.buildSearchDTO(searchData, {}, {}, null);
            searchDTO.pageNumber = 1;
            searchDTO.totalCount = 0;
            searchDTO.pageSize = 30;
            var restParameters = {
                key: {
                    schemaId: "list",
                    mode: "none",
                    platform: "web"
                },
                SearchDTO: searchDTO
            }
            var urlToUse = url("/api/Data/invbalances?" + $.param(restParameters));
            $http.get(urlToUse).success(function (data) {
                var resultObject = data.resultObject;
                var fields = resultObject[0].fields;
                parameters.fields['#curbal'] = fields.curbal;
            });
        },
        submitTransfer: function (schema, datamap, savefn) {
            // Save transfer
            savefn();
            // Redirect to the matrectrans grid
            redirectService.goToApplicationView("matrectransTransfers", "list", null, null, null);
        },
        cancelTransfer: function () {
            redirectService.goToApplicationView("matrectransTransfers", "list", null, null, null, null);
        }
    };

});