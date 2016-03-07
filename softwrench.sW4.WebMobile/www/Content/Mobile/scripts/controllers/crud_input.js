(function (angular) {
    "use strict";

    angular.module("softwrench").controller("CrudInputController", ['$log', '$scope', 'crudContextService', 'fieldService', 'offlineAssociationService', '$ionicPopover', 'expressionService', function ($log, $scope, crudContextService, fieldService, offlineAssociationService, $ionicPopover, expressionService) {

        $ionicPopover.fromTemplateUrl('Content/Mobile/templates/compositionmenu.html', {
            scope: $scope,
        }).then(function (popover) {
            $scope.compositionpopover = popover;
        });

    }]);
    
})(angular);


