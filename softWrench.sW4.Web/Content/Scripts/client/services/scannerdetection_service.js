(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('scannerdetectionService', [
    "$log", "$http", "$rootScope", "$timeout", "restService", "searchService", "redirectService",
    "contextService", "alertService", "associationService", "modalService",
    "fieldService", "submitService", "validationService", "commandService", "scanningCommonsService", "applicationService",
    function ($log, $http, $rootScope, $timeout, restService, searchService, redirectService,
            contextService, alertService, associationService, modalService,
            fieldService, submitService, validationService, commandService, scanningCommonsService, applicationService) {

    var validateAssocationLookupFn = function (result, searchObj) {
        if (Object.keys(result).length != 1 ||
            !result[searchObj.fieldMetadata.associationKey] ||
            result[searchObj.fieldMetadata.associationKey].associationData.length != 1) {
            // Exit if more than one record is returned
            alertService.alert("{0} is not a valid option for the {1} field".format(searchObj.code, searchObj.fieldMetadata.label));
            return false;
        }
        return true;
    };

    return {

        initInventoryGridListener: function (scope, schema, datamap, parameters) {
            var searchData = parameters.searchData;

            // Set the avgTimeByChar to the correct value depending on if using mobile or desktop
            $(document).scannerDetection({
                avgTimeByChar: scanningCommonsService.getTimeBetweenChars(),
                onComplete: function (data) {
                    // Retrieve the scan order string from the context that relates to the current schema
                    const scanOrderString = contextService.retrieveFromContext(schema.schemaId + "ScanOrder");
                    const scanOrder = scanOrderString.split(",");
                    const extraparameters = { keepfilterparameters: true };
                    // When the user scans a value, loop through the properties in the scan order
                    for (let attribute in scanOrder) {
                        const currentAttribute = scanOrder[attribute];
                        // If the property is not already in the scan data, add it and its value
                        if (!searchData.hasOwnProperty(currentAttribute)) {
                            const localSearchData = {};
                            localSearchData[currentAttribute] = data;
                            searchService.refreshGrid(localSearchData, null, extraparameters);
                            return;
                            // Else if the property exists but is blank, set it to its new value
                        } else if (searchData[currentAttribute] === '') {
                            searchData[currentAttribute] = data;
                            searchService.refreshGrid(searchData, null, extraparameters);
                            return;
                        }
                    }
                },
            });
        },
        initIssueItemListener: function (scope, schema, datamap, parameters) {
            // Set the avgTimeByChar to the correct value depending on if using mobile or desktop
            $(document).scannerDetection({
                avgTimeByChar: scanningCommonsService.getTimeBetweenChars(),
                onComplete: function (data) {
                    //This function will look for an item being scanned for the Issue Inventory application. 
                    //Scanned items are added to the invissue composition.  

                    //Checks to see if the item has already been added to the composition list.
                    //If the item is found, this will increment the quantity by 1.
                    for (let key in parameters.clonedCompositionData) {
                        const item = parameters.clonedCompositionData[key];
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
                    const restParameters = {
                        id: data,
                        application: "item",
                        key: {
                            schemaId: "itemlookup",
                            mode: "output",
                            platform: platform(),
                        },
                    };
                    const urlToUse = url("/api/data/item?" + $.param(restParameters));
                    return $http.get(urlToUse).then(function (response) {
                        const data = response.data;
                        if (data.resultObject === undefined) {
                            return;
                        }
                        var itemdata = data.resultObject;
                        const matusetransData = parameters.parentdata;
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
                        const user = contextService.getUserData();
                        newRecord['siteid'] = user.siteid;
                        newRecord['storeloc'] = matusetransData['#storeloc'];
                        const searchData = {
                            itemnum: newRecord['itemnum'],
                            location: newRecord['storeloc'],
                            siteid: newRecord['siteid']
                        };
                        return searchService.searchWithData("invcost", searchData).then(function (costdataResponse) {
                            const costdata = costdataResponse.data;
                            const resultObject = costdata.resultObject;
                            const resultFields = resultObject[0];
                            const costtype = itemdata['inventory_.costtype'];
                            if (costtype === 'STANDARD') {
                                newRecord['unitcost'] = resultFields.stdcost;
                            } else if (costtype === 'AVERAGE') {
                                newRecord['unitcost'] = resultFields.avgcost;
                            }
                            parameters.clonedCompositionData.push(newRecord);
                            return;
                        });
                    });
                },
            });
        },

        initInvuseTransferDetailListener: function (schema, datamap) {
            // Set the avgTimeByChar to the correct value depending on if using mobile or desktop
            $(document).scannerDetection({
                avgTimeByChar: scanningCommonsService.getTimeBetweenChars(),
                onComplete: function (data) {
                    datamap['invuseline_.itemnum'] = data;
                    $rootScope.$digest();
                },
            });
        },

        initMaterialScanningListener: function (scope, schema, datamap, parameters) {

            scanningCommonsService.registerScanCallBackOnSchema(parameters, function(data) {
                datamap['itemnum'] = data;
                $rootScope.$digest();
            });
        },

        initInvIssueDetailListener: function (scope, schema, datamap) {
            // Set the avgTimeByChar to the correct value depending on if using mobile or desktop
            $(document).scannerDetection({
                avgTimeByChar: scanningCommonsService.getTimeBetweenChars(),
                onComplete: function (data) {
                    //If the user scan's the string '%SUBMIT%', then the sw_submitdata event
                    //will be called using the default submit functions/process
                    if (data == '%SUBMIT%') {
                        return applicationService.save({ isComposition: false, selecteditem: scope.datamap });
                    }
                    const scanOrderString = contextService.retrieveFromContext(schema.schemaId + "ScanOrder");
                    const scanOrder = scanOrderString.split(",");
                    for (let attribute in scanOrder) {
                        // Loop through the scan order, checking the corresponding fields in the datamap
                        // to see if they have a value
                        const currentAttribute = scanOrder[attribute];
                        if (datamap[currentAttribute] === '' || datamap[currentAttribute] === null) {
                            // Update the associated values
                            const fieldMetadata = fieldService.getDisplayableByKey(schema, currentAttribute);
                            // Update the associated values using the new scanned data
                            const lookupObj = {};
                            lookupObj.code = data;
                            lookupObj.fieldMetadata = fieldMetadata;
                            lookupObj.application = null;
                            lookupObj.schemaId = null;
                            if (fieldMetadata.rendererParameters != undefined) {
                                lookupObj.application = fieldMetadata.rendererParameters.application;
                                lookupObj.schemaId = fieldMetadata.rendererParameters.schemaId;
                            }
                            const searchObj = {};
                            searchObj[currentAttribute] = data;
                            associationService.updateDependentAssociationValues(scope, datamap, lookupObj, validateAssocationLookupFn, searchObj);
                            // Exit the loop once we have set a value from the scan
                            break;
                        }
                    };
                }
            });
        },

        initSouthernPhysicalCountGridListener: function (scope, schema, datamap, parameters) {
            var searchData = parameters.searchData;

            // Set the avgTimeByChar to the correct value depending on if using mobile or desktop
            $(document).scannerDetection({
                avgTimeByChar: scanningCommonsService.getTimeBetweenChars(),
                onComplete: function (data) {
                    // Retrieve the scan order string from the context that relates to the current schema
                    const scanOrderString = contextService.retrieveFromContext(schema.schemaId + "ScanOrder");
                    const scanOrder = scanOrderString.split(",");
                    const extraparameters = { keepfilterparameters: true };
                    // When the user scans a value, loop through the properties in the scan order
                    for (let attribute in scanOrder) {
                        const currentAttribute = scanOrder[attribute];
                        // If the property is not already in the scan data, add it and its value
                        if (!searchData.hasOwnProperty(currentAttribute)) {
                            const localSearchData = {};
                            localSearchData[currentAttribute] = data;
                            searchService.refreshGrid(localSearchData, null, extraparameters);
                            return;
                            // Else if the property exists but is blank, set it to its new value
                        } else if (searchData[currentAttribute] === '') {
                            searchData[currentAttribute] = data;
                            searchService.refreshGrid(searchData, null, extraparameters);
                            return;
                        }
                    }
                },
            });
            // Add auto redirect
            // After griddata has changed
            scope.$on(JavascriptEventConstants.GridDataChanged, function (event, data, panelId) {
                // If only one record is found
                if (data.length === 1) {
                    const invbalanceid = data[0]['invbalancesid'];
                    const param = {};
                    param.id = invbalanceid;
                    const application = 'physicalcount';
                    const detail = 'detail';
                    const mode = 'input';
                    param.scanmode = true;
                    // Redirect to the detail page for that record
                    redirectService.goToApplicationView(application, detail, mode, null, param, null);
                }
            });
        },

        initSouthernOperatorRounds: function (scope, schema, datamap, parameters) {
            scanningCommonsService.registerScanCallBackOnSchema(parameters, function (data) {
                $log.get("scan#initSouthernOperatorRounds").info('receiving operator round notifications');
                const assets = datamap.multiassetlocci_;
                for (let asset in assets) {
                    if (!assets.hasOwnProperty(asset)) {
                        continue;
                    }

                    if (!assets[asset].assetnum || !assets[asset].assetnum.equalIc(data)) {
                        alertService.alert("Asset record {0} not found".format(data));
                        return;
                    }

                    assets[asset]['#selected'] = "true";
                    const datamapToSend = {};
                    angular.copy(datamap, datamapToSend);
                    //                            datamapToSend['multiassetlocci_'] = [datamap];
                    const command = {
                        service: "meterReadingService",
                        method: "read",
                        nextSchemaId: "readings",
                        stereotype: "modal",
                        properties: {
                            modalclass: "readingsmodal"
                        },
                        scopeParameters: ['schema', 'datamap']
                    };
                    const clonedSchema = {};
                    angular.copy(parameters.parentschema, clonedSchema);
                    const scope = {
                        datamap: datamapToSend,
                        schema: clonedSchema
                    };
                    scope.schema.applicationName = "workorder";

                    commandService.doCommand(scope, command);
                }
            });

        }
    };
}]);

})(angular);