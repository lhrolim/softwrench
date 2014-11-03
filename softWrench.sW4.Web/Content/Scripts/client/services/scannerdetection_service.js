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
            //This function will look for an item being scanned for the Issue Inventory application. 
            //Scanned items are added to the invissue composition.  
            $(document).scannerDetection(function (data) {
               
                //Checks to see if the item has already been added to the composition list.
                //If the item is found, this will increment the quantity by 1.
                for (var key in parameters.clonedCompositionData) {
                    var item = parameters.clonedCompositionData[key];
                    //Records with a value for the matusetransid are being hidden from the user.
                    if (item['matusetransid'] == null) {
                        if (item['itemnum'] == data) {
                            item['quantity']++;
                            redirectService.redirectToTab('invissue_');
                            return;
                        }
                    }
                }

                //If the item was not found in the composition list, a lookup is made to fetch
                //associated information (description, costtype, etc)
                    var restParameters = {
                        id: data,
                        application: "item",
                        key: {
                            schemaId: "itemlookup",
                            mode: "output",
                            platform: platform(),
                        },
                    };
                    var urlToUse = url("/api/data/item?" + $.param(restParameters));

                    $http.get(urlToUse).success(function (data) {
                        if (data.resultObject['fields'] === undefined) {
                            return;
                        }
                        var fields = data.resultObject['fields'];
                        
                        var matusetransData = datamap['fields'];

                        var newRecord = {};
                        newRecord['itemnum'] = fields['itemnum'];
                        newRecord['quantity'] = 1;
                        newRecord['item_.description'] = fields['description'];
                        newRecord['issuetype'] = matusetransData['#issuetype'];
                        newRecord['matusetransid'] = null;
                        newRecord['assetnum'] = matusetransData['#assetnum'];
                        newRecord['issueto'] = matusetransData['#issueto'];
                        newRecord['location'] = matusetransData['#location'];
                        newRecord['refwo'] = matusetransData['#refwo'];
                        var user = contextService.getUserData();
                        newRecord['siteid'] = user.siteid;
                        newRecord['storeloc'] = matusetransData['#storeloc'];
                        parameters.clonedCompositionData.push(newRecord);
                        redirectService.redirectToTab('invissue_');
                        return;
                    });
            });

        },
        
    };

});


