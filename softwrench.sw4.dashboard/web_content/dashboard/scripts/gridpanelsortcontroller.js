
(function (angular) {
    "use strict";

    angular.module("sw_layout").controller("gridpanelsortController", ["$scope", "searchService", "crudContextHolderService", function ($scope, searchService, crudContextHolderService) {


        function init() {
            const datamap = crudContextHolderService.rootDataMap("#modal");
            $scope.datamap = datamap;

            const sortingField = datamap.defaultSortField;
            datamap["multiSort"] = searchService.parseMultiSort(sortingField);

            var appFields = [];

            if (datamap.appFields) {
                if (datamap.appFields instanceof Array) {
                    appFields = datamap.appFields.map(i => {
                        return { attribute: i };
                    });
                }
                else if (typeof datamap.appFields === "object") {
                    appFields = datamap.appFields;
                } else {
                    appFields = datamap.appFields.split(",").map(i => {
                        return { attribute: i };
                    });
                }
            }

            $scope.vm = {
                appFields
            };
        }

        $scope.$on(JavascriptEventConstants.ModalShown, function (event, modalData) {
            if (modalData.schema.applicationName === "_dashboardgrid" && modalData.schema.schemaId === "detail") {
                init();
            }
        });

        $scope.$on(DashboardEventConstants.AppFieldsLoaded, function (event) {
            init();
        });


        init();



    }]);

})(angular);