(function (angular) {
    "use strict";

angular.module('sw_layout')
    .service('srsummaryService', ["redirectService", function (redirectService) {

    return {

        redirectToHapagHome: function () {
            if (GetPopUpMode().equalsAny("browser", "nomenu")) {
                //if creating sr from asset, we will get into this workflow
                window.close();
                return;
            }
            redirectService.redirectToAction(null, 'HapagHome', null, null);
        }

    };

}]);

})(angular);