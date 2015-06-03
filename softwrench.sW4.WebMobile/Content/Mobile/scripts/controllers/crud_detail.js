softwrench.controller('CrudDetailController', function ($log, $scope, $rootScope, schemaService, crudContextService, fieldService, offlineAssociationService, $ionicPopover) {

    function init() {
        $scope.displayables = crudContextService.mainDisplayables();
        $scope.schema= crudContextService.currentDetailSchema();
        $scope.datamap = crudContextService.currentDetailItem();
    }

    $ionicPopover.fromTemplateUrl('Content/Mobile/templates/compositionmenu.html', {
        scope: $scope,
    }).then(function (popover) {
        $scope.compositionpopover = popover;
    });


    $scope.expandCompositions = function ($event) {
        $scope.compositionpopover.show($event);
    }

    $scope.$on("sw_compositionselected", function () {
        $scope.compositionpopover.hide();
    });

    $scope.title = function () {
        return crudContextService.currentTitle();
    }

    $scope.loadMainTab = function() {
        crudContextService.loadTab(null);
    }

    $scope.tabTitle = function() {
        return crudContextService.tabTitle();
    }

    $scope.isOnMainTab = function () {
        return crudContextService.isOnMainTab();
    }


    $scope.detailSummary = function () {
        return schemaService.getTitle(crudContextService.currentDetailSchema(), $scope.datamap, true);
    }

    $scope.navigateNext = function () {
        crudContextService.navigateNext().then(function () {
            $scope.datamap = crudContextService.currentDetailItem();
        });
    }

    $scope.navigatePrevious = function () {
        crudContextService.navigatePrevious();
        $scope.datamap = crudContextService.currentDetailItem();
    }
    $rootScope.$on('$stateChangeSuccess',
          function (event, toState, toParams, fromState, fromParams) {
              if (toState.name.startsWith("main.cruddetail")) {
                  //needs to refresh the displayables and datamap everytime the detail page is loaded.
                  init();
              } else {
                  crudContextService.leavingDetail();
              }
          });

    init();

}
);