
(function (angular) {
    'use strict';

    angular.module('sw_layout').factory('invissueService', ["$rootScope", "$log", 'searchService', "inventoryServiceCommons", "redirectService", "alertService", "inventorySharedService", "applicationService", invissueService]);

    function invissueService($rootScope, $log, searchService, inventoryServiceCommons, redirectService, alertService, inventorySharedService, applicationService) {
        const service = {
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
            const param = {
                id: datamap['matusetransid']
            };
            const application = schema.applicationName;
            const mode = 'input';
            const schemaId = inventorySharedService.nextInvIssueDetailSchema(schema.applicationName, schema, datamap);

            if (!!schemaId) {
                //in that case, we will just bring the details page.
                redirectService.goToApplicationView(application, schemaId, mode, null, param, null);
                return;
            }

            //TODO: what´s the idea here? just one item?
            var transformedData = angular.copy(datamap);
            transformedData['#quantityadj'] = 1;
            inventoryServiceCommons.returnTransformation(null, transformedData);
            // Get the cost type

            inventoryServiceCommons.returnConfirmation(null, transformedData).then(() => {
                inventoryServiceCommons.updateInventoryCosttype({ fields: transformedData }, 'storeloc').then((dm) => {

                    var originalDatamap = {
                        fields: datamap,
                    };
                    return applicationService.save({ refresh: true, selecteditem: dm.fields, originalDatamap: originalDatamap }).then(data => {
                        $rootScope.$broadcast(JavascriptEventConstants.RefreshGrid);
                    }).finally(() => {
                        sessionStorage.mockclientvalidation = false;
                    });
                });
            });
        };


        function validateInvIssue(schema, datamap) {
            const errors = [];
            const refwo = datamap['refwo'];
            const location = datamap['location'];
            const assetnum = datamap['assetnum'];
            const gldebitacct = datamap['gldebitacct'];
            const itemtype = datamap['inventory_.item_.itemtype'];
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
            const fields = parameters.fields;
            if (nullOrEmpty(fields['location'])) {
                fields['refwo'] = null;
                fields['workorder'] = null;
                fields['assetnum'] = null;
            }
        };

        function afterChangeCurbal(parameters) {
            const fields = parameters['fields'];
            fields['#curbal'] = fields['binbalances_.curbal'];
            fields['lotnum'] = fields['binbalances_.lotnum'];
        };

        function afterChangeLotnum(parameters) {
            const fields = parameters['fields'];
            fields['#curbal'] = fields['binbalances_.curbal'];
            fields['lotnum'] = fields['binbalances_.lotnum'];
        }

        function afterChangeLaborCode(parameters) {
            const fields = parameters.fields;
            if (fields['labor_']) {
                fields['issueto'] = fields['labor_']['personid'];
            }
        };

        function afterChangeWorkorder(parameters) {
            const fields = parameters.fields;
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
            const fields = parameters.fields;
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
            const fields = parameters.fields;
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
                return;
            };
            // If the binbalances_ record is not filled but the binnum is
            // (binnum filled after itemnum change) then use the available 
            // fields to find an applicable lotnum and curbal. If the binnum
            // has been cleared, clear the lot and curbal
            if (fields['binnum'] != null && fields['binnum'] != " ") {
                const searchData = {
                    orgid: fields['orgid'],
                    siteid: fields['siteid'],
                    itemnum: fields['itemnum'],
                    location: fields['storeloc'],
                    binnum: fields['binnum']
                };
                return searchService.searchWithData("invbalances", searchData, "invbalancesList").then(function (response) {
                    const data = response.data;
                    const resultObject = data.resultObject;
                    const resultFields = resultObject[0];
                    const lotnum = resultFields['lotnum'];
                    const curbal = resultFields['curbal'];
                    fields['lotnum'] = lotnum;
                    fields['#curbal'] = curbal == null ? 0 : curbal;
                    fields['curbal'] = curbal == null ? 0 : curbal;
                    fields['binbalances_.lotnum'] = lotnum;
                    fields['binbalances_.curbal'] = curbal == null ? 0 : curbal;
                    fields['quantity'] = 1;
                });
            }
            else {
                fields['lotnum'] = null;
                fields['#curbal'] = null;
            }
        };


        function afterchangeItem(parameters) {
            const fields = parameters['fields'];
            fields['lotnum'] = null;
            fields['#curbal'] = null;
            const itemnum = fields['itemnum'];
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
            const fields = parameters['fields'];
            const parentdata = parameters['parentdata'];
            if (fields['storeloc'] == null) {
                nullifyProperties(fields, ['unitcost', 'binnum', 'inventory_.issueunit', '#curbal', 'matusetransid']);
                return;
            }

            fields['lotnum'] = null;
            fields['#curbal'] = null;
            const invParams = {
                itemnum: parentdata['itemnum'],
                storeloc: fields['storeloc'],
                siteid: parentdata['siteid'],
                orgid: parentdata['orgid'],
                itemsetid: fields['itemsetid']
            };
            updateInventoryCosttype(parameters, 'storeloc');
        }

        function doUpdateUnitCostFromInventoryCost(parameters, unitCostFieldName, locationFieldName) {
            var fields = parameters['fields'];
            const searchData = {
                itemnum: parameters['parentdata']['itemnum'],
                location: fields[locationFieldName],
                siteid: parameters['parentdata']['siteid']
            };
            searchService.searchWithData("invcost", searchData).then(function (response) {
                const data = response.data;
                const resultObject = data.resultObject;
                const resultFields = resultObject[0].fields;
                const costtype = fields['inventory_.costtype'];
                if (costtype === 'STANDARD') {
                    parameters.fields[unitCostFieldName] = resultFields.stdcost;
                } else if (costtype === 'AVERAGE') {
                    parameters.fields[unitCostFieldName] = resultFields.avgcost;
                }
            });
        };

        function updateInventoryCosttype(parameters, storelocation) {
            const searchData = {
                itemnum: parameters['parentdata']['itemnum'],
                location: parameters['fields'][storelocation],
                siteid: parameters['parentdata']['siteid'],
                orgid: parameters['parentdata']['orgid'],
                itemsetid: parameters['fields']['itemsetid']
            };
            searchService.searchWithData("inventory", searchData).then(function (response) {
                const data = response.data;
                const resultObject = data.resultObject;
                const fields = resultObject[0].fields;
                const costtype = fields['costtype'];
                if (costtype.equalIc("fifo") || costtype.equalIc("lifo")) {
                    // TODO: Add support for FIFO / LIFO cost types
                    alertService.error("FIFO and LIFO cost types are not supported at this time");
                    return;
                }
                parameters['fields']['inventory_.costtype'] = costtype;
                doUpdateUnitCostFromInventoryCost(parameters, "unitcost", storelocation);
            });
        };
    }

})(angular);