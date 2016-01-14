(function (angular) {
    "use strict";

    angular.module("sw_layout")
        .directive("filterMultipleOption", ["contextService", "restService", "filterModelService", "cmpAutocompleteServer", "$timeout", "searchService", "schemaService", "modalFilterService", "modalService", "crudContextHolderService", 
            function (contextService, restService, filterModelService,
            cmpAutocompleteServer, $timeout, searchService, schemaService, modalFilterService, modalService, crudContextHolderService) {

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
                    scope.lookupModalBuffer = {};


                    if (filter.allowBlank) {
                        var item = {
                            label: "No " + filter.label,
                            //SearchUtils.cs#NullOrPrefix
                            value: "nullor:",
                            nonstoreable: true
                        }
                        if (scope.filter.lazy) {
                            scope.suggestedoptions.push(item);
                        } else {
                            scope.filteroptions.push(item);
                        }
                        
                    }

                    //initing any metadata declared option first
                    scope.filter.options.forEach(function (item) {
                        //these won´t go to currently used
                        item.nonstoreable = true;
                        scope.suggestedoptions.push(item);
                    });

                  


                    if (!scope.filter.lazy && !!filter.provider) {
                        //let´s get the whole list from the server 
                        //FilterData#GetFilterOptions(string application, ApplicationMetadataSchemaKey key, string filterProvider, string filterAttribute, string labelSearchString)
                        var parameters = {
                            application: schema.applicationName,
                            key: schemaService.buildApplicationMetadataSchemaKey(schema),
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

                controller: ["$scope", "$rootScope", function ($scope, $rootScope) {


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
                            $scope.lookupModalBuffer = {};
                            $timeout(function() {
                                $scope.jelement.typeahead('val', '');
                            },0,false)
                            
                        }
                    });

                    $scope.getAllAvailableOptions = function() {
                        return $scope.filteroptions.concat($scope.suggestedoptions).concat($scope.vm.recentlyOptions);
                    }

                    // updates the lookup modal grid buffer
                    // if a option is unselected or selected by user the lookup modal grid buffer has
                    // to be updated to reflect the options selection state
                    function updateLookupModalGridBuffer(changedOption, selectedItems) {
                        var value = changedOption.value;

                        // if the option was selected or unselected
                        var selected = selectedItems.some(function (item) {
                            return value === item.value;
                        });

                        // add or removes an indication of option state on lookup modal grid buffer

                        // if unselected removes entry from lookup modal grid buffer
                        if (!selected) {
                            delete $scope.lookupModalBuffer[value];
                            return;
                        }

                        // if selected add an entry to lookup modal grid buffer
                        // when lookup modal is opened the lookup modal grid buffer is passed
                        // and any selected option will be selected on modal too
                        var promise = modalFilterService.getModalFilterSchema($scope.filter, $scope.schema);
                        promise.then(function (modalSchema) {
                            var attFieldName = $scope.filter.advancedFilterAttribute || modalSchema.idFieldName;
                            var datamap = { fields: {} };
                            datamap.fields[attFieldName] = value;
                            $scope.lookupModalBuffer[value] = datamap;
                        });
                    }

                    // changed option is sent in case of user action on changing the state of option checkbox
                    $scope.modifySearchData = function (changedOption) {
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

                        // if has lookup for options
                        if (changedOption && $scope.filter.advancedFilterSchemaId) {
                            updateLookupModalGridBuffer(changedOption, selectedItems);
                        }
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

                    $scope.lookup = function () {
                        // sets the modal grid buffer from the local buffer
                        var selectionModel = crudContextHolderService.getSelectionModel(modalService.panelid);
                        selectionModel.selectionBuffer = $scope.lookupModalBuffer;
                        $rootScope.$broadcast("sw.crud.list.filter.modal.show", filter);
                        // sets the id column to use on buffer in case the filter value is not the id column
                        if ($scope.filter.advancedFilterAttribute) {
                            selectionModel.selectionBufferIdCollumn = $scope.filter.advancedFilterAttribute;
                        }
                    }

                    //return call from the autocomplete server invocation
                    $scope.$on("sw_autocompleteselected", function (event, jqueryEvent, item, filterAttribute) {
                        if (filterAttribute !== $scope.filter.attribute) {
                            //not the filter interested in listening the event (i.e this should be handled by another filter but this)
                            return;
                        }
                        //cleaning up jquery element
                        $scope.jelement.typeahead('val', '');
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

                    function isOptionFilter() {
                        return !filter || filter.type !== "MetadataOptionFilter";
                    }

                    /**
                    * Add one filter option (if there is not one with same value)
                    * and makes it selected.
                    * 
                    * @param {} value The option value.
                    */
                    function addLookupOption(value) {
                        // searchs for itens with same value on filter options
                        var alreadyExists = $scope.vm.recentlyOptions.some(function (existingItem) {
                            return value === existingItem.value;
                        });
                        // if there is not one adds a new item
                        if (!alreadyExists) {
                            var item = {
                                label: value,
                                value: value,
                                nonstoreable: false
                            }
                            $scope.vm.recentlyOptions.push(item);
                        }

                        // masks the option with the given value as selected
                        $scope.selectedOptions[value] = 1;
                    }

                    function applyModal(modalSchema) {
                        // resets the selected options
                        $scope.selectedOptions = [];

                        // adds an option for each row on modal's grid buffer
                        var buffer = crudContextHolderService.getSelectionModel(modalService.panelid).selectionBuffer;
                        var attFieldName = $scope.filter.advancedFilterAttribute || modalSchema.idFieldName;
                        for (var id in buffer) {
                            if (!buffer.hasOwnProperty(id)) {
                                continue;
                            }
                            var datamap = buffer[id];
                            var value = datamap.fields[attFieldName];
                            addLookupOption(value);
                        }

                        // updates the local buffer
                        // used when the lookup modal opens again
                        // originaly the modal loses the grid buffer on hide
                        if (Object.keys(buffer).length > 0) {
                            $scope.lookupModalBuffer = angular.copy(buffer);
                        } else {
                            $scope.lookupModalBuffer = {};
                        }

                        // redo the search
                        $scope.modifySearchData();
                        modalService.hide();
                    }

                    $scope.$on("sw.crud.list.filter.modal.apply", function (event, args) {
                        if (!isOptionFilter) {
                            return;
                        }
                        var promise = modalFilterService.getModalFilterSchema($scope.filter, $scope.schema);
                        promise.then(function (modalSchema) {
                            if (modalSchema !== args[0]) {
                                return;
                            }
                            applyModal(modalSchema);
                        });
                    });
                }]



            };

            return directive;

        }]);

})(angular);

