var app = angular.module('sw_layout');

app.factory('imacservice', function ($http, $rootScope, alertService, fieldService, redirectService, i18NService, associationService) {

    "ngInject";

    /*
    * The location could point either to a building only, or to the entire building+floor+room, but never straight 
    * to the floor alone as it does not exist as an entry on hapag database.
    *
    *EX: DE-34F-0017/BLDG --> Building
    *DE-34F-0017/FL:0008/RO:0002 --> Room
    */
    var parseLocations = function (datamap, location, triggerparams) {
        if (location == null) {
            datamap['building'] = datamap['floor'] = datamap['room'] = "$null$ignorewatch";
            datamap['buildingOriginal'] = null;
            return;
        }
        var idxBldg = location.indexOf('/BLDG');
        if (idxBldg != -1) {
            //point straight to building only
            datamap['building'] = location.substring(0, idxBldg) + '$ignorewatch';
            datamap['floor'] = datamap['room'] = "$null$ignorewatch";
            datamap['buildingOriginal'] = null;
        } else {
            var parts = location.split("/");
            if (parts.length != 3) {
                //this should never at hapag´s... but let´s play safe
                datamap['building'] = datamap['floor'] = datamap['room'] = "$null$ignorewatch";
                datamap['buildingOriginal'] = null;
                return;
            }
            var shouldIgnoreWatch = triggerparams.dispatchedbytheuser && triggerparams.phase != 'initial';
            datamap['building'] = parts[0] + (shouldIgnoreWatch ? '$ignorewatch' : '');
            datamap['floor'] = parts[1].substring(parts[1].indexOf(':') + 1) + (shouldIgnoreWatch ? '$ignorewatch' : '');
            datamap['room'] = location;
            datamap['buildingOriginal'] = parts[0];
        }


    };

    var setOriginalDataFromAttribute = function (schema, datamap, associationOptions, attributeName) {
        if (datamap[attributeName] != null && datamap[attributeName] != "$null$ignorewatch") {
            var fo = associationService.getFullObjectByAttribute(attributeName, schema, datamap, associationOptions);
            if (fo != null) {
                datamap['#' + attributeName + 'Original_label'] = fo.label;
            }
        } else {
            datamap['#' + attributeName + 'Original_label'] = null;
        }
    }

    var checkCostCenterAvailability = function (availablecostcenters, costCenter, field) {
        if (!availablecostcenters) {
            //asset deselected
            return false;
        }
        for (var i = 0; i < availablecostcenters.length; i++) {
            if (availablecostcenters[i][field] == costCenter) {
                return true;
            }
        }
        return false;
    }


    var setOriginalData = function (schema, datamap, associationOptions) {
        setOriginalDataFromAttribute(schema, datamap, associationOptions, 'building');
        setOriginalDataFromAttribute(schema, datamap, associationOptions, 'floor');
        setOriginalDataFromAttribute(schema, datamap, associationOptions, 'room');

        //for child assets
        setOriginalDataFromAttribute(schema, datamap, associationOptions, 'building2');
        setOriginalDataFromAttribute(schema, datamap, associationOptions, 'floor2');
        setOriginalDataFromAttribute(schema, datamap, associationOptions, 'room2');
        setOriginalDataFromAttribute(schema, datamap, associationOptions, 'costcenter');

    }

    return {



        beforeChangeAsset: function (event) {
            var datamap = event.fields;
            datamap['building'] = "$null$ignorewatch";
            datamap['floor'] = "$null$ignorewatch";
            datamap['room'] = "$null$ignorewatch";
            datamap['costcenter'] = null;
        },

        //R0038 implementation
        afterUsageChange: function (event) {
            var dm = event.fields;
            if (dm.usage.equalsAny('EDUCATION', 'POOL', 'MEETING')) {
                var selectedUser = associationService.getFullObjectByAttribute("userid", event.scope.schema, dm, event.scope.associationOptions);
                if (selectedUser) {
                    //setting here cause it will be removed later since the field will become invisible
                    dm["#userid_label"] = selectedUser.label;
                }
                dm["#newuserid"] = "";
            } else {
                dm["#newuserid"] = null;
                dm["#userid_label"] = null;
            }
        },


        afterChangeAsset: function (event) {
            var userId = event.fields['asset_.aucisowner_.person_.personid'];
            var costCenter = event.fields['asset_.assetglaccount_.glaccount'];
            var costCenterLabel = event.fields['asset_.assetglaccount_.displaycostcenter'];
            var currentITC = event.fields['asset_.primaryuser_.hlagdisplayname'];
            var assetLocation = event.fields['asset_.location_.description'];
            var assetUsage = event.fields['asset_.usage'];
            event.fields['userid'] = userId;
            event.fields['originaluser'] = userId;
            event.fields['#originaluser_label'] = event.fields['asset_.aucisowner_.person_.hlagdisplayname'];
            if (userId && !event.scope.associationOptions['person_']) {
                //https://controltechnologysolutions.atlassian.net/browse/HAP-864
                event.scope.associationOptions['person_'] = [];
                var item = {
                    value: userId,
                    label: event.fields['asset_.aucisowner_.person_.hlagdisplayname']
                }
                event.scope.associationOptions['person_'].push(item);
            }

            event.fields['assetlocation'] = assetLocation;
            var schemaId = event.scope.schema.schemaId;
            if (schemaId.startsWith('update') || schemaId.startsWith("move")) {
                if (assetUsage &&
                    assetUsage.equalsAny("EDUCATION", "POOL", "MEETING", "MOB MARITME", "MOBILE HOME", "NON-STANDARD", "SPARE UNIT", "SPECIAL FUNC", "STANDARD", "STAND ALONE", "TEST", "TRAINEE")) {
                    //HAP-1025
                    event.fields['usage'] = assetUsage.toUpperCase();
                } else {
                    event.fields['usage'] = "STANDARD";
                }
            } else {
                event.fields['assetusage'] = assetUsage;
            }

            if (schemaId.startsWith('installstd')) {
                event.fields['macaddressOriginal'] = event.fields['asset_.assetspecmacaddress_.alnvalue'];
                event.fields['macaddress'] = event.fields['asset_.assetspecmacaddress_.alnvalue'];
            }

            var availablecostcenters = event.scope.associationOptions.costCentersByPrimaryUser || event.scope.associationOptions.costCentersByToItc;
            event.fields['costcenteroriginal'] = costCenter;
            if (schemaId.startsWith('install') || schemaId.startsWith('move') || schemaId.startsWith("update")) {
                //if there´s an association, then, we set the value, and the label would be picked from the associationOptions list
                //if the asset had a costcenter that is not one of the user´s costcentes, we need to remove it
                if (checkCostCenterAvailability(availablecostcenters, costCenter, 'value')) {
                    event.fields['costcenter'] = costCenter;
                } else {
                    event.fields['costcenter'] = null;
                }
            } else {
                //if replace then we have a readonly costcenter instead of an optionfield, so we need the label as the "value"
                event.fields['costcenter'] = costCenterLabel;
            }
            event.fields['currentitc'] = currentITC;
            parseLocations(event.fields, event.fields['asset_.location'], event.triggerparams);
            setOriginalData(event.scope.schema, event.fields, event.scope.associationOptions);
        },



        goToImacGrid: function () {
            if (GetPopUpMode().equalsAny("browser", "nomenu")) {
                //if creating imac from asset, we will get into this workflow
                window.close();
                return;
            }
            //Redirect to IMAC Grid                
            redirectService.goToApplicationView("imac", "list", "input", i18NService.get18nValue("IMAC Grid", "IMAC Grid", null), null);
        },

        opendetail: function (datamap, displayables) {

            var id = datamap['ticketid'];
            var parameters = { id: id, popupmode: 'browser' };
            redirectService.goToApplicationView('imac', 'detail', 'output', null, parameters);
        },

        toitcchanged: function (event) {
            //            event.fields['userid'] = event.fields['asset_.primaryuser_.person_.hlagdisplayname'];
            //TODO: fix the bug where the autocompleteserver is not being correctly bound if it´s included dinamically (via show expressions)
            $("input[data-association-key=person_]").typeahead('val', event.fields['asset_.aucisowner_.person_.hlagdisplayname']);
            var costCenter = event.fields['asset_.assetglaccount_.glaccount'];
            event.fields['costcenter'] = costCenter;
            setOriginalDataFromAttribute(event.scope.schema, event.fields, event.scope.associationOptions, 'costcenter');
        },

        validatedecommission: function (schema, datamap) {
            var result = [];
            if (!datamap["#accepted"]) {
                result.push("Confirm that all data for decommission process has been provided")
            }
            return result;
        },

        setcommodities: function (event) {
            var datamap = event.datamap;
            var selectedCommodities = datamap['assetCommodities'];
            var associationOptions = event.associationOptions;
            var commodities = associationOptions["assetCommodities"];
            datamap["#assetCommodities_label"] = [];
            for (var i = 0; i < commodities.length; i++) {
                var commodity = commodities[i];
                var selected = false;
                for (var j = 0; j < selectedCommodities.length; j++) {
                    if (selectedCommodities[j] == commodity.value) {
                        selected = true;
                        break;
                    }
                }
                datamap["#assetCommodities_label"].push(commodity.label + " : " + (selected ? "Y" : "N"));
            }
        }
    };
});