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

    var setBatchIssueBin = function(parameters) {
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

    var getBinQuantity = function (searchData, parameters, balanceField, binnum) {
        searchService.searchWithData("invbalances", searchData).success(function (data) {
            var resultObject = data.resultObject;
            resultObject.forEach(function(row) {
                var fields = row['fields'];
                if (fields['binnum'] == binnum) {
                    parameters.fields[balanceField] = fields.curbal;
                    return;
                }
            });
        });
    };

    var doUpdateUnitCostFromInventoryCost = function(parameters, unitCostFieldName) {
        var searchData = {
            itemnum: parameters['fields']['itemnum'],
            location: parameters['fields']['storeloc'],
            siteid: parameters['fields']['siteid']
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
        navToBatchFilter: function () {
            redirectService.goToApplicationView("invissue", "batchInvIssueFilter", "input", null, null, null);
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

        navToBatchIssueDetail: function (schema, datamap) {
            var siteid = datamap['siteid'];
            
            if (siteid == null || siteid.trim() == "") {
                alertService.alert("A Site Id is required.");
                return;
            }
    
            var storeloc = datamap['storeloc'];

            if (storeloc == null || storeloc.trim() == "") {
                alertService.alert("A Storeroom is required.");
                return;
            }

            var refwo = datamap['refwo'];
            var location = datamap['location'];
            var assetnum = datamap['assetnum'];
            var gldebitacct = datamap['gldebitacct'];

            if ((refwo == null || storeloc.trim() == "") &&
                (location == null || location.trim() == "") &&
                (assetnum == null || assetnum.trim() == "") &&
                (gldebitacct == null || gldebitacct.trim() == "")) {
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
        submitNewInvIssue: function (schema, datamap, saveFn) {
            var newRecord = {};
            newRecord['itemnum'] = "Z-RAGS";
            newRecord['quantity'] = 1;
            newRecord['item_.description'] = "Z-RAGS";
            newRecord['issuetype'] = "ISSUE";
            newRecord['matusetransid'] = null;
            newRecord['assetnum'] = "2025";
            
            var user = contextService.getUserData();
            newRecord['siteid'] = user.siteid;
            datamap.push(newRecord);
        },
        cancelNewInvIssue: function () {
            redirectService.goToApplicationView("invissue", "list", null, null, null, null);
        },
        displayNewIssueModal: function (parentschema, parentdatamap) {
            var clonedCompositionData = parentdatamap['invissue_'];
            var compositionschema = parentschema.cachedCompositions['invissue_'].schemas['detail'];
            var parentdata = parentdatamap;
            var user = contextService.getUserData();
            var itemDatamap = {};
            itemDatamap['itemnum'] = null;
            itemDatamap['enterby'] = user.login.toUpperCase();
            itemDatamap['siteid'] = user.siteId;
            itemDatamap['matusetransid'] = null;
            itemDatamap['refwo'] = parentdata['#refwo'];
            itemDatamap['assetnum'] = parentdata['#assetnum'];
            itemDatamap['issuetype'] = parentdata['#issuetype'];
            itemDatamap['issueto'] = parentdata['#issueto'];
            itemDatamap['location'] = parentdata['#location'];
            itemDatamap['storeloc'] = parentdata['#storeloc'];
            itemDatamap['gldebitacct'] = parentdata['#gldebitacct'];

            contextService.insertIntoContext('clonedCompositionData', clonedCompositionData, false);
            modalService.show(compositionschema, itemDatamap, null);
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
        addItemToBatch: function (datamap) {
            var test = contextService.fetchFromContext('clonedCompositionData', true, false);
            if (test['invissue_'] == null) {
                test['invissue_'] = [];
            }
            var newissue = {};
            newissue.matusetransid = "";
            newissue.assetnum = "2235";
            newissue.itemnum = "Z-RAGS";
            test['invissue_'].push(newissue);
            modalService.hide();
        },
        invIssue_afterChangeWorkorder: function (parameters) {
            if (parameters.fields['refwo'] == null || parameters.fields['refwo'].trim() == "") {
                parameters.fields['refwo'] = null;
                parameters.fields['location'] = null;
                parameters.fields['assetnum'] = null;
                parameters.fields['gldebitacct'] = null;
                return;
            }
            
            // If the workorder's location is null, remove the current datamap's location
            if (parameters.fields['workorder_.location'] == null) {
                parameters.fields['location'] = null;
            } else {
                parameters.fields['location'] = parameters.fields['workorder_.location'];
            }

            // If the workorder's assetnum is null, remove the current datamap's assetnum
            if (parameters.fields['workorder_.assetnum'] == null) {
                parameters.fields['assetnum'] = null;
            } else {
                parameters.fields['assetnum'] = parameters.fields['workorder_.assetnum'];
            }

            // If the workorder's GL account is null, remove the current datamap's GL Debit Acct
            if (parameters.fields['workorder_.glaccount'] == null) {
                parameters.fields['gldebitacct'] = null;
            } else {
                parameters.fields['gldebitacct'] = parameters.fields['workorder_.glaccount'];
            }

            return;
        },

        afterChangeStoreroom: function (parameters) {
            doUpdateUnitCostFromInventoryCost(parameters,'unitcost');
        },
        afterchangeinvissueitem: function (parameters) {
            doUpdateUnitCostFromInventoryCost(parameters, 'unitcost');
        },

        invUse_afterChangeFromStoreroom: function (parameters) {
            doUpdateUnitCostFromInventoryCost(parameters, 'invuseline_.unitcost');
            var itemnum = parameters['fields']['invuseline_.itemnum'];
            var siteid = parameters['fields']['siteid'];
            var fromstoreloc = parameters['fields']['fromstoreloc'];
            if (itemnum !== undefined && itemnum.trim() != "" &&
                siteid !== undefined && siteid.trim() != "" && 
                fromstoreloc !== undefined && fromstoreloc.trim() != "") {
                var searchData = {
                    itemnum: itemnum,
                    siteid: siteid,
                    itemsetid: parameters['fields']['inventory_.itemsetid'],
                    location: parameters['fields']['fromstoreloc']
                };
                getBinQuantity(searchData, parameters, '#curbal', null);
            } else {
                parameters['fields']['invuseline_.tobin'] = null;
                parameters['fields']['#curbal'] = null;
                parameters['fields']['invuseline_.frombin'] = null;
            }
        },

        invIssue_afterChangeItem: function(parameters) {
            setBatchIssueBin(parameters);
        },

        batchinvIssue_afterChangeBin: function(parameters) {
            var itemnum = parameters['fields']['itemnum'];
            var siteid = parameters['fields']['siteid'];
            var storeloc = parameters['fields']['storeloc'];
            var binnum = parameters['fields']['binnum'];
            if (binnum != null && binnum.trim() != "") {
                if (itemnum != null && itemnum.trim() != "" &&
                    siteid != null && siteid.trim() != "" &&
                    storeloc != null && storeloc.trim() != "") {
                    var searchData = {
                        itemnum: itemnum,
                        siteid: siteid,
                        location: parameters['fields']['storeloc']
                    };
                    getBinQuantity(searchData, parameters, '#curbal', binnum);
                } else {
                    parameters['fields']['#curbal'] = null;
                }
            } else {
                setBatchIssueBin(parameters);
            }
            
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
            var binnum = parameters['fields']['invuseline_.frombin'];
            var searchData = {
                itemnum: parameters['fields']['itemnum'],
                siteid: parameters['fields']['siteid'],
                itemsetid: parameters['fields']['inventory_.itemsetid'],
                location: parameters['fields']['storeloc']
            };
            getBinQuantity(searchData, parameters, '#curbal', binnum);
        },
        invUse_afterChangeFromBin: function (parameters) {

            if (parameters['fields']['invuseline_.frombin'] == null ||
                parameters['fields']['invuseline_.frombin'].trim() == "") {
                parameters['fields']['#curbal'] = null;
                return;
            }

            var binnum = parameters['fields']['invuseline_.frombin'];
            var searchData = {
                itemnum: parameters['fields']['invuseline_.itemnum'],
                siteid: parameters['fields']['inventory_.siteid'],
                itemsetid: parameters['fields']['inventory_.itemsetid'],
                location: parameters['fields']['fromstoreloc']
            };
            getBinQuantity(searchData, parameters, '#curbal', binnum);
            return;
        },
        invUse_afterChangeItem: function (parameters) {

            if (parameters['fields']['invuseline_.itemnum'] == null ||
                parameters['fields']['invuseline_.itemnum'].trim() == "") {
                parameters['fields']['fromstoreloc'] = null;
                return;
            }

        },
        invUse_afterChangeSite: function (parameters) {

            if (parameters['fields']['invuseline_.siteid'] == null ||
                parameters['fields']['invuseline_.siteid'].trim() == "") {
                parameters['fields']['invuseline_.itemnum'] = null;
                parameters['fields']['invuseline_.tostoreloc'] = null;
                parameters['fields']['invuseline_.tobin'] = null;
                return;
            }

        },
        submitTransfer: function (schema, datamap) {
            // Save transfer
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
        overrideGlAccount: function(event) {
            if (event.fields['#gldebitacct'] != null || event.fields['gldebitacct'].trim != "") {
                event.fields['gldebitacct'] = event.fields['#gldebitacct'];
            }
        }

    };
});