(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('imacservice', ["$http", "alertService", "fieldService", "redirectService", "i18NService", function ($http, alertService, fieldService, redirectService, i18NService) {
    /*
    * The location could point either to a building only, or to the entire building+floor+room, but never straight 
    * to the floor alone as it does not exist as an entry on hapag database.
    *
    *EX: DE-34F-0017/BLDG --> Building
    *DE-34F-0017/FL:0008/RO:0002 --> Room
    */
    var parseLocations = function (datamap, location, triggerparams) {
        if (location == null) {
            datamap['building'] = datamap['floor'] = datamap['room']= null;
            return;
        }
        var idxBldg = location.indexOf('/BLDG');
        if (idxBldg != -1) {
            //point straight to building only
            datamap['building'] = location.substring(0, idxBldg) + '$ignorewatch';
            datamap['floor'] = datamap['room'] = null;
        } else {
            var parts = location.split("/");
            if (parts.length != 3) {
                //this should never at hapag´s... but let´s play safe
                datamap['building'] = datamap['floor'] = datamap['room'] = null;
                return;
            }
            var shouldIgnoreWatch = triggerparams.dispatchedbytheuser && triggerparams.phase != 'initial';
            datamap['building'] = parts[0] + (shouldIgnoreWatch ? '$ignorewatch' : '');
            datamap['floor'] = parts[1].substring(parts[1].indexOf(':') + 1) + (shouldIgnoreWatch ? '$ignorewatch' : '');
            datamap['room'] = location;
        }
    };
    return {

        afterChangeAsset: function (event) {
            var userId = event['asset_.aucisowner_.person_.personid'];
            var costCenter = event['asset_.assetglaccount_.glaccount'];
            event['userid'] = userId;
            event['costcenter'] = costCenter;
            parseLocations(event, event['asset_.location'], event.triggerparams);
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

    };

}]);

})(angular);