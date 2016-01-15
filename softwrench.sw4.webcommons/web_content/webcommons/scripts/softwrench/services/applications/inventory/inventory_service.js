(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('inventoryService', [
        "$http", "$timeout", "contextService", "redirectService", "modalService", "searchService", "restService", "alertService", "inventoryServiceCommons", "$rootScope", "applicationService", 
        function ($http, $timeout, contextService, redirectService, modalService, searchService, restService, alertService, inventoryServiceCommons, $rootScope, applicationService) {

    var formatQty = function (datamap, value, column) {
        if (datamap['issuetype'] == 'ISSUE') {
            if (datamap[column.attribute] != null) {
                return Math.abs(datamap[column.attribute]);
            }
            else {
                return "0";
            }
        }
        return datamap[column.attribute];
    };

    var formatQtyReturned = function (datamap, value, column) {
        if (datamap['issuetype'] == 'ISSUE') {
            if (datamap[column.attribute] == null) {
                return "0";
            }
            return datamap[column.attribute];
        }
        return "";
    };

    var createInvUse = function (schema, useType) {
        var invuse = {};
        invuse.usetype = useType;
        contextService.insertIntoContext("invuse", invuse, false);
        redirectService.goToApplicationView("invuse", "newdetail", "Input", null, null, null);
    };

    var submitInvIssueRec = function (datamap, clonedCompositionData, index) {
        if (index == clonedCompositionData.length) {
            return;
        }

        var fields = clonedCompositionData[index];
        if (fields['matusetransid'] != null) {
            submitInvIssueRec(datamap, clonedCompositionData, ++index);
            return;
        }

        if (fields['binnum'] == null) {
            fields['binnum'] = "";
        }

        var newIssueItem = {};
        newIssueItem['matusetransid'] = null;
        newIssueItem['rowstamp'] = null;
        newIssueItem['quantity'] = fields['quantity'];
        newIssueItem['issuetype'] = 'ISSUE';
        newIssueItem['itemnum'] = fields['itemnum'];
        newIssueItem['location'] = fields['location'];
        newIssueItem['unitcost'] = fields['unitcost'];
        newIssueItem['binnum'] = fields['binnum'];
        newIssueItem['siteid'] = fields['siteid'];
        newIssueItem['storeloc'] = datamap['#storeloc'];
        newIssueItem['refwo'] = datamap['#refwo'];
        newIssueItem['issueto'] = datamap['#issueto'];
        newIssueItem['assetnum'] = datamap['#assetnum'];
        newIssueItem['gldebitacct'] = (fields['gldebitacct'] == null) ?
        datamap['#gldebitacct'] : fields['gldebitacct'];

        var jsonString = angular.toJson(newIssueItem);
        var httpParameters = {
            application: "invissue",
            platform: "web",
            currentSchemaKey: "editinvissuedetail.input.web"
        };
        restService.invokePost("data", "post", httpParameters, jsonString, function () {
            fields['matusetransid'] = -1;
            submitInvIssueRec(datamap, clonedCompositionData, ++index);
            return;
        }, function () {
            submitInvIssueRec(datamap, clonedCompositionData, ++index);
            return;
        });
    };

    var setBatchIssueBin = function (parameters) {
        var itemnum = parameters['fields']['itemnum'];
        var siteid = parameters['fields']['siteid'];
        var storeloc = parameters['fields']['storeloc'];
        if (itemnum != null && itemnum.trim() != "" &&
            siteid != null && siteid.trim() != "" &&
            storeloc != null && storeloc.trim() != "") {
            var searchData = {
                itemnum: itemnum,
                siteid: siteid,
                location: parameters['fields']['storeloc']
            };
            getBinQuantity(searchData, parameters, '#curbal', null);
        } else {
            parameters['fields']['#curbal'] = null;
            parameters['fields']['invbalances_.binnum'] = null;
        }
    };

    var getBinQuantity = function (searchData, parameters, balanceField, binnum, lotnum) {
        searchService.searchWithData("invbalances", searchData, "invbalancesList").success(function (data) {
            var resultObject = data.resultObject;

            for (var i = 0; i < resultObject.length; i++) {
                var fields = resultObject[i]['fields'];
                if (fields['binnum'] == binnum && fields['lotnum'] == lotnum) {
                    parameters.fields[balanceField] = fields.curbal;
                    // Exit the loop
                    break;
                }
                parameters.fields[balanceField] = null;
            };
        });
    };





    var invIssue_maximo71_afterChangeItem = function (parameters) {
        if (nullOrEmpty(parameters['fields']['itemnum']) || nullOrEmpty(parameters['fields']['storeloc'])) {
            return;
        }
        var maxvarsSearchData = {
            varname: 'DEFISSUECOST',
            siteid: parameters['fields']['siteid']
        };
        searchService.searchWithData("maxvars", maxvarsSearchData).success(function (maxvarsData) {
            var resultObject = maxvarsData.resultObject;
            var fields = resultObject[0].fields;
            var costtype = fields['varvalue'];
            parameters['fields']['inventory_.costtype'] = costtype;

            var itemnum = parameters['fields']['itemnum'];
            if (nullOrEmpty(itemnum)) {
                parameters['fields']['itemnum'] = null;
                parameters['fields']['unitcost'] = null;
                parameters['fields']['inventory_.issueunit'] = null;
                parameters['fields']['inventory_.itemtype'] = null;
                parameters['fields']['#curbal'] = null;
                return;
            }

            var searchData = {
                itemnum: parameters['fields']['itemnum'],
                location: parameters['fields']['storeloc'],
                siteid: parameters['fields']['siteid']
            };
            searchService.searchWithData("invcost", searchData).success(function (data) {
                var invcostRo = data.resultObject;
                var invcostFields = invcostRo[0].fields;
                if (costtype === 'STDCOST') {
                    parameters.fields['unitcost'] = invcostFields.stdcost;
                } else if (costtype === 'AVGCOST') {
                    parameters.fields['unitcost'] = invcostFields.avgcost;
                }
                parameters['fields']['binnum'] = parameters['fields']['inventory_.binnum'];
                parameters['fields']['lotnum'] = null;
            });
        });
    };

    return {
        createIssue: function (schema, datamap) {
            redirectService.goToApplicationView(schema.applicationName, "newInvIssueDetail", "input", null, null, null);
        },

        navToBatchFilter: function (schema, datamap) {
            redirectService.goToApplicationView(schema.applicationName, "batchInvIssueFilter", "input", null, null, null);
        },

        formatQtyReturnedList: function (parameters) {
            var value = parameters.value;
            var column = parameters.column;
            var dm = parameters.datamap;
            if (dm != undefined) {
                if (dm.fields != undefined) {
                    dm = parameters.datamap.fields;
                }
            }
            return formatQtyReturned(dm, value, column);
        },

        formatQtyList: function (parameters) {
            var value = parameters.value;
            var column = parameters.column;
            var dm = parameters.datamap;
            if (dm != undefined) {
                if (dm.fields != undefined) {
                    dm = parameters.datamap.fields;
                }
            }
            return formatQty(dm, value, column);
        },

        formatQtyReturnedDetail: function (parameters) {
            var value = parameters.value;
            var column = parameters.column;
            var dm = parameters.datamap;
            if (dm != undefined) {
                if (dm.fields != undefined) {
                    dm = parameters.datamap.fields;
                }
                var formattedValue = formatQtyReturned(dm, value, column);
                dm[column.attribute] = formattedValue;
                return formattedValue;
            }
            return;
        },

        formatQtyDetail: function (parameters) {
            var value = parameters.value;
            var column = parameters.column;
            var dm = parameters.datamap;
            if (dm != undefined) {
                if (dm.fields != undefined) {
                    dm = parameters.datamap.fields;
                }
                var formattedValue = formatQty(dm, value, column);
                dm[column.attribute] = formattedValue;
                return formattedValue;
            }
            return;
        },

        submitReturnConfirmation: function (event, datamap, parameters) {
            inventoryServiceCommons.returnTransformation(event, datamap);
            return inventoryServiceCommons.returnConfirmation(event, datamap, parameters);
        },

        invissuelistclick_maximo71: function (datamap, schema) {
            var param = {};
            param.id = datamap['matusetransid'];
            var application = schema.applicationName;
            var detail = 'viewinvreturndetail';
            var mode = 'input';
            //Logic to determine whether the record is an ISSUE
            //and whether all of the issued items have been returned
            if (datamap['issuetype'] == 'ISSUE') {

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
                if (qtyreturned + datamap['quantity'] < 0) {
                    if (qtyreturned + datamap['quantity'] == -1) {
                        var transformedData = angular.copy(datamap);
                        transformedData['#quantityadj'] = 1;
                        inventoryServiceCommons.returnTransformation(null, transformedData);
                        // Maximo 7.1 store the inventory cost type in a different table than maximo 7.5
                        // Using the afterchange item for maximo 7.1 to get the cost type and unit cost
                        invIssue_maximo71_afterChangeItem({ fields: transformedData });
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
                                    selecteditem: transformedData,
                                    originalDatamap: originalDatamap,
                                });
                            },
                        });

                        return;
                    } else {
                        detail = 'editinvissuedetail';
                    }
                } else {
                    //If all of the items have been returned, show the viewdetail page for 'ISSUE' records
                    detail = 'viewinvissuedetail';
                }
            }

            redirectService.goToApplicationView(application, detail, mode, null, param, null);
        },

        navToBatchIssueDetail: function (schema, datamap) {
            var siteid = datamap['siteid'];

            if (nullOrEmpty(siteid)) {
                alertService.alert("A Site Id is required.");
                return;
            }

            var storeloc = datamap['storeloc'];

            if (nullOrEmpty(storeloc)) {
                alertService.alert("A Storeroom is required.");
                return;
            }

            var refwo = datamap['refwo'];
            var location = datamap['location'];
            var assetnum = datamap['assetnum'];
            var gldebitacct = datamap['gldebitacct'];

            if (nullOrEmpty(refwo) &&
                nullOrEmpty(location) &&
                nullOrEmpty(assetnum) &&
                nullOrEmpty(gldebitacct)) {
                alertService.alert("Either a Workorder, Location, Asset, or GL Debit Account is required.");
                return;
            }

            var newDatamap = {};
            newDatamap['assetnum'] = datamap['assetnum'];
            newDatamap['#refwo'] = datamap['refwo'];
            newDatamap['#storeloc'] = datamap['storeloc'];
            newDatamap['#siteid'] = datamap['siteid'];
            newDatamap['#gldebitacct'] = datamap['gldebitacct'];
            newDatamap['#issuetype'] = datamap['issuetype'];
            newDatamap['#location'] = datamap['location'];
            newDatamap['#assetnum'] = datamap['assetnum'];
            newDatamap['#issueto'] = datamap['issueto'];
            newDatamap['invissue_'] = [];

            var param = {};
            param.id = datamap['refwo'];
            redirectService.goToApplicationView('invissuewo', 'newdetail', null, null, param, newDatamap);
        },

        submitNewBatchIssue: function (schema, datamap) {
            var clonedCompositionData = contextService.fetchFromContext('clonedCompositionData', true, true);
            submitInvIssueRec(datamap, clonedCompositionData, 0);
        },

        navToIssueReturnList: function (schema, datamap) {
            redirectService.goToApplicationView(schema.applicationName, "invIssueList", null, null, null, null);
        },

        displayNewIssueModal: function (parentschema, parentdatamap) {
            var compositionschema = parentschema.cachedCompositions['invissue_'].schemas['detail'];
            var user = contextService.getUserData();
            var itemDatamap = {};
            itemDatamap['itemnum'] = null;
            itemDatamap['inventory_'] = null;
            itemDatamap['#gldebitacct'] = null;
            itemDatamap['enterby'] = user.login.toUpperCase();
            itemDatamap['siteid'] = user.siteId;
            itemDatamap['matusetransid'] = null;
            itemDatamap['refwo'] = parentdatamap['#refwo'];
            itemDatamap['assetnum'] = parentdatamap['#assetnum'];
            itemDatamap['issueto'] = parentdatamap['#issueto'];
            itemDatamap['location'] = parentdatamap['#location'];
            itemDatamap['storeloc'] = parentdatamap['#storeloc'];
            itemDatamap['gldebitacct'] = parentdatamap['#gldebitacct'];

            modalService.show(compositionschema, itemDatamap, {}, null, null, parentdatamap, parentschema);
        },

        hideNewIssueModal: function (parameters) {
            modalService.hide();
            parameters['datamap'] = null;
        },

        batchissuelistclick: function (datamap, column, schema) {
            var newDatamap = {};
            angular.copy(datamap, newDatamap);

            modalService.show(schema, newDatamap);

        },

        cancelNewInvIssueItem: function () {
            modalService.hide();
        },

        addItemToBatch: function (datamap) {
            var itemtype = datamap['inventory_.item_.itemtype'];
            var issueto = datamap['issueto'];
            if (itemtype == 'TOOL' && nullOrEmpty(issueto)) {
                alertService.alert("Issued To is required when issuing a tool.");
                return;
            }

            var itemnum = datamap['itemnum'];

            if (nullOrEmpty(itemnum)) {
                alertService.alert("An item is required.");
                return;
            }

            var clonedCompositionData = contextService.fetchFromContext('clonedCompositionData', true, true);

            datamap['issuetype'] = "ISSUE";
            var newissue = angular.copy(datamap);
            newissue['item_.description'] = newissue['inventory_.item_.description'];
            newissue.matusetransid = null;

            datamap['issuetype'] = null;
            datamap['assetnum'] = null;
            datamap['itemnum'] = null;
            datamap['inventory_.item_.description'] = null;
            datamap['enterby'] = null;
            datamap['#gldebitacct'] = null;
            datamap['chartofaccounts_'] = null;
            datamap['gldebitacct'] = null;
            datamap['binnum'] = null;
            datamap['unitcost'] = null;
            datamap['quantity'] = null;
            datamap['gldebitacct'] = null;

            clonedCompositionData.push(newissue);
            modalService.hide();
        },

        updateItemInBatch: function (datamap) {
            var clonedCompositionData = contextService.fetchFromContext('clonedCompositionData', true, true);

            var compositionItem = null;
            for (var i = 0; i < clonedCompositionData.length; i++) {
                compositionItem = clonedCompositionData[i];
                if (compositionItem['matusetransid'] != null) {
                    continue;
                }

                if (compositionItem['itemnum'] == datamap['itemnum']) {
                    break;
                }
            }

            if (compositionItem != null) {
                var newissue = angular.copy(datamap);
                compositionItem['item_.description'] = newissue['inventory_.item_.description'];
                compositionItem['itemnum'] = newissue['itemnum'];
                compositionItem['#gldebitacct'] = newissue['#gldebitacct'];
                compositionItem['gldebitacct'] = newissue['gldebitacct'];
                compositionItem['binnum'] = newissue['binnum'];
                compositionItem['unitcost'] = newissue['unitcost'];
                compositionItem['quantity'] = newissue['quantity'];
                compositionItem['itemtype'] = newissue['itemtype'];
                compositionItem['costtype'] = newissue['costtype'];

                datamap['issuetype'] = null;
                datamap['assetnum'] = null;
                datamap['itemnum'] = null;
                datamap['inventory_.item_.description'] = null;
                datamap['enterby'] = null;
                datamap['#gldebitacct'] = null;
                datamap['chartofaccounts_'] = null;
                datamap['gldebitacct'] = null;
                datamap['binnum'] = null;
                datamap['unitcost'] = null;
                datamap['quantity'] = null;
                datamap['gldebitacct'] = null;
            }
            modalService.hide();
        },

        invIssueBatch_afterChangeItem: function (parameters) {
            var itemnum = parameters['fields']['itemnum'];
            parameters['fields']['binnum'] = null;
            parameters['fields']['lotnum'] = null;
            parameters['fields']['binbalances_.curbal'] = null;
            if (nullOrEmpty(itemnum)) {
                parameters['fields']['itemnum'] = null;
                parameters['fields']['unitcost'] = null;
                parameters['fields']['inventory_.issueunit'] = null;
                parameters['fields']['inventory_.itemtype'] = null;
                return;
            }

            setBatchIssueBin(parameters);

            var searchData = {
                itemnum: parameters['fields']['itemnum'],
                location: parameters['fields']['storeloc'],
                siteid: parameters['fields']['siteid'],
                orgid: parameters['fields']['orgid'],
                itemsetid: parameters['fields']['itemsetid']
            };
            searchService.searchWithData("inventory", searchData).success(function (data) {
                var resultObject = data.resultObject;
                var fields = resultObject[0].fields;
                var costtype = fields['costtype'];
                parameters['fields']['inventory_.costtype'] = costtype;
                var locationFieldName = "";
                if (parameters['fields'].storeloc != undefined) {
                    locationFieldName = "storeloc";
                }
                //parameters['fields']['inventory_.issueunit'] = fields['issueunit'];
                inventoryServiceCommons.doUpdateUnitCostFromInventoryCost(parameters, "unitcost", locationFieldName);
            });
        },

        invIssue_maximo71_afterChangeItem: function (parameters) {
            invIssue_maximo71_afterChangeItem(parameters)
        },

        createTransfer: function (schema) {
            if (schema === undefined) {
                return;
            }
            createInvUse(schema, "TRANSFER");
        },

        getReserveBinQuantity: function (parameters) {
            var binnum = parameters['fields']['#frombin'];
            var lotnum = parameters['fields']['#fromlot'];
            var searchData = {
                itemnum: parameters['fields']['itemnum'],
                siteid: parameters['fields']['siteid'],
                itemsetid: parameters['fields']['itemsetid'],
                location: parameters['fields']['location'],
                lotnum: parameters['fields']['#fromlot']
            };
            getBinQuantity(searchData, parameters, '#curbal', binnum, lotnum);
        },

        getTransferBinQuantity: function (parameters) {
            var binnum = parameters['fields']['invuseline_.frombin'];
            var lotnum = parameters['fields']['invuseline_.fromlot'];
            var searchData = {
                itemnum: parameters['fields']['invuseline_.itemnum'],
                siteid: parameters['fields']['inventory_.siteid'],
                itemsetid: parameters['fields']['inventory_.itemsetid'],
                location: parameters['fields']['fromstoreloc']
            };
            getBinQuantity(searchData, parameters, '#curbal', binnum, lotnum);
        },


        validateReturn: function (schema, datamap) {
            var errors = [];
            var quantityAdj = parseInt(datamap['#quantityadj']);
            var quantity = datamap['quantity'];
            var qtyReturned = parseInt(datamap['qtyreturned']);
            if (quantity - (quantityAdj + qtyReturned) < 0) {
                errors.push("The quantity being returned cannot be greater than the quantity that was issued.");
            }
            return errors;
        },


        afterChangeTransferFromLocation: function (event) {
            event.fields['itemnum'] = "";
            event.fields['inventory_.item_.itemtype'] = "";
            event.fields['invuseline_.frombin'] = "";
            event.fields['invuseline_.fromblot'] = "";
            event.fields['lotnum'] = "";
            event.fields['inventory_.issueunit'] = "";
            event.fields['#curbal'] = "";
            event.fields['invuseline_.unitcost'] = "";
            event.fields['invuseline_.costtype'] = "";
        },

        overrideGlAccount: function (event) {
            event.fields['gldebitacct'] = event.fields['#gldebitacct'];
        },

        afterChangeIssueQuantity: function (event) {
            if (event.fields['#issueqty'] > event.fields['reservedqty']) {
                alertService.alert("The quantity being transferred cannot be greater than the current balance of the From Bin.");
                event.scope.datamap['#issueqty'] = event.fields['reservedqty'];
            }
        },

        submitReservedInventoryIssue: function (schema, datamap) {
            if (datamap['#issueqty'] > datamap['invbalances_.curbal']) {
                alertService.alert("The quantity being issued cannot be greater than the current balance of the From Bin.");
                return;
            }
            // If the bin is null, set to a blank space so the MIF interprets the blank bin value correctly.
            if (datamap['invbalances_.binnum'] == null) {
                datamap['invbalances_.binnum'] = "";
            }
            // Create new matusetrans record
            var matusetransDatamap = {
                matusetransid: null,
                rowstamp: null,
                refwo: datamap['wonum'],
                assetnum: datamap['assetnum'],
                issueto: datamap['issueto'],
                location: datamap['oplocation'],
                glaccount: datamap['glaccount'],
                issuetype: 'ISSUE',
                itemnum: datamap['itemnum'],
                storeloc: datamap['location'],
                binnum: datamap['invbalances_.binnum'],
                lotnum: datamap['invbalances_.lotnum'],
                quantity: datamap['#issueqty'],
                unitcost: datamap['unitcost'],
                issueunit: datamap['issueunit'],
                enterby: datamap['enterby'],
                itemtype: datamap['item_.itemtype'],
                siteid: datamap['siteid'],
                costtype: datamap['inventory_.costtype']
            };
            // Post the new matusetrans record
            var httpParameters = {
                applicationName: "invissue",
                currentSchemaKey: "newInvIssueDetail.input.web",
                platform: "web"
            };

            applicationService.getPostPromise("invissue", "newInvIssueDetail", httpParameters, matusetransDatamap)
                .then(function (data) {

                // Get the reserved, actual, and issue quantities
                var reservedQty = Number(datamap["reservedqty"]);
                var actualQty = Number(datamap["actualqty"]);
                var issueQty = Number(datamap["#issueqty"]);
                // Calculate the new reserved and actual values based on the quantity being issued
                var newReservedQty = reservedQty - issueQty;
                var newActualQty = actualQty + issueQty;
                httpParameters = {
                    currentSchemaKey: "detail.input.web",
                    platform: "web",
                    applicationName: 'reservedMaterials'
                };

                // If the reserved quantity reaches 0 then the invreserve record should be deleted but
                // the delete functionality is not currently working so just update the invreserve record for now

                if (newReservedQty > 0) {
                    // If there is still a reserved quantity, update the record
                    // Update the datamap with the new values
                    datamap["reservedqty"] = newReservedQty;
                    datamap["actualqty"] = newActualQty;
                    // Put the updated invreserve record
                    var jsonWrapper = {
                        json: datamap,
                        requestData: httpParameters
                    }

                    jsonString = angular.toJson(jsonWrapper);
                    var urlToUse = url("/api/data/reservedMaterials/" + datamap["requestnum"]);


                    $http.put(urlToUse, jsonString).success(function () {
                        // Return to the list of reserved materials
                        redirectService.goToApplication("reservedMaterials", "reservedMaterialsList", null, null);
                    }).error(function () {
                        // Failed to update the material reservation
                    });
                } else {
                    // If the reserved quantity has reached 0, delete the record
                    var deleteUrl = url("/api/data/reservedMaterials/" + datamap["requestnum"] + "?" + $.param(httpParameters));
                    $http.delete(deleteUrl).success(function () {
                        // Return to the list of reserved materials
                        redirectService.goToApplication("reservedMaterials", "reservedMaterialsList", null, null);
                    });
                }
            });
        },

        onloadReservation: function (scope, schema, datamap) {
            if (datamap.fields) {
                datamap = datamap.fields;
            }
            var parameters = {
                fields: datamap
            };
            inventoryServiceCommons.updateInventoryCosttype(parameters, location);
            datamap['#issueqty'] = datamap['reservedqty'];
            datamap['#issuetype'] = "ISSUE";
            var searchData = {
                itemnum: parameters['fields']['itemnum'],
                siteid: parameters['fields']['siteid'],
                itemsetid: parameters['fields']['itemsetid'],
                location: parameters['fields']['location'],
                binnum: parameters['fields']['#frombin']
            };

            getBinQuantity(searchData, parameters, '#curbal');
        },

        afterChangeInvreserveAsset: function (parameters) {
            if (parameters['fields']['asset_.binnum'] && !nullOrEmpty(parameters['fields']['asset_.binnum'])) {
                parameters['fields']['invbalances_.binnum'] = parameters['fields']['asset_.binnum'];
            }
        },
    };

}]);

})(angular);