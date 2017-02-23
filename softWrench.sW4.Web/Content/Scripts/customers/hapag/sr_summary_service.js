var app = angular.module('sw_layout');

app.factory('srsummaryService', function (redirectService) {

    "ngInject";

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

});