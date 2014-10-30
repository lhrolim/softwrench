var app = angular.module('sw_layout');

app.factory('scannerdetectionService', function ($http, $rootScope, restService, searchService, redirectService, contextService) {

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
            
            $(document).scannerDetection(function (data) {
                var lookupData = searchService.getSearchOperationById(data);

                var itemExist = false;
                for (var key in parameters.clonedCompositionData) {
                    var item = parameters.clonedCompositionData[key];
                    if (item['matusetransid'] == null) {
                        if (item['itemnum'] == data) {
                            item['quantity']++;
                            itemExist = true;
                        }
                    }
                }

                if (!itemExist) {
                    var newRecord = {};
                    newRecord['itemnum'] = data;
                    newRecord['quantity'] = 1;
                    newRecord['item_.description'] = "Z RAGGIN IT UP";
                    newRecord['issuetype'] = "ISSUE";
                    newRecord['matusetransid'] = null;
                    newRecord['assetnum'] = "11250";
                    newRecord['issueto'] = "SINCLAIR";
                    newRecord['location'] = "BR2000";
                    newRecord['refwo'] = "43079";
                    newRecord['siteid'] = "BEDFORD";
                    newRecord['storeloc'] = "CENTRAL";
                    parameters.clonedCompositionData.push(newRecord);
                }
                
                redirectService.redirectToTab('invissue_');

            });
        },
        
    };

});


