var app = angular.module('sw_layout');

app.factory('imacservice', function ($http, alertService, fieldService, redirectService, i18NService) {
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
            return;
        }
        var idxBldg = location.indexOf('/BLDG');
        if (idxBldg != -1) {
            //point straight to building only
            datamap['building'] = location.substring(0, idxBldg) + '$ignorewatch';
            datamap['floor'] = datamap['room'] = "$null$ignorewatch";
        } else {
            var parts = location.split("/");
            if (parts.length != 3) {
                //this should never at hapag´s... but let´s play safe
                datamap['building'] = datamap['floor'] = datamap['room'] = "$null$ignorewatch";
                return;
            }
            var shouldIgnoreWatch = triggerparams.dispatchedbytheuser && triggerparams.phase != 'initial';
            datamap['building'] = parts[0] + (shouldIgnoreWatch ? '$ignorewatch' : '');
            datamap['floor'] = parts[1].substring(parts[1].indexOf(':') + 1) + (shouldIgnoreWatch ? '$ignorewatch' : '');
            datamap['room'] = location;
        }
    };

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

    return {

        beforeChangeAsset: function (event) {
            var datamap =event.fields;
            datamap['building'] = "$null$ignorewatch";
            datamap['floor'] = "$null$ignorewatch";
            datamap['room'] = "$null$ignorewatch";
            datamap['costcenter'] = null;
        },

        afterChangeAsset: function (event) {
            var userId = event.fields['asset_.aucisowner_.person_.personid'];
            var costCenter = event.fields['asset_.assetglaccount_.glaccount'];
            var costCenterLabel = event.fields['asset_.assetglaccount_.displaycostcenter'];
            var currentITC = event.fields['asset_.primaryuser_.hlagdisplayname'];
            var assetLocation = event.fields['asset_.location_.description'];
            var assetUsage = event.fields['asset_.usage'];
            event.fields['userid'] = userId;
            event.fields['assetlocation'] = assetLocation;
            event.fields['assetusage'] = assetUsage;
            var schemaId = event.scope.schema.schemaId;
            var availablecostcenters = event.scope.associationOptions.costCentersByPrimaryUser;
            if (schemaId.startsWith('install') || schemaId.startsWith('move')) {
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
            $("input[data-association-key=person_]").typeahead('val', event.fields['asset_.primaryuser_.person_.hlagdisplayname']);
            var costCenter = event.fields['asset_.assetglaccount_.glaccount'];
            event.fields['costcenter'] = costCenter;
        },

        validatedecommission: function (schema, datamap) {
            var result = [];
            if (!datamap["#accepted"]) {
                result.push("Confirm that all data for decommission process has been provided")
            }
            return result;
        }
    };
});