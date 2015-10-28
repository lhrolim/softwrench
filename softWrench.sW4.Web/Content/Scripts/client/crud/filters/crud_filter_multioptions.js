﻿(function (angular) {
    "use strict";

    angular.module("sw_layout")
        .directive("filterMultipleOption", ["contextService", "restService", "filterModelService", "cmpAutocompleteServer", "$timeout", "searchService", function (contextService, restService, filterModelService,
            cmpAutocompleteServer, $timeout, searchService) {

            var directive = {
                restrict: "E",
                templateUrl: contextService.getResourceUrl("/Content/Templates/crud/filter/crud_filter_multipleoptions.html"),
                scope: {
                    filter: '=',
                    searchData: '=',
                    searchOperator: '=',
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
                    scope.vm.recentlyOptions = [];

                    //initing any metadata declared option first
                    scope.filter.options.forEach(function (item) {
                        if (filter.lazy) {
                            scope.vm.recentlyOptions.push(item);
                        } else {
                            scope.filteroptions.push(item);
                        }
                    });

                    if (!scope.filter.lazy) {
                        //let´s get the whole list from the server 
                        //FilterData#GetFilterOptions(string application, ApplicationMetadataSchemaKey key, string filterProvider, string filterAttribute, string labelSearchString)
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
                        scope.vm.showRecently = true;

                        //for lazy filters we will start the recently used from local storage and init the autocompleteservers
                        scope.vm.recentlyOptions = scope.vm.recentlyOptions.concat(filterModelService.lookupRecentlyUsed(schema.applicationName, schema.schemaId, filter.attribute));
                        $timeout(function () {
                            cmpAutocompleteServer.init(element, null, scope.schema, scope);

                            $('input.typeahead', element).each(function (index, element) {
                                var jelement = $(element);
                                jelement.on("keyup", function (e) {
                                    //if filter is applied, let´s not show recently used filters
                                    var newShowRecently = $(e.target).val() === "";
                                    //let´s try to avoid useless digest calls
                                    var shouldDigest = newShowRecently !== scope.vm.showRecently;
                                    if (newShowRecently) {
                                        scope.filteroptions = [];
                                        shouldDigest = true;
                                    }
                                    scope.vm.showRecently = newShowRecently;
                                    if (shouldDigest) {
                                        //if to avoid calling digest inadvertdly
                                        scope.$digest();
                                    }
                                });
                            });


                            scope.$on("sw.autocompleteserver.response", function (event, response) {
                                if (scope.vm.showRecently) {
                                    //seems like the autocomplete is returning the list when it reaches 0 sometimes
                                    return;
                                }
                                for (var i = response.length - 1; i >= 0; i--) {
                                    var item = response[i];
                                    item.removable = true;
                                }

                                //updating the list of options
                                scope.filteroptions = response;
                                scope.$digest();
                            });

                        }, 0, false);

                    }
                },

                controller: ["$scope", function ($scope) {


                    var filter = $scope.filter;

                    $scope.labelValue = function (option) {
                        if (filter.displayCode === false) {
                            return option.label;
                        }
                        return "(" + option.value + ")" + " - " + option.label;
                    }


                    //clear button of the filter clicked
                    $scope.$on("sw.filter.clear", function (event, filterAttribute) {
                        if (filterAttribute === $scope.filter.attribute) {
                            $scope.vm.allSelected = 0;
                            $scope.toggleSelectAll();
                        }
                    });

                    $scope.modifySearchData = function () {
                        var searchData = $scope.searchData;
                        var searchOperator = $scope.searchOperator;
                        searchData[filter.attribute] = null;
                        searchOperator[filter.attribute] = null;

                        var selectedItems = filterModelService.buildSelectedItemsArray($scope.filteroptions, $scope.selectedOptions);
                        var result = filterModelService.buildSearchValueFromOptions(selectedItems);
                        $scope.vm.recentlyOptions = filterModelService.updateRecentlyUsed($scope.schema, $scope.filter.attribute, selectedItems);
                        if (result) {
                            searchData[filter.attribute] = result;
                            searchOperator[filter.attribute] = searchService.getSearchOperationById("EQ");
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

                    $scope.removeItem = function (item) {
                        var arr = $scope.filteroptions;
                        var idx = -1;
                        for (var i = 0; i < arr.length; i++) {
                            if (arr[i].value === item.value) {
                                idx = i;
                                break;
                            }
                        }
                        if (idx !== -1) {
                            arr.splice(idx, 1);
                            filterModelService.deleteFromRecentlyUsed($scope.schema, $scope.filter.attribute, item);
                        }

                    }

                    //return call from the autocomplete server invocation
                    $scope.$on("sw_autocompleteselected", function (event, jqueryEvent, item, filterAttribute) {
                        if (filterAttribute !== $scope.filter.attribute) {
                            //not the filter interested in listening the event (i.e this should be handled by another filter but this)
                            return;
                        }
                        //cleaning up jquery element
                        $(jqueryEvent.target).val("");
                        if ($scope.filteroptions.some(function (el) {
                                return el.value === item.value;
                        })) {
                            //to avoid duplications
                            return;
                        }
                        //this items can be later removed by the user, in opposition from the ones coming from metadata eventually
                        item.removable = true;
                        $scope.filteroptions.push(item);
                        filterModelService.updateRecentlyUsed($scope.schema, filterAttribute, item);
                        $scope.$digest();
                    });


                }]



            };

            return directive;

        }]);

})(angular);

