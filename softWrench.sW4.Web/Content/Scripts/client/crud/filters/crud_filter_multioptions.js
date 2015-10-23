(function (angular) {
    "use strict";

    angular.module("sw_layout")
        .directive("filterMultipleOption", ["contextService", "restService", "filterModelService", function (contextService, restService, filterModelService) {

            var directive = {
                restrict: "E",
                templateUrl: contextService.getResourceUrl("/Content/Templates/crud/filter/crud_filter_multipleoptions.html"),
                scope: {
                    filter: '=',
                    searchData: '=',
                    schema: '=',
                    applyFilter: "&"
                },

                link: function (scope, element, attrs) {
                    scope.vm = {};

                    //let´s avoid some null pointers
                    scope.filter.options = scope.filter.options || [];

                    //this is also static
                    var schema = scope.schema;
                    var filter = scope.filter;


                    scope.selectedOptions = [];
                    scope.filteroptions = [];
                    scope.filter.options.forEach(function (item) {
                        scope.filteroptions.push(item);
                    });

                    if (!scope.filter.lazy) {
                        //let´s get the whole list from the server 
                        //GetFilterOptions(string application, ApplicationMetadataSchemaKey key, string filterProvider, string filterAttribute, string labelSearchString)
                        var parameters = {
                            application: schema.applicationName,
                            key: {
                                schemaId: schema.schemaId,
                                platform: "web"
                            },
                            labelSearchString: '',
                            filterProvider: filter.provider,
                            filterAttribute: filter.attribute
                        }

                        restService.getPromise("FilterData", "GetFilterOptions", parameters).then(function (result) {
                            scope.filteroptions = scope.filteroptions.concat(result.data);
                        });
                    } else {
                        scope.filteroptions = scope.filteroptions.concat(filterModelService.lookupRecentlyUsed(schema.applicationName, schema.schemaId, filter.attribute));
                    }
                },

                controller: ["$scope", function ($scope) {


                    var filter = $scope.filter;


                    $scope.$on("sw.filter.clear", function (event, filterAttribute) {
                        if (filterAttribute === $scope.filter.attribute) {
                            $scope.vm.allSelected = 0;
                            $scope.toggleSelectAll();
                        }
                    });

                    $scope.modifySearchData = function () {
                        var searchData = $scope.searchData;
                        searchData[filter.attribute] = null;
                        var result = filterModelService.buildSearchValueFromOptions($scope.selectedOptions);
                        if (result) {
                            searchData[filter.attribute] = result;
                        }
                        $scope.applyFilter();
                    }

                    $scope.toggleSelectAll = function () {
                        var value = $scope.vm.allSelected ? 1 : 0;
                        for (var i = 0; i < $scope.filteroptions.length; i++) {
                            var el = $scope.filteroptions[i];
                            $scope.selectedOptions[el.value] = value;
                        }
                        $scope.modifySearchData();

                    }



                }]



            };

            return directive;

        }]);

})(angular);

