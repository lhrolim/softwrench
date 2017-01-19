(function (angular) {
    "use strict";

    angular.module('sw_layout')
        .service('inventoryService', [
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
                    const invuse = {};
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
                    const newIssueItem = {};
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
                    const jsonString = angular.toJson(newIssueItem);
                    const httpParameters = {
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
                    const itemnum = parameters['fields']['itemnum'];
                    const siteid = parameters['fields']['siteid'];
                    const storeloc = parameters['fields']['storeloc'];
                    if (itemnum != null && itemnum.trim() != "" &&
                        siteid != null && siteid.trim() != "" &&
                        storeloc != null && storeloc.trim() != "") {
                        const searchData = {
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
                    searchService.searchWithData("invbalances", searchData, "invbalancesList").then(function (response) {
                        const data = response.data;
                        const resultObject = data.resultObject;
                        for (let i = 0; i < resultObject.length; i++) {
                            const fields = resultObject[i];
                            if (fields['binnum'] == binnum && fields['lotnum'] == lotnum) {
                                parameters.fields[balanceField] = fields.curbal;
                                // Exit the loop
                                break;
                            }
                            parameters.fields[balanceField] = null;
                        };
                    });
                };

                //afterchange
                var invIssue_maximo71_afterChangeItem = function (parameters) {
                    if (nullOrEmpty(parameters['fields']['itemnum']) || nullOrEmpty(parameters['fields']['storeloc'])) {
                        return;
                    }
                    const maxvarsSearchData = {
                        varname: 'DEFISSUECOST',
                        siteid: parameters['fields']['siteid']
                    };
                    return searchService.searchWithData("maxvars", maxvarsSearchData).then(function (response) {
                        const maxvarsData = response.data;
                        const resultObject = maxvarsData.resultObject;
                        const fields = resultObject[0];
                        var costtype = fields['varvalue'];
                        parameters['fields']['inventory_.costtype'] = costtype;
                        const itemnum = parameters['fields']['itemnum'];
                        if (nullOrEmpty(itemnum)) {
                            parameters['fields']['itemnum'] = null;
                            parameters['fields']['unitcost'] = null;
                            parameters['fields']['inventory_.issueunit'] = null;
                            parameters['fields']['inventory_.itemtype'] = null;
                            parameters['fields']['#curbal'] = null;
                            return;
                        }
                        const searchData = {
                            itemnum: parameters['fields']['itemnum'],
                            location: parameters['fields']['storeloc'],
                            siteid: parameters['fields']['siteid']
                        };
                        searchService.searchWithData("invcost", searchData).then(function (response) {
                            const data = response.data;
                            const invcostRo = data.resultObject;
                            const invcostFields = invcostRo[0];
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
                        const value = parameters.value;
                        const column = parameters.column;
                        const dm = parameters.datamap;
                        return formatQtyReturned(dm, value, column);
                    },

                    formatQtyList: function (parameters) {
                        const value = parameters.value;
                        const column = parameters.column;
                        const dm = parameters.datamap;
                        return formatQty(dm, value, column);
                    },

                    formatQtyReturnedDetail: function (parameters) {
                        const value = parameters.value;
                        const column = parameters.column;
                        const dm = parameters.datamap;
                        if (dm != undefined) {
                            const formattedValue = formatQtyReturned(dm, value, column);
                            dm[column.attribute] = formattedValue;
                            return formattedValue;
                        }
                        return;
                    },

                    formatQtyDetail: function (parameters) {
                        const value = parameters.value;
                        const column = parameters.column;
                        const dm = parameters.datamap;
                        if (dm != undefined) {
                            const formattedValue = formatQty(dm, value, column);
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
                        const param = {};
                        param.id = datamap['matusetransid'];
                        const application = schema.applicationName;
                        var detail = 'viewinvreturndetail';
                        const mode = 'input';
                        //Logic to determine whether the record is an ISSUE
                        //and whether all of the issued items have been returned
                        if (datamap['issuetype'] == 'ISSUE') {

                            //Sets qtyreturned to 0 if null
                            //Parses the qtyreturned if its in a strng format
                            let qtyreturned = 0;
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
                                    return inventoryServiceCommons.returnConfirmation(null, transformedData).then(() => {
                                        sessionStorage.mockclientvalidation = true;
                                        return applicationService.save({ selecteditem: transformedData, originalDatamap: originalDatamap }).then(data => {
                                            $rootScope.$broadcast(JavascriptEventConstants.RefreshGrid);
                                        }).finally(() => {
                                            sessionStorage.mockclientvalidation = false;
                                        });
                                    });
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
                        const siteid = datamap['siteid'];
                        if (nullOrEmpty(siteid)) {
                            alertService.alert("A Site Id is required.");
                            return;
                        }
                        const storeloc = datamap['storeloc'];
                        if (nullOrEmpty(storeloc)) {
                            alertService.alert("A Storeroom is required.");
                            return;
                        }
                        const refwo = datamap['refwo'];
                        const location = datamap['location'];
                        const assetnum = datamap['assetnum'];
                        const gldebitacct = datamap['gldebitacct'];
                        if (nullOrEmpty(refwo) &&
                            nullOrEmpty(location) &&
                            nullOrEmpty(assetnum) &&
                            nullOrEmpty(gldebitacct)) {
                            alertService.alert("Either a Workorder, Location, Asset, or GL Debit Account is required.");
                            return;
                        }
                        const newDatamap = {};
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
                        const param = {};
                        param.id = datamap['refwo'];
                        redirectService.goToApplicationView('invissuewo', 'newdetail', null, null, param, newDatamap);
                    },

                    submitNewBatchIssue: function (schema, datamap) {
                        const clonedCompositionData = contextService.fetchFromContext('clonedCompositionData', true, true);
                        submitInvIssueRec(datamap, clonedCompositionData, 0);
                    },

                    navToIssueReturnList: function (schema, datamap) {
                        redirectService.goToApplicationView(schema.applicationName, "invIssueList", null, null, null, null);
                    },

                    displayNewIssueModal: function (parentschema, parentdatamap) {
                        const compositionschema = parentschema.cachedCompositions['invissue_'].schemas['detail'];
                        const user = contextService.getUserData();
                        const itemDatamap = {};
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
                        const newDatamap = {};
                        angular.copy(datamap, newDatamap);

                        modalService.show(schema, newDatamap);

                    },

                    cancelNewInvIssueItem: function () {
                        modalService.hide();
                    },

                    addItemToBatch: function (datamap) {
                        const itemtype = datamap['inventory_.item_.itemtype'];
                        const issueto = datamap['issueto'];
                        if (itemtype == 'TOOL' && nullOrEmpty(issueto)) {
                            alertService.alert("Issued To is required when issuing a tool.");
                            return;
                        }
                        const itemnum = datamap['itemnum'];
                        if (nullOrEmpty(itemnum)) {
                            alertService.alert("An item is required.");
                            return;
                        }
                        const clonedCompositionData = contextService.fetchFromContext('clonedCompositionData', true, true);
                        datamap['issuetype'] = "ISSUE";
                        const newissue = angular.copy(datamap);
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
                        const clonedCompositionData = contextService.fetchFromContext('clonedCompositionData', true, true);
                        var compositionItem = null;
                        for (let i = 0; i < clonedCompositionData.length; i++) {
                            compositionItem = clonedCompositionData[i];
                            if (compositionItem['matusetransid'] != null) {
                                continue;
                            }

                            if (compositionItem['itemnum'] == datamap['itemnum']) {
                                break;
                            }
                        }

                        if (compositionItem != null) {
                            const newissue = angular.copy(datamap);
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

                    //afterchange
                    invIssueBatch_afterChangeItem: function (parameters) {
                        const fields = parameters['fields'];
                        const itemnum = fields['itemnum'];
                        fields['binnum'] = null;
                        fields['lotnum'] = null;
                        fields['binbalances_.curbal'] = null;
                        if (nullOrEmpty(itemnum)) {
                            fields['itemnum'] = null;
                            fields['unitcost'] = null;
                            fields['inventory_.issueunit'] = null;
                            fields['inventory_.itemtype'] = null;
                            return;
                        }

                        setBatchIssueBin(parameters);
                        const searchData = {
                            itemnum: fields['itemnum'],
                            location: fields['storeloc'],
                            siteid: fields['siteid'],
                            orgid: fields['orgid'],
                            itemsetid: fields['itemsetid']
                        };
                        searchService.searchWithData("inventory", searchData).then(function (response) {
                            const data = response.data;
                            const resultObject = data.resultObject;
                            const fields = resultObject[0];
                            const costtype = fields['costtype'];
                            fields['inventory_.costtype'] = costtype;
                            var locationFieldName = "";
                            if (fields.storeloc != undefined) {
                                locationFieldName = "storeloc";
                            }
                            //parameters['fields']['inventory_.issueunit'] = fields['issueunit'];
                            inventoryServiceCommons.doUpdateUnitCostFromInventoryCost(parameters, "unitcost", locationFieldName);
                        });
                    },
                    //afterchange
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
                        const binnum = parameters['fields']['#frombin'];
                        const lotnum = parameters['fields']['#fromlot'];
                        const searchData = {
                            itemnum: parameters['fields']['itemnum'],
                            siteid: parameters['fields']['siteid'],
                            itemsetid: parameters['fields']['itemsetid'],
                            location: parameters['fields']['location'],
                            lotnum: parameters['fields']['#fromlot']
                        };
                        getBinQuantity(searchData, parameters, '#curbal', binnum, lotnum);
                    },

                    getTransferBinQuantity: function (parameters) {
                        const binnum = parameters['fields']['invuseline_.frombin'];
                        const lotnum = parameters['fields']['invuseline_.fromlot'];
                        const searchData = {
                            itemnum: parameters['fields']['invuseline_.itemnum'],
                            siteid: parameters['fields']['inventory_.siteid'],
                            itemsetid: parameters['fields']['inventory_.itemsetid'],
                            location: parameters['fields']['fromstoreloc']
                        };
                        getBinQuantity(searchData, parameters, '#curbal', binnum, lotnum);
                    },


                    validateReturn: function (schema, datamap) {
                        const errors = [];
                        const quantityAdj = parseInt(datamap['#quantityadj']);
                        const quantity = datamap['quantity'];
                        const qtyReturned = parseInt(datamap['qtyreturned']);
                        if (quantity - (quantityAdj + qtyReturned) < 0) {
                            errors.push("The quantity being returned cannot be greater than the quantity that was issued.");
                        }
                        return errors;
                    },

                    //afterchange
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

                    //afterchange
                    afterChangeIssueQuantity: function (event) {
                        if (event.fields['#issueqty'] > event.fields['reservedqty']) {
                            alertService.alert("The quantity being transferred cannot be greater than the current balance of the From Bin.");
                            event.fields['#issueqty'] = event.fields['reservedqty'];
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
                        const matusetransDatamap = {
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
                                const reservedQty = Number(datamap["reservedqty"]);
                                const actualQty = Number(datamap["actualqty"]);
                                const issueQty = Number(datamap["#issueqty"]);
                                // Calculate the new reserved and actual values based on the quantity being issued
                                const newReservedQty = reservedQty - issueQty;
                                const newActualQty = actualQty + issueQty;
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
                                    const jsonWrapper = {
                                        json: datamap,
                                        requestData: httpParameters
                                    };
                                    const jsonString = angular.toJson(jsonWrapper);
                                    const urlToUse = url("/api/data/reservedMaterials/" + datamap["requestnum"]);
                                    $http.put(urlToUse, jsonString).then(function () {
                                        // Return to the list of reserved materials
                                        redirectService.goToApplication("reservedMaterials", "reservedMaterialsList", null, null);
                                    }).catch(function () {
                                        // Failed to update the material reservation
                                    });
                                } else {
                                    // If the reserved quantity has reached 0, delete the record
                                    const deleteUrl = url("/api/data/reservedMaterials/" + datamap["requestnum"] + "?" + $.param(httpParameters));
                                    $http.delete(deleteUrl).then(function () {
                                        // Return to the list of reserved materials
                                        redirectService.goToApplication("reservedMaterials", "reservedMaterialsList", null, null);
                                    });
                                }
                            });
                    },

                    onloadReservation: function (scope, schema, datamap) {
                        const parameters = {
                            fields: datamap
                        };
                        inventoryServiceCommons.updateInventoryCosttype(parameters, location);
                        datamap['#issueqty'] = datamap['reservedqty'];
                        datamap['#issuetype'] = "ISSUE";
                        const searchData = {
                            itemnum: parameters['fields']['itemnum'],
                            siteid: parameters['fields']['siteid'],
                            itemsetid: parameters['fields']['itemsetid'],
                            location: parameters['fields']['location'],
                            binnum: parameters['fields']['#frombin']
                        };
                        getBinQuantity(searchData, parameters, '#curbal');
                    },

                    //afterchange
                    afterChangeInvreserveAsset: function (parameters) {
                        if (parameters['fields']['asset_.binnum'] && !nullOrEmpty(parameters['fields']['asset_.binnum'])) {
                            parameters['fields']['invbalances_.binnum'] = parameters['fields']['asset_.binnum'];
                        }
                    },
                };

            }]);

})(angular);