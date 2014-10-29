var app = angular.module('sw_layout');

app.factory('scannerdetectionService', function ($http, $rootScope, restService, searchService, contextService) {

    return {
        initInventoryGridListener: function (schema, datamap, parameters) {
            var searchData = parameters.searchData;

            $(document).scannerDetection(function (data) {
                var scanOrderString = contextService.scanOrder();
                var scanOrder = scanOrderString.split(",");
                var extraparameters = { keepfilterparameters: true };
                for (var attribute in scanOrder) {
                    var currentAttribute = scanOrder[attribute];
                    // If the property is not already in th escan data, add it and its value
                    if (!searchData.hasOwnProperty(currentAttribute)) {
                        var localSearchData = {};
                        localSearchData[currentAttribute] = data;
                        searchService.refreshGrid(localSearchData, extraparameters);
                        return;
                    // Else if the property exists but is blank, set it to its new value
                    } else if (searchData[currentAttribute] === '') {
                        searchData[currentAttribute] = data;
                        searchService.refreshGrid(searchData, extraparameters);
                        return;
                    }
                }
            });
        },
        initIssueItemListener: function (schema, datamap, parameters) {
            var compData = datamap;
            $(document).scannerDetection(function (data) {

                //var test = compData;
                if (compData === undefined) {
                    compData = {};
                }
                if (compData['fields'] === undefined){
                    compData['fields'] = {};
                    compData['fields']['invissue_'] = {};
                } else {
                    if (compData['fields']['invissue_'] === undefined) {
                        compData['fields']['invissue_'] = [];
                    }
                }
                
                var newRecord = {};
                newRecord['itemnum'] = data;
                newRecord['quantity'] = 1;
                newRecord['description'] = "Z RAGGIN IT UP";

                compData['fields']['invissue_'] = [newRecord];

            });
        },
        
    };

});


