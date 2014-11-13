var app = angular.module('sw_layout');

app.factory('inventoryService', function ($http, contextService, redirectService, modalService, searchService, restService, alertService) {
    var formatQty = function (datamap, value, column) {
        if (datamap['issuetype'] == 'ISSUE') {
            if (datamap[column.attribute] != null) {
                return Math.abs(datamap[column.attribute]);
            }
        }
        return datamap[column.attribute];
    };

    var formatQtyReturned = function (datamap, value, column) {
        if (datamap['issuetype'] == 'ISSUE') {
            if (datamap[column.attribute] == null) {
                return 0;
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

    var getBinQuantity = function (searchData, parameters, balanceField) {
        searchService.searchWithData("invbalances", searchData).success(function (data) {
            var resultObject = data.resultObject;
            var fields = resultObject[0].fields;
            parameters.fields[balanceField] = fields.curbal;
        });
    };

    var doUpdateUnitCostFromInventoryCost = function(parameters, unitCostFieldName) {
        var searchData = {
            itemnum: parameters['fields']['itemnum'],
            location: parameters['fields']['storeloc'],
        };
        searchService.searchWithData("invcost", searchData).success(function(data) {
            var resultObject = data.resultObject;
            var fields = resultObject[0].fields;
            var costtype = parameters['fields']['inventory_.costtype'];
            if (costtype === 'STANDARD') {
                parameters.fields[unitCostFieldName] = fields.stdcost;
            } else if (costtype === 'AVERAGE') {
                parameters.fields[unitCostFieldName] = fields.avgcost;
            }
        });
    };

    return {
        createIssue: function () {
            redirectService.goToApplicationView("invissue", "newInvIssueDetail", "input", null, null, null);
        },
        navToBulkFilter: function () {
            redirectService.goToApplicationView("invissue", "filter", "input", null, null, null);
        },
        formatQtyReturnedList: function (parameters) {
            var value = parameters.value;
            var column = parameters.column;
            var dm = parameters.datamap;
            if (dm != undefined) {
                if (dm.fields != undefined) {
                    var dm = parameters.datamap.fields;
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
                    var dm = parameters.datamap.fields;
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
                    var dm = parameters.datamap.fields;
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
                    var dm = parameters.datamap.fields;
                }
                var formattedValue = formatQty(dm, value, column);
                dm[column.attribute] = formattedValue;
                return formattedValue;
            }
            return;
        },


        returnInvIssue: function (matusetransitem) {
            var returnQty = matusetransitem['#quantityadj'];
            var item = matusetransitem['itemnum'];
            var storeloc = matusetransitem['storeloc'];
            var binnum = matusetransitem['binnum'];
            var message = "Return (" + returnQty + ") " + item + " to " + storeloc + "?";
            if (binnum != null) {
                message = message + " (Bin: " + binnum + ")";
            }
            alertService.confirm(null, null, function () {
                var newReturnItem = angular.copy(matusetransitem);
                newReturnItem['issueid'] = matusetransitem['matusetransid'];
                newReturnItem['matusetransid'] = null;
                newReturnItem['rowstamp'] = null;
                newReturnItem['quantity'] = matusetransitem['#quantityadj'];
                newReturnItem['issuetype'] = 'RETURN';
                newReturnItem['qtyreturned'] = null;
                newReturnItem['qtyrequested'] = matusetransitem['#quantityadj'];

                var jsonString = angular.toJson(newReturnItem);
                var httpParameters = {
                    application: "invissue",
                    platform: "web",
                    currentSchemaKey: "editinvissuedetail.input.web"
                };
                restService.invokePost("data", "post", httpParameters, jsonString, function () {
                    redirectService.goToApplicationView("invissue", "list", null, null, null, null);
                });
                modalService.hide();
            }, message, function () {
                modalService.hide();
            });
        },
        invissuelistclick: function (datamap, schema) {
            var param = {};
            param.id = datamap['matusetransid'];
            var application = 'invissue';
            var detail = 'viewinvreturndetail';
            var mode = 'output';

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
                    detail = 'editinvissuedetail';
                    mode = 'input';
                } else {
                    //If all of the items have been returned, show the viewdetail page for 'ISSUE' records
                    detail = 'viewinvissuedetail';
                }
            }

            redirectService.goToApplicationView(application, detail, mode, null, param, null);
        },

        editinvissuewo: function (schema, datamap) {
            var newDatamap = {};
            newDatamap['#assetnum'] = datamap['assetnum'];
            newDatamap['#issueto'] = datamap['issueto'];
            newDatamap['#issuetype'] = datamap['issuetype'];
            newDatamap['#location'] = datamap['location'];
            newDatamap['#refwo'] = datamap['refwo'];
            newDatamap['#storeloc'] = datamap['storeloc'];
            newDatamap['location'] = datamap['location'];
            newDatamap['assetnum'] = datamap['assetnum'];
            newDatamap['issueto'] = datamap['#issueto'];
            newDatamap['invissue_'] = [];

            var param = {};
            param.id = datamap['refwo'];
            redirectService.goToApplicationView('invissuewo', 'newdetail', null, null, param, newDatamap);
        },
        submitNewInvIssue: function (schema, datamap, saveFn) {
            modalService.show(schema.compositiondetailschema, datamap, saveFn);
        },
        cancelNewInvIssue: function () {
            redirectService.goToApplicationView("invissue", "invissuelist", null, null, null, null);
        },
        displayPopupModal: function (parentschema, parentdatamap) {
            var compositionschema = parentschema.cachedCompositions['invissue_'].schemas['detail'];
            var parentdata = parentdatamap['fields'];
            var user = contextService.getUserData();
            var itemDatamap = {};
            itemDatamap['itemnum'] = "";
            itemDatamap['enterby'] = user.login;
            itemDatamap['siteid'] = user.siteId;
            itemDatamap['matusetransid'] = null;
            itemDatamap['refwo'] = parentdata['#refwo'];
            itemDatamap['assetnum'] = parentdata['#assetnum'];
            itemDatamap['issuetype'] = parentdata['#issuetype'];
            itemDatamap['issueto'] = parentdata['#issueto'];
            itemDatamap['location'] = parentdata['#location'];
            itemDatamap['storeloc'] = parentdata['#storeloc'];
            var compositiondata = parentdatamap['fields']['invissue_'];

            modalService.show(compositionschema, itemDatamap, null, compositiondata);

        },
        cancelNewInvIssueItem: function () {
            modalService.hide();
        },
        addItemToInvIssue: function (schema, datamap, parentdata, clonedcompositiondata, originalDatamap, previousdata) {
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
        },
        afterchangeworkorder: function (parameters) {
            // If the new work order has a location assigned to it, fill the location on the invissue
            if (parameters.fields['workorder_.location'] == null) {
                parameters.fields['location'] = null;
            } else {
                parameters.fields['location'] = parameters.fields['workorder_.location'];
            }

            // If the new workorder has an asset assigned to it, fill the asset num on the invissue
            if (parameters.fields['workorder_.assetnum'] == null) {
                parameters.fields['assetnum'] = null;
            } else {
                parameters.fields['assetnum'] = parameters.fields['workorder_.assetnum'];
            }

            // If the new work order has a gldebitaccnt, fill the gldebitacct on the invissue
            if (parameters.fields['workorder_.glaccount']) {
                parameters.fields['gldebitacct'] = parameters.fields['workorder_.glaccount'];
            }
        },
        afterChangeStoreroom: function (parameters) {
            doUpdateUnitCostFromInventoryCost(parameters,'unitcost');
        },
        afterchangeinvissueitem: function (parameters) {
            doUpdateUnitCostFromInventoryCost(parameters, 'unitcost');
        },

        afterChangeLocation: function (parameters) {
            doUpdateUnitCostFromInventoryCost(parameters, 'invuseline_.unitcost');
        },

        invIssue_afterChangeAsset: function (parameters) {
            //Sets the associated GL Debit Account
            //if a workorder isn't already specified
            //Updates the location field from the asset's location
            if (parameters.fields['assetnum'].trim() != "") {
                var refwo = parameters.fields['refwo'];
                var location = parameters.fields['location'];

                if (!refwo || refwo.trim() == "") {
                    refwo = "";
                }
                if (!location || location.trim() == "") {
                    location = "";
                }

                if (refwo != "") {
                    return;
                }

                if (refwo != "" && location == "") {
                    parameters.fields['location'] = parameters.fields['asset_.location'];
                    return;
                }

                if (refwo == "") {
                    parameters.fields['location'] = parameters.fields['asset_.location'];
                    parameters.fields['gldebitacct'] = parameters.fields['asset_.glaccount'];
                }
            }
        },
        invIssue_afterChangeLocation: function (parameters) {
            //Sets the gldebitacct and clears the asset 
            //if there is no refwo defined
            if (parameters.fields['location'].trim() != "") {
                var refwo = parameters.fields['refwo'];

                if (!refwo || refwo.trim() == "") {
                    refwo = "";
                }

                if (refwo != "")
                    return;

                parameters.fields['assetnum'] = "";
                parameters.fields['gldebitacct'] = parameters.fields['location_.glaccount'];
            }

        },

        createTransfer: function (schema) {
            if (schema === undefined) {
                return;
            }
            createInvUse(schema, "TRANSFER");
        },
        

        getIssueBinQuantity: function (parameters) {
            var searchData = {
                itemnum: parameters['fields']['itemnum'],
                siteid: parameters['fields']['siteid'],
                itemsetid: parameters['fields']['inventory_.itemsetid'],
                location: parameters['fields']['storeloc'],
                binnum: parameters['fields']['binnum']
            };
            getBinQuantity(searchData, parameters, '#curbal');
        },
        getTransferBinQuantity: function (parameters) {
            var searchData = {
                itemnum: parameters['fields']['invuseline_.itemnum'],
                siteid: parameters['fields']['inventory_.siteid'],
                itemsetid: parameters['fields']['inventory_.itemsetid'],
                location: parameters['fields']['fromstoreloc'],
                binnum: parameters['fields']['invuseline_.frombin']
            };
            getBinQuantity(searchData, parameters, '#curbal');
        },
        submitTransfer: function (schema, datamap) {
            // Save transfer
            var user = contextService.getUserData();

            var jsonString = angular.toJson(datamap);
            var httpParameters = {
                application: "invuse",
                platform: "web",
                currentSchemaKey: "newdetail.input.web"
            };
            restService.invokePost("data", "post", httpParameters, jsonString, function () {
                var restParameters = {
                    key: {
                        schemaId: "list",
                        mode: "none",
                        platform: "web"
                    },
                    SearchDTO: null
                }
                var urlToUse = url("/api/Data/matrectransTransfers?" + $.param(restParameters));
                $http.get(urlToUse).success(function (data) {
                    redirectService.goToApplication("matrectransTransfers", "list", null, data);
                });
            });
        },

        cancelTransfer: function () {
            redirectService.goToApplication("matrectransTransfers", "list");
        },
   
        afterChangeTransferQuantity: function (event) {
            if (event.fields['invuseline_.quantity'] > event.fields['#curbal']) {
                alertService.alert("The quantity being transferred cannot be greater than the current balance of the From Bin.");
                event.scope.datamap['invuseline_.quantity'] = event.fields['#curbal'];
            }
        },

    };
});