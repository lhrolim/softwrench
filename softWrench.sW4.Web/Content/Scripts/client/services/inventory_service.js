var app = angular.module('sw_layout');

app.factory('inventoryService', function ($http, contextService, redirectService, modalService, searchService, restService, alertService) {
    var createTransaction = function (schema, issueType) {
        var matusetrans = {};
        matusetrans.issueType = issueType;
        contextService.insertIntoContext("matusetrans", matusetrans, false);
        redirectService.goToApplicationView("invissue", "filter", "input", null, null, null);
    };
    var createInvUse = function(schema, useType) {
        var invuse = {};
        invuse.usetype = useType;
        contextService.insertIntoContext("invuse", invuse, false);
        redirectService.goToApplicationView("invuse", "newdetail", "Input", null, null, null);
    };
    return {
        createIssue: function (schema) {
            if (schema === undefined) {
                return;
            }

            createTransaction(schema, "ISSUE");
        },
        test: function(schema, datamap) {
            redirectService.goToApplicationView("invissue", "detail", "Input", null, null, null);
        },
        createReturn: function(schema) {
            if (schema === undefined) {
                return;
            }

            createTransaction(schema, "RETURN");
        },
        afterchangeworkorder: function (parameters) {

            if (parameters.fields['workorder_.location'] == null) {
                parameters.fields['workorder_.location'] = " ";
                parameters.fields['location'] = " ";
            } else {
                parameters.fields['location'] = parameters.fields['workorder_.location'];
            }

            if (parameters.fields['workorder_.assetnum'] == null) {
                parameters.fields['workorder_.assetnum'] = " ";
                parameters.fields['assetnum'] = " ";
            } else {
                parameters.fields['assetnum'] = parameters.fields['workorder_.assetnum'];
            }

            var gldebitacct = parameters.fields['gldebitacct'];


            if (parameters.fields['workorder_.glaccount']) {
                gldebitacct = parameters.fields['workorder_.glaccount'];
            }

            parameters.fields['gldebitacct'] = gldebitacct;
        },
        invissuelistclick: function(datamap, schema) {
            var param = {};
            param.id = datamap['matusetransid'];
            var application = 'invissue';
            var detail = 'viewinvreturndetail';
            var mode = 'output';

            if (datamap['issuetype'] == 'ISSUE') {
                if (typeof datamap['qtyreturned'] === "string") {
                    datamap['qtyreturned'] = parseInt(datamap['qtyreturned']);
                }
                if (datamap['qtyreturned'] + datamap['quantity'] < 0) {
                    detail = 'editinvissuedetail';
                    mode = 'input';
                } else {
                    detail = 'viewinvissuedetail';
                }
            }

            datamap['quantity'] = Math.abs(datamap['quantity']);

            redirectService.goToApplicationView(application, detail, mode, null, param, null);
        },
        
        editinvissuewo: function(schema, datamap) {
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
        afterchangeinvissueitem: function (parameters) {
            var user = contextService.getUserData();
            var searchData = {
                itemnum: parameters['fields']['itemnum'],
                location: parameters['fields']['location'],
                //siteid: user.siteId,
                //orgid: user.orgId,
                //status: "ACTIVE"
            };
            var searchDTO = searchService.buildSearchDTO(searchData, {}, {}, null);
            searchDTO.pageNumber = 1;
            searchDTO.totalCount = 0;
            searchDTO.pageSize = 30;
            var restParameters = {
                key: {
                    schemaId: "list",
                    mode: "none",
                    platform: "web"
                },
                SearchDTO: searchDTO
            };
            var urlToUse = url("/api/Data/inventory?" + $.param(restParameters));
            $http.get(urlToUse).success(function (data) {
                var resultObject = data.resultObject;
                var fields = resultObject[0].fields;
                var costtype = parameters['fields']['inventory_.costtype'];
                if (costtype === 'STANDARD') {
                    parameters.fields['unitcost'] = fields.stdcost;
                } else if (costtype === 'AVERAGE') {
                    parameters.fields['unitcost'] = fields.avgcost;
                }
            });
        },
        invIssue_afterChangeAsset: function (parameters) {
            if(parameters.fields['assetnum'].trim() != "") {
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
                if(parameters.fields['location'].trim() != "") {
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
	createTransfer: function(schema) {
            if (schema === undefined) {
                return;
            }
            createInvUse(schema, "TRANSFER");
        },
        afterChangeLocation: function (parameters) {
            var searchData = {
                itemnum: parameters['fields']['invuseline_.itemnum'],
                location: parameters['fields']['fromstoreloc']
            };
            var searchDTO = searchService.buildSearchDTO(searchData, {}, {}, null);
            searchDTO.pageNumber = 1;
            searchDTO.totalCount = 0;
            searchDTO.pageSize = 30;
            var restParameters = {
                key: {
                    schemaId: "list",
                    mode: "none",
                    platform: "web"
                },
                SearchDTO: searchDTO
            };
            var urlToUse = url("/api/Data/invcost?" + $.param(restParameters));
            $http.get(urlToUse).success(function (data) {
                var resultObject = data.resultObject;
                var fields = resultObject[0].fields;
                var costtype = parameters['fields']['inventory_.costtype'];
                if (costtype === 'STANDARD')
                {
                    parameters.fields['invuseline_.unitcost'] = fields.stdcost;
                }
                else if (costtype === 'AVERAGE') {
                    parameters.fields['invuseline_.unitcost'] = fields.avgcost;
                }
            });
        },
        getBinQuantity: function (parameters) {
            var searchData = {
                itemnum: parameters['fields']['invuseline_.itemnum'],
                siteid: parameters['fields']['inventory_.siteid'],
                itemsetid: parameters['fields']['inventory_.itemsetid'],
                location: parameters['fields']['fromstoreloc'],
                binnum: parameters['fields']['invuseline_.frombin']
            }
            var searchDTO = searchService.buildSearchDTO(searchData, {}, {}, null);
            searchDTO.pageNumber = 1;
            searchDTO.totalCount = 0;
            searchDTO.pageSize = 30;
            var restParameters = {
                key: {
                    schemaId: "list",
                    mode: "none",
                    platform: "web"
                },
                SearchDTO: searchDTO
            }
            var urlToUse = url("/api/Data/invbalances?" + $.param(restParameters));
            $http.get(urlToUse).success(function (data) {
                var resultObject = data.resultObject;
                var fields = resultObject[0].fields;
                parameters.fields['#curbal'] = fields.curbal;
            });
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
            redirectService.goToApplication("matrectransTransfers", "list", null, null);
        },
        cancelTransfer: function () {
            redirectService.goToApplicationView("matrectransTransfers", "list", null, null, null, null);
        },
	afterChangeTransferQuantity: function (event) {
            if (event.fields['invuseline_.quantity'] > event.fields['#curbal']) {
                alertService.alert("The quantity being transferred cannot be greater than the current balance of the From Bin.");
                event.scope.datamap['invuseline_.quantity'] = event.fields['#curbal'];
            }
        },

    };
});