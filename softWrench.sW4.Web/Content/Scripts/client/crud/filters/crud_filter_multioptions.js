(function (angular) {
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
                    scope.suggestedoptions = [];
                    scope.vm.recentlyOptions = [];


                    if (filter.allowBlank) {
                        var item = {
                            label: "No " + filter.label,
                            //SearchUtils.cs#NullOrPrefix
                            value: "nullor:",
                            nonstoreable: true
                        }
                        scope.suggestedoptions.push(item);
                    }

                    //initing any metadata declared option first
                    scope.filter.options.forEach(function (item) {
                        //these won´t go to currently used
                        item.nonstoreable = true;
                        scope.suggestedoptions.push(item);
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
                        scope.vm.notSearching = true;

                        //for lazy filters we will start the recently used from local storage and init the autocompleteservers
                        scope.vm.recentlyOptions = scope.vm.recentlyOptions.concat(filterModelService.lookupRecentlyUsed(schema.applicationName, schema.schemaId, filter.attribute));
                        $timeout(function () {
                            cmpAutocompleteServer.init(element, null, scope.schema, scope);

                            $('input.typeahead', element).each(function (index, element) {
                                var jelement = $(element);
                                //caching element to later access it on controlle fns
                                scope.jelement = jelement;
                                jelement.on("keydown", function (e) {
                                    if (e.keyCode === 13) {
                                        //enter and magnify click have the same effect
                                        scope.executeAsFreeText();
                                    }
                                });

                                jelement.on("keyup", function (e) {
                                    //if filter is applied, let´s not show recently used filters
                                    var newShowRecently = $(e.target).val() === "";
                                    //let´s try to avoid useless digest calls
                                    var shouldDigest = newShowRecently !== scope.vm.showRecently;
                                    if (newShowRecently) {
                                        scope.filteroptions = [];
                                        shouldDigest = true;
                                    }
                                    scope.vm.notSearching = newShowRecently;
                                    if (shouldDigest) {
                                        //if to avoid calling digest inadvertdly
                                        scope.$digest();
                                    }
                                });
                            });


                            scope.$on("sw.autocompleteserver.response", function (event, response) {
                                if (scope.vm.notSearching) {
                                    //seems like the autocomplete is returning the list when it reaches 0 sometimes
                                    return;
                                }
                                for (var i = response.length - 1; i >= 0; i--) {
                                    var item = response[i];
                                    item.removable = true;
                                }
                                if (response.length === 21) {
                                    //TODO: return rather an object containing the count
                                    scope.vm.hasMoreItems = true;
                                    response = response.slice(0, 20);
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
                        if (filter.displayCode === false || option.value === "nullor:") {
                            return option.label;
                        }
                        return "(" + option.value + ")" + " - " + option.label;
                    }


                    $scope.executeAsFreeText = function () {
                        var searchData = $scope.searchData;
                        var searchOperator = $scope.searchOperator;
                        var val = $scope.jelement.val();
                        if (val === "") {
                            $('.dropdown.open').removeClass('open');
                            //not calling the filter if nothing was selected
                            return;
                        }
                        searchData[filter.attribute] = $scope.jelement.val();
                        searchOperator[filter.attribute] = searchService.getSearchOperationById("CONTAINS");
                        $scope.applyFilter({ keepitOpen: false });
                    }

                    //clear button of the filter clicked
                    $scope.$on("sw.filter.clear", function (event, filterAttribute) {
                        if (filterAttribute === $scope.filter.attribute) {
                            $scope.vm.allSelected = 0;
                            $scope.toggleSelectAll();
                        }
                    });

                    $scope.getAllAvailableOptions = function() {
                        return $scope.filteroptions.concat($scope.suggestedoptions).concat($scope.vm.recentlyOptions);
                    }

                    $scope.modifySearchData = function () {
                        var searchData = $scope.searchData;
                        var searchOperator = $scope.searchOperator;
                        searchData[filter.attribute] = null;
                        searchOperator[filter.attribute] = null;

                        var selectedItems = filterModelService.buildSelectedItemsArray($scope.getAllAvailableOptions(), $scope.selectedOptions);
                        var result = filterModelService.buildSearchValueFromOptions(selectedItems);
                        $scope.vm.recentlyOptions = filterModelService.updateRecentlyUsed($scope.schema, $scope.filter.attribute, selectedItems);
                        if (result) {
                            searchData[filter.attribute] = result;
                            searchOperator[filter.attribute] = searchService.getSearchOperationById("EQ");
                        }
                        $scope.applyFilter({ keepitOpen: true });
                    }

                    $scope.toggleSelectAll = function () {
                        var value = $scope.vm.allSelected ? 1 : 0;
                        var availableOptions = $scope.getAllAvailableOptions();
                        for (var i = 0; i < availableOptions.length; i++) {
                            var el = availableOptions[i];
                            $scope.selectedOptions[el.value] = value;
                        }
                        $scope.modifySearchData();

                    }

                    $scope.removeItem = function (item) {
                        var arr = $scope.vm.recentlyOptions;
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

