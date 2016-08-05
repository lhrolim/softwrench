(function(softwrench) {
    "use strict";

    softwrench.controller('CompositionMenuController', ["$log", "$scope", "$ionicScrollDelegate", "crudContextService", function ($log, $scope, $ionicScrollDelegate, crudContextService) {

        $scope.compositionMenus = function () {
            return crudContextService.currentCompositionsToShow();
        }

        $scope.getTabIcon = function (tab) {
            return tab.schema.schemas.list.properties['icon.composition.tab'];
        };

        $scope.loadTab = function (tab) {
            crudContextService.loadTab(tab).then(() => {
                $scope.$emit("sw_compositionselected");
            });
            $ionicScrollDelegate.scrollTop();
        }

        $scope.notOnMainTab = function () {
            return !crudContextService.isOnMainTab();
        }

    }]);

})(softwrench);

