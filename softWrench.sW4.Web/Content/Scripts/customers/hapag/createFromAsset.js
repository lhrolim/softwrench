(function (angular) {
    "use strict";

angular.module("sw_layout").controller("CreateFromAssetController", CreateFromAssetController);
function CreateFromAssetController($scope, $http, i18NService, fieldService, redirectService) {
    "ngInject";

    var OPERATING = "OPERATING";
    var ACTIVE = "120 Active";
    var IDLE = "150 Idle";
    var ORDERED = "010 Ordered";

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.getTicketOptions = function (datamap) {
        //TODO: review logic of when to select what
        var status = datamap.status;
        var options = [{ label: '--Select One--', value: 'Select One' }];
        if (status.equalsAny(ACTIVE, OPERATING)) {
            options.push({ label: 'Service Request', value: 'servicerequest' });
        }
        options.push({ label: 'Imac Service', value: 'imac' });
        return options;
    }

    $scope.getTypeOfImac = function (datamap) {
        //this will be filled server side at HapagAssetDataSet and HapagImacDataSet
        return datamap['#availableimacoptions'];
    }


    $scope.createFromAsset = function () {
        var datamap = $scope.datamap;
        var typeofticket = $scope.typeofticket;
        var typeofimac = $scope.typeofimac;
        var schemaid = typeofticket == 'servicerequest' ? 'general' : 'add';
        var initialData = {};
        initialData['typeofimac'] = typeofimac;
        initialData['assetnum'] = datamap['assetnum'];
        initialData['classstructureid'] = datamap['classstructureid'];
        initialData['pluspcustomer'] = datamap['pluspcustomer'];
        //if the user is not custudian of the asset it need to be an itc asset
        initialData['#iscustodian'] = datamap['#iscustodian'];
        //the real schema will be determined on serverside
        var parameters = {
            //this means that we want to keep the same window (!=browser), but with no menu
            popupmode: 'nomenu'
        };
        var title = schemaid == 'general' ? capitaliseFirstLetter('new {0} service request') : capitaliseFirstLetter('new ' + typeofimac + ' imac service');
        redirectService.goToApplicationView(typeofticket, schemaid, 'input', title, parameters, initialData);
    };
}

window.AboutController = CreateFromAssetController;

})(angular);