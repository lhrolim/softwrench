(function (angular) {
    "use strict";

    angular.module("sw_layout")
        .directive("crudFilterBar", ["contextService", function (contextService) {
            var directive = {
                restrict: "E",
                templateUrl: contextService.getResourceUrl("/Content/Templates/crud/filter/crud_filter_bar.html"),
                scope: {
                    schema: "=", // model's current schema
                    datamap: "=", // model's current datamap
                    searchData: "=", // shared dictionary [column : current filter search value]
                    searchOperator: "=", // shared dictionary [column : current filter operator object]
                    selectAll: "=", // shared boolean flag indicating if multiple select in filters is selected
                    advancedFilterMode: "=", // shared boolean flag indicating if advanced filter mode is activated
                    filterApplied: "&" // callback executed when the filters are applied
                }
            };
            return directive;
        }
    ]);

})(angular);