var app = angular.module('sw_layout');

app.factory('scannerdetectionService', function ($http, $rootScope, restService, searchService, redirectService, contextService, alertService, associationService, modalService) {

    return {
        initInventoryGridListener: function (schema, datamap, parameters) {
            var searchData = parameters.searchData;

            $(document).scannerDetection(function (data) {
                var scanOrderString = contextService.invbalancesScanOrder();
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
                            $rootScope.$digest();
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
                        var itemdata = data.resultObject['fields'];
                        
                        var matusetransData = parameters.parentdata;

                        if (itemdata['itemtype'] == 'TOOL' && matusetransData['#issueto'] == null) {
                            //var compositionschema = parentschema.cachedCompositions['invissue_'].schemas['detail'];
                            //var itemDatamap = {};
                            //itemDatamap['itemnum'] = null;
                            //itemDatamap['inventory_'] = null;
                            //itemDatamap['#gldebitacct'] = null;
                            //itemDatamap['enterby'] = user.login.toUpperCase();
                            //itemDatamap['siteid'] = user.siteId;
                            //itemDatamap['matusetransid'] = null;
                            //itemDatamap['refwo'] = matusetransData['#refwo'];
                            //itemDatamap['assetnum'] = matusetransData['#assetnum'];
                            //itemDatamap['issueto'] = matusetransData['#issueto'];
                            //itemDatamap['location'] = matusetransData['#location'];
                            //itemDatamap['storeloc'] = matusetransData['#storeloc'];
                            //itemDatamap['gldebitacct'] = matusetransData['#gldebitacct'];

                            //modalService.show(compositionschema, itemDatamap, null, matusetransData, parameters.parentschema);
                            alertService.alert("Issued To is required when issuing a tool.");
                            return;
                        }

                        var newRecord = {};
                        newRecord['itemnum'] = itemdata['itemnum'];
                        newRecord['quantity'] = 1;
                        newRecord['item_.description'] = itemdata['description'];
                        newRecord['binnum'] = itemdata['inventory_.binnum'];
                        newRecord['issuetype'] = 'ISSUE';
                        newRecord['matusetransid'] = null;
                        newRecord['assetnum'] = matusetransData['#assetnum'];
                        newRecord['gldebitacct'] = matusetransData['#gldebitacct'];
                        newRecord['issueto'] = matusetransData['#issueto'];
                        newRecord['location'] = matusetransData['#location'];
                        newRecord['refwo'] = matusetransData['#refwo'];
                        var user = contextService.getUserData();
                        newRecord['siteid'] = user.siteid;
                        newRecord['storeloc'] = matusetransData['#storeloc'];

                        var searchData = {
                            itemnum: newRecord['itemnum'],
                            location: newRecord['storeloc'],
                            siteid: newRecord['siteid']
                        };
                        searchService.searchWithData("invcost", searchData).success(function (costdata) {
                            var resultObject = costdata.resultObject;
                            var resultFields = resultObject[0].fields;
                            var costtype = itemdata['inventory_.costtype'];
                            if (costtype === 'STANDARD') {
                                newRecord['unitcost'] = resultFields.stdcost;
                            } else if (costtype === 'AVERAGE') {
                                newRecord['unitcost'] = resultFields.avgcost;
                            }
                            parameters.clonedCompositionData.push(newRecord);
                            return;
                        });
                    });
            });

        },
        initInvuseTransferListener: function (schema, datamap) {
            $(document).scannerDetection(function (data) {
                datamap['invuseline_.itemnum'] = data;
                $rootScope.$digest();
            });
        },
        initInvIssueListener: function (schema, datamap) {
            $(document).scannerDetection(function (data) {
                var scanOrderString = contextService.scanOrder(schema.applicationName);
                var scanOrder = scanOrderString.split(",");
                var extraparameters = { keepfilterparameters: true };
                for (var attribute in scanOrder) {
                    var currentAttribute = scanOrder[attribute];
                    // If the property is not already in th escan data, add it and its value
                    if (!datamap.hasOwnProperty(currentAttribute)) {
                        var localSearchData = {};
                        localSearchData[currentAttribute] = data;
                        searchService.refreshGrid(localSearchData, extraparameters);
                        return;
                        // Else if the property exists but is blank, set it to its new value
                    } else if (datamap[currentAttribute] === '' || datamap[currentAttribute] === null) {
                        datamap[currentAttribute] = data;
                        $rootScope.$digest();

                        //associationService.updateUnderlyingAssociationObject(null, null, scope);
                        return;
                    }
                }
            });
        },
        
    };

});


