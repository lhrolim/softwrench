(function (angular) {
    "use strict";

    angular.module("softwrench").controller("FsLocationDrillDownController", ["$rootScope", "$scope", "$timeout", "$ionicHistory", "drillDownService", "crudContextService", "swdbDAO", function ($rootScope, $scope, $timeout, $ionicHistory, drillDownService, crudContextService, swdbDAO) {
        $scope.buildLocationTitle = function (location) {
            if (!location) {
                return "";
            }
            const datamap = location.datamap;
            return datamap.description ? `${datamap.location} - ${datamap.description}` : datamap.location;
        }

        $scope.buildAssetTitle = function (asset) {
            if (!asset) {
                return "";
            }
            const datamap = asset.datamap;
            return datamap.description ? `${datamap.assetnum} - ${datamap.description}` : datamap.assetnum;
        }

        $scope.locationClick = function (location) {
            drillDownService.locationDrillDownClick(location);
        }

        $scope.locationTitle = function () {
            return $scope.buildLocationTitle($scope.drillDown.selectedLocation);
        }

        $scope.locationSubTitle = function () {
            const dd = $scope.drillDown;
            const locationsTitle = `${dd.locationsCount} ${dd.locationsCount === 1 ? "sublocation" : "sublocations"}`;
            const assetsTitle = `${dd.assetsCount} ${dd.assetsCount === 1 ? "asset" : "assets"}`;
            return `${locationsTitle} - ${assetsTitle}`;
        }

        $scope.noChildLocations = function () {
            return ($scope.drillDown.locations ? $scope.drillDown.locations.length : 0) === 0;
        }

        $scope.assetsSubTitle = function () {
            const dd = $scope.drillDown;
            return `${dd.assetsCount} ${dd.assetsCount === 1 ? "asset" : "assets"}`;
        }

        $scope.noAssetsTitle = function () {
            const dd = $scope.drillDown;
            const location = dd.selectedLocation;
            const locationTerm = location ? ` on location ${$scope.buildLocationTitle(location)}` : "";
            const childTerm = dd.childAssetsView ? " children's" : "";
            return `No${childTerm} assets were found${locationTerm}.`;
        }

        $scope.noAssets = function () {
            return ($scope.drillDown.assets ? $scope.drillDown.assets.length : 0) === 0;
        }

        $scope.assetClick = function (asset) {
            const datamap = crudContextService.currentDetailItemDataMap();
            datamap.assetnum = asset.datamap.assetnum;
            datamap.location = asset.datamap.location;
            const failurecode = asset.datamap.failurecode;
            datamap["failurecode"] = failurecode;
            if (!!failurecode) {
                return swdbDAO.findSingleByQuery("AssociationData", `textindex01='${failurecode}' and application = 'failurelistonly'`).then(result => {
                    datamap["failurelistonly_.failurelist"] = result.datamap.failurelist;
                    $timeout(() => {
                        $ionicHistory.goBack();
                        $timeout(() => {
                            //this inner timeout is needed because we need the ionautocompletes, which have the $on listeners, to be back on the screen in order to handle the event
                            $rootScope.$broadcast("sw:association:resolved", {
                                failurelistonly_: {
                                    associationKey: "failurelistonly_",
                                    item: result
                                }
                            });        
                        },200,false);
                    }, 0, false);
                    
                });
            }

            //            datamap.failurecode = asset.datamap.failurecode;
            $timeout(() => $ionicHistory.goBack(), 0, false);
        }

        $scope.loadMore = function () {
            drillDownService.loadMore();
        }

        drillDownService.drillDownClear();
        $scope.drillDown = drillDownService.getDrillDown();

        $scope.$watch("drillDown.locationQuery", function () {
            if (!$scope.drillDown.assetView) {
                drillDownService.updateDrillDownLocations();
            }
        });

        $scope.$watch("drillDown.assetQuery", function () {
            if ($scope.drillDown.assetView) {
                drillDownService.updateDrillDownAssets();
            }
        });
    }]);

    angular.module("softwrench").directive("locationDrillDown", [function () {
        const directive = {
            restrict: "E",
            template: "<div ng-if='!context.currentDetailItem.datamap.assetnum'><button class='button button-calm button-block search-clear-button' ng-click='openDrillDown()' style='margin-bottom: 0px;'>Drill Down</button></div>" +
            "<textarea autosize-textarea wrap='hard' ng-model='description' ng-if='context.currentDetailItem.datamap.assetnum' ng-click='openDrillDown()' ng-readonly='true' style='cursor: pointer; color: black;'/>",
            transclude: false,
            replace: false,
            scope: {
            },
            controller: ["$rootScope", "$scope", "routeService", "drillDownService", "crudContextService", function ($rootScope, $scope, routeService, drillDownService, crudContextService) {
                $scope.openDrillDown = function () {
                    routeService.go("main.cruddetail.locationdrilldown");
                }

                $scope.context = crudContextService.getCrudContext();

                $scope.$watch("context.currentDetailItem.datamap.assetnum", function (assetNum) {
                    $scope.description = "";
                    if (!assetNum) {
                        return;
                    }
                    drillDownService.findAsset(assetNum).then((asset) => {
                        if (!asset) {
                            return;
                        }
                        const datamap = asset.datamap;
                        $scope.description = datamap.description ? datamap.assetnum + " - " + datamap.description : datamap.assetnum;
                    });
                });
            }]
        };

        return directive;
    }]);
})(angular);