
(function (angular) {
    'use strict';

    angular.module('sw_layout').factory('invissueService', ["$rootScope", "$log", 'searchService', "inventoryServiceCommons", "redirectService", invissueService]);

    function invissueService($rootScope, $log, searchService, inventoryServiceCommons, redirectService) {

        var service = {
            afterChangeBin: afterChangeBin,
            afterchangeItem: afterchangeItem,
            afterChangeRotAsset: afterChangeRotAsset,
            afterChangeAsset: afterChangeAsset,
            afterChangeWorkorder: afterChangeWorkorder,
            afterChangeLocation: afterChangeLocation,
            afterChangeCurbal: afterChangeCurbal,
            afterChangeLotnum: afterChangeLotnum,
            afterChangeLaborCode: afterChangeLaborCode,
            invissuelistclick: invissuelistclick,
            validateInvIssue: validateInvIssue,
            afterChangeStoreloc: afterChangeStoreloc
        };

        return service;


        function invissuelistclick(datamap, schema) {
            var param = {};
            param.id = datamap['matusetransid'];
            var application = schema.applicationName;
            var schemaId;
            var mode = 'input';

            //Logic to determine whether the record is an ISSUE
            //and whether all of the issued items have been returned
            if (datamap['issuetype'] != 'ISSUE') {
                //if it´s not an issue redirecting to return screen
                redirectService.goToApplicationView(application, 'viewinvreturndetail', mode, null, param, null);
                return;
            }


            //Sets qtyreturned to 0 if null
            //Parses the qtyreturned if its in a strng format
            var qtyreturned = 0;
            if (typeof datamap['qtyreturned'] === "string") {
                qtyreturned = parseInt(datamap['qtyreturned']);
            } else if (datamap['qtyreturned'] != null) {
                qtyreturned = datamap['qtyreturned'];
            }

            //For an issue, the quantity will be a negative number, representing the # of items issued
            //The below if statement will add the positive quantityreturned to the negative quantity.
            //If the result is negative, then are still items to be returned
            if (qtyreturned + datamap['quantity'] >= 0) {
                //If all of the items have been returned, show the viewdetail page for 'ISSUE' records
                redirectService.goToApplicationView(application, 'viewinvissuedetail', mode, null, param, null);
                return;
            }

            if (qtyreturned + datamap['quantity'] != -1) {
                //There are still items to be returned
                schemaId = application == "invreturn" ? 'editinvreturndetail' : 'editinvissuedetail';
                redirectService.goToApplicationView(application, schemaId, mode, null, param, null);
                return;
            }


            //TODO: what´s the idea here? just one item?
            //TODO: ken: is it correct to search for things even before the confirmation occurs?
            var transformedData = angular.copy(datamap);
            transformedData['#quantityadj'] = 1;
            inventoryServiceCommons.returnTransformation(null, transformedData);
            // Get the cost type
            inventoryServiceCommons.updateInventoryCosttype({ fields: transformedData }, 'storeloc');
            var originalDatamap = {
                fields: datamap,
            };
            inventoryServiceCommons.returnConfirmation(null, transformedData, {
                continue: function () {
                    // TODO: update so that mock client validation is not necessary
                    sessionStorage.mockclientvalidation = true;
                    $rootScope.$broadcast('sw_submitdata', {
                        successCbk: function (data) {
                            sessionStorage.mockclientvalidation = false;
                            $rootScope.$broadcast('sw_refreshgrid');
                        },
                        failureCbk: function (data) {
                            sessionStorage.mockclientvalidation = false;
                        },
                        isComposition: false,
                        refresh: true,
                        selecteditem: transformedData,
                        originalDatamap: originalDatamap,
                    });
                },
            });


        };


        function validateInvIssue(schema, datamap) {
            var errors = [];
            var refwo = datamap['refwo'];
            var location = datamap['location'];
            var assetnum = datamap['assetnum'];
            var gldebitacct = datamap['gldebitacct'];
            var itemtype = datamap['inventory_.item_.itemtype'];
            if (itemtype == 'ITEM' &&
                nullOrEmpty(refwo) &&
                nullOrEmpty(location) &&
                nullOrEmpty(assetnum) &&
                nullOrEmpty(gldebitacct)) {
                errors.push("Either a Workorder, Location, Asset, or GL Debit Account is required.");
            }
            return errors;
        };

        function afterChangeLocation(parameters) {
            var fields = parameters.fields;
            if (nullOrEmpty(fields['location'])) {
                fields['refwo'] = null;
                fields['workorder'] = null;
                fields['assetnum'] = null;
            }
        };

        function afterChangeCurbal(parameters) {
            var fields = parameters['fields'];
            fields['#curbal'] = fields['binbalances_.curbal'];
            fields['lotnum'] = fields['binbalances_.lotnum'];
        };

        function afterChangeLotnum(parameters) {
            var fields = parameters['fields'];
            fields['#curbal'] = fields['binbalances_.curbal'];
            fields['lotnum'] = fields['binbalances_.lotnum'];
        }

        function afterChangeLaborCode(parameters) {
            var fields = parameters.fields;
            if (fields['labor_']) {
                fields['issueto'] = fields['labor_']['personid'];
            }
        };

        function afterChangeWorkorder(parameters) {
            var fields = parameters.fields;
            if (nullOrEmpty(fields['refwo'])) {
                fields['refwo'] = null;
                fields['workorder'] = null;
                fields['location'] = null;
                fields['assetnum'] = null;
                fields['gldebitacct'] = null;
                return;
            }
            // If the workorder's location is null, remove the current datamap's location
            if (fields['workorder_.location'] == null) {
                fields['location'] = null;
            } else {
                fields['location'] = fields['workorder_.location'];
            }
            // If the workorder's assetnum is null, remove the current datamap's assetnum
            if (fields['workorder_.assetnum'] == null) {
                fields['assetnum'] = null;
            } else {
                fields['assetnum'] = fields['workorder_.assetnum'];
            }
        };


        function afterChangeAsset(parameters) {
            var fields = parameters.fields;
            var refwo = fields['refwo'];
            var location = fields['location'];
            if (!nullOrEmpty(fields['assetnum'])) {
                if (!refwo || refwo.trim() == "") {
                    refwo = null;
                }
                if (!location || location.trim() == "") {
                    location = null;
                }
                if (refwo != "") {
                    return;
                }
                if (refwo != "" && location == "") {
                    fields['location'] = fields['asset_.location'];
                    return;
                }
                if (refwo == "") {
                    fields['location'] = fields['asset_.location'];
                }
            } else {
                // If the asset is cleared, clear the workorder and the location
                fields['refwo'] = null;
                fields['workorder'] = null;
                fields['location'] = null;
            }
        };


        function afterChangeRotAsset(parameters) {
            var fields = parameters.fields;
            if (fields['rotassetnum'].trim() != "") {
                fields['binbalances_.binnum'] = fields['rotatingasset_.binnum'];
                fields['binnum'] = fields['rotatingasset_.binnum'];
                fields['binbalances_.lotnum'] = "";
                fields['lotnum'] = "";
                fields['binbalances_.curbal'] = 1;
            }
        };


        function afterChangeBin(parameters) {
            var fields = parameters['fields'];

            if (fields['binnum'] == null) {
                fields['binbalances_.binnum'] = null;
                fields['binbalances_.lotnum'] = null;
                fields['binbalances_.curbal'] = null;
                return;
            }
            if (fields['binbalances_'] && fields['binnum'] != null) {
                //Check if null rather than nullOrEmpty since the binnum for an association option can be an empty string
                fields['lotnum'] = fields['binbalances_.lotnum'];
                fields['#curbal'] = fields['binbalances_.curbal'];
                fields['curbal'] = fields['binbalances_.curbal'];
                fields['quantity'] = 1;
                //to enable composition details on batch mode
                if (fields['#datamaptype'] == "compositionitem") {
                    fields['matusetransid'] = -1 * (fields["#datamapidx"] - 1);
                }


                return;
            };
            // If the binbalances_ record is not filled but the binnum is
            // (binnum filled after itemnum change) then use the available 
            // fields to find an applicable lotnum and curbal. If the binnum
            // has been cleared, clear the lot and curbal
            if (fields['binnum'] != null && fields['binnum'] != " ") {
                var searchData = {
                    orgid: fields['orgid'],
                    siteid: fields['siteid'],
                    itemnum: fields['itemnum'],
                    location: fields['storeloc'],
                    binnum: fields['binnum']
                };
                searchService.searchWithData("invbalances", searchData, "invbalancesList").success(function (data) {
                    var resultObject = data.resultObject;
                    var resultFields = resultObject[0].fields;
                    var lotnum = resultFields['lotnum'];
                    var curbal = resultFields['curbal'];
                    fields['lotnum'] = lotnum;
                    fields['#curbal'] = curbal == null ? 0 : curbal;
                    fields['curbal'] = curbal == null ? 0 : curbal;
                    fields['binbalances_.lotnum'] = lotnum;
                    fields['binbalances_.curbal'] = curbal == null ? 0 : curbal;
                    fields['quantity'] = 1;
                    //to enable composition details on batch mode
                    if (fields['#datamaptype'] == "compositionitem") {
                        //let´s put a negative id so that it gets ignored on maximo side
                        fields['matusetransid'] = -1 * (fields["#datamapidx"] - 1);
                    }
                });
            }
            else {
                fields['lotnum'] = null;
                fields['#curbal'] = null;
            }
        };


        function afterchangeItem(parameters) {
            var fields = parameters['fields'];

            fields['lotnum'] = null;
            fields['#curbal'] = null;

            var itemnum = fields['itemnum'];
            if (nullOrEmpty(itemnum)) {
                fields['binnum'] = null;
                fields['unitcost'] = null;
                fields['inventory_.issueunit'] = null;
                fields['inventory_.itemtype'] = null;

                return;
            }

            if (fields['inventory_.binnum'] == null) {
                fields['binnum'] = "";
            } else {
                fields['binnum'] = fields['inventory_.binnum'];
            }
        };

        function afterChangeStoreloc(parameters) {
            var fields = parameters['fields'];
            var parentdata = parameters['parentdata'];

            if (fields['storeloc'] == null) {
                nullifyProperties(fields, ['unitcost', 'binnum', 'inventory_.issueunit', '#curbal', 'matusetransid']);
                return;
            }

            fields['lotnum'] = null;
            fields['#curbal'] = null;

            var invParams = {
                itemnum: parentdata['itemnum'],
                storeloc: fields['storeloc'],
                siteid: parentdata['siteid'],
                orgid: parentdata['orgid'],
                itemsetid: fields['itemsetid']
            };
            inventoryServiceCommons.updateInventoryCosttype({ fields: invParams }, 'storeloc');
        }
    }

})(angular);