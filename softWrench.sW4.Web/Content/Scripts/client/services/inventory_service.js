var app = angular.module('sw_layout');

app.factory('inventoryService', function ($http, contextService, redirectService, searchService) {
    var createInvUse = function(schema, useType) {
        var invuse = {};
        invuse.usetype = useType;
        contextService.insertIntoContext("invuse", invuse, false);
        redirectService.goToApplicationView("invuse", "newdetail", "Input", null, null, null);
    };
    return {
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
        submitTransfer: function (schema, datamap) {
            // Save transfer
            
            // Redirect to the matrectrans grid
            redirectService.goToApplicationView("matrectransTransfers", "list", null, null, null, null);
        },
        cancelTransfer: function () {
            redirectService.goToApplicationView("matrectransTransfers", "list", null, null, null, null);
        }
    };

});