(function (angular) {
    "use strict";

    angular.module("sw_layout")
        .directive("filterMultipleOption", ["$log", "contextService", "restService", "filterModelService", "cmpAutocompleteServer", "$timeout", "searchService", "schemaService", "modalFilterService", "modalService", "crudContextHolderService",
            function ($log, contextService, restService, filterModelService,
            cmpAutocompleteServer, $timeout, searchService, schemaService, modalFilterService, modalService, crudContextHolderService) {

                var directive = {
                    restrict: "E",
                    templateUrl: contextService.getResourceUrl("/Content/Templates/crud/filter/crud_filter_multipleoptions.html"),
                    scope: {
                        filter: '=',
                        searchData: '=',
                        searchOperator: '=',
                        schema: '=',
                        applyFilter: "&",
                        panelid: "="
                    },

                    link: function (scope, element, attrs) {
                        scope.vm = {};

                        var linkLog = $log.getInstance("filterMultipleOption", ["link"]);

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
                        linkLog.debug("Initing metadata options from filter of attribute (" + filter.attribute + ").");
                        scope.filter.options.forEach(function (item) {
                            linkLog.debug("Initing metadata option (" + item.value + ") " + item.label);
                            //these won´t go to currently used
                            item.nonstoreable = true;
                            scope.suggestedoptions.push(item);
                            scope.preSelectOptionIfNeeded(item, linkLog);
                        });

                        //select filter options for the top level preselect condition
                        scope.preSelectOption(scope.filter.options, linkLog);

                        if (!filter.lazy && filter.provider) {
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
                                scope.filteroptions = removeDuplicatesOnArray(scope.filteroptions, "value");

                                //select filter options for the top level preselect condition
                                scope.preSelectOption(scope.filteroptions, linkLog);
                                scope.parseSearchData();
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
                                    } else {
                                        scope.vm.hasMoreItems = false;
                                    }

                                    //updating the list of options
                                    scope.filteroptions = response;
                                    scope.$digest();
                                });

                                scope.parseSearchData();
                            }, 0, false);

                        }
                    },

                    controller: ["$scope", "$rootScope", function ($scope, $rootScope) {
                        var filter = $scope.filter;

                        $scope.labelValue = function (option) {
                            if (option.value === "nullor:") {
                                return option.label;
                            }
                            // verifies if the display code is set on option, if not verifies on filter
                            var displaycodeOptionDefined = typeof (option.displayCode) != "undefined";
                            if ((displaycodeOptionDefined && !option.displayCode) || (!displaycodeOptionDefined && filter.displayCode === false)) {
                                return option.label;
                            }
                            var label = "(" + option.value + ")";
        
                            if (filter.rendererType === 'statusicons' && !!option.label) {
                                label = option.label;
                            } else if (!!option.label) {
                                label += " - " + option.label;
                            }
                            return label;
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
                                $timeout(function () {
                                    $scope.jelement.typeahead('val', '');
                                }, 0, false);

                            }
                        });

                        $scope.getAllAvailableOptions = function () {
                            var allOptions = $scope.filteroptions.concat($scope.suggestedoptions).concat($scope.vm.recentlyOptions);
                            return removeDuplicatesOnArray(allOptions, "value");
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

                        function updateLookupModalGridBufferMultiple(selectedValues) {
                            // updates lookup modal grid buffer
                            // when lookup modal is opened the lookup modal grid buffer is passed
                            // and any selected option will be selected on modal too
                            $scope.lookupModalBuffer = {};
                            if (!selectedValues || selectedValues.length === 0) {
                                return;
                            }

                            var promise = modalFilterService.getModalFilterSchema($scope.filter, $scope.schema);
                            promise.then(function (modalSchema) {
                                var attFieldName = $scope.filter.advancedFilterAttribute || modalSchema.idFieldName;
                                selectedValues.forEach(function (value) {
                                    var datamap = { fields: {} };
                                    datamap.fields[attFieldName] = value;
                                    $scope.lookupModalBuffer[value] = datamap;
                                });
                            });
                        }


                        // compare two selection options, priority is given to the one that have data with more values
                        // this is use to prioritise options that agregates many search data on the parse
                        function compareSelectionOptions(a, b) {
                            if (a.parsedValues.length > b.parsedValues.length) {
                                return -1;
                            }
                            if (a.parsedValues.length < b.parsedValues.length) {
                                return 1;
                            }
                            return 0;
                        }

                        // matches the search data with the select options, gives priority to options that have
                        // more search data (ex. "a,b,c,f" > "c,f")
                        function innerParseSearchData(dataOptions) {
                            var allOptions = $scope.getAllAvailableOptions();
                            var optionsWithParsedValues = [];

                            // parse the value of the option ("1,2,3" -> [1, 2, 3])
                            // and sort the optios by the number of values desc
                            allOptions.forEach(function (option) {
                                var parsedOption = {
                                    option: option,
                                    parsedValues: filterModelService.parseOptions(option.value)
                                }
                                optionsWithParsedValues.push(parsedOption);
                            });
                            optionsWithParsedValues.sort(compareSelectionOptions);

                            // verifies if the options have the search data and should be selected
                            optionsWithParsedValues.forEach(function (parsedOption) {
                                // dataOptions array loses entries so should be verified empty
                                if (dataOptions.length === 0) {
                                    return;
                                }

                                // verifies if option value is entirely on search data
                                var containsAllData = parsedOption.parsedValues.every(function(data) {
                                    return dataOptions.indexOf(data) >= 0;
                                });
                                if (!containsAllData) {
                                    return;
                                }

                                // marks the option as selected and consumes the search data
                                $scope.selectedOptions[parsedOption.option.value] = 1;
                                dataOptions = dataOptions.filter(function(data) {
                                    return parsedOption.parsedValues.indexOf(data) < 0;
                                });
                            });
                        }

                        $scope.parseSearchData = function (searchData) {
                            $scope.selectedOptions = [];

                            //make sure statusicons options are checked
                            if (filter.rendererType == 'statusicons') {
                                filter.options.forEach(function(option) {
                                    if ($scope.searchData[option.value] === '1') {
                                        $scope.selectedOptions[option.value] = 1;
                                    }
                                });
                            }

                            var data = null;
                            if (!searchData) {
                                if ($scope.searchData) {
                                    data = $scope.searchData[filter.attribute];
                                }
                            } else {
                                data = searchData;
                            }

                            $scope.cacheAtributeSearchData = data;
                            var dataOptions = data && typeof data == "string" ? filterModelService.parseOptions(data) : [];
                            if (dataOptions.length > 0) {
                                // clones the array data to enable array customization
                                innerParseSearchData([].concat(dataOptions));
                            }

                            if ($scope.filter.advancedFilterSchemaId) {
                                updateLookupModalGridBufferMultiple(dataOptions);
                            }
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

                            if (filter.rendererType == 'statusicons') {
                                searchData[filter.attribute] = null;
                                searchOperator[filter.attribute] = null;

                                //remove all search data (prevent unselected options from remaining)
                                var availableOptions = $scope.getAllAvailableOptions();
                                availableOptions.forEach(function (option) {
                                    delete searchData[option.value];
                                    delete searchOperator[option.value];
                                });

                                //add all checked otopions
                                if (result) {
                                    var optionList = result.split(',');

                                    //if multiple options selected
                                    if (optionList.length > 0) {
                                        optionList.forEach(function(option) {
                                            searchData[option] = 1;
                                            searchOperator[option] = searchService.getSearchOperationById("GTE");
                                        });
                                    } else {
                                        searchData[result] = 1;
                                        searchOperator[result] = searchService.getSearchOperationById("GTE");
                                    }
                                }
                            }

                            $scope.cacheAtributeSearchData = result;
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

                        $scope.preSelectOption = function (options, log) {
                            if ($scope.filter.preselected !== undefined && $scope.filter.preselected !== '') {
                                $scope.filter.preselected.split(",").forEach(function (value) {
                                    options.forEach(function (item) {
                                        //these won´t go to currently used
                                        item.nonstoreable = true;
                                        if (item.value === value.trim()) {
                                            log.debug("Option pre selected: (" + item.value + ") " + item.label);
                                            $scope.selectedOptions[item.value] = 1;
                                        }
                                    });
                                });
                            }
                        }

                        $scope.preSelectOptionIfNeeded = function (option, log) {
                            var searchValue = $scope.searchData[filter.attribute];
                            if (option.preSelected && searchValue && searchValue.indexOf(option.value) >= 0) {
                                log.debug("Option pre selected: (" + option.value + ") " + option.label);
                                $scope.selectedOptions[option.value] = 1;
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
                        function addLookupOption(value, label) {
                            // searchs for itens with same value on filter options
                            var alreadyExists = $scope.vm.recentlyOptions.some(function (existingItem) {
                                return value === existingItem.value;
                            });
                            // if there is not one adds a new item
                            if (!alreadyExists) {
                                var item = {
                                    label: label,
                                    value: value,
                                    nonstoreable: false,
                                    removable: true
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
                                var label = value;
                                if (datamap.fields.hasOwnProperty("description")) {
                                    //TODO: receive this from metadata somehow
                                    label = datamap.fields["description"];
                                }
                                addLookupOption(value, label);
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

                        // When changing grids the selection should be restarted
                        $scope.$on("sw_gridchanged", function () {
                            var log = $log.getInstance("filterMultipleOption#sw_gridchanged", ["grid"]);
                            log.debug("grid change, reset of selected options of filter: " + filter.attribute);
                            $scope.selectedOptions = [];
                            filter.options.forEach(function (item) {
                                $scope.preSelectOptionIfNeeded(item, log);
                            });

                            //select filter options for the top level preselect condition
                            $scope.preSelectOption($scope.filteroptions, log);
                        });

                        // This listener is designed to update the selected options after anything indirectely changes the searchdata besides the user selecting a filter option.
                        // It's called everytime the grid receives data from server and updates the grid data.
                        // To avoid changes that are directly made to the filter and changes to another filters $scope.cacheAtributeSearchData is used.
                        $scope.$on("sw_griddatachanged", function (event, datamap, schema, panelid) {
                            if ($scope.paneild != panelid) {
                                return;
                            }
                            // timeout is used to enables $scope.searchData from this scope to be updated with $scope.searchData from crud_list
                            $timeout(function () {
                                var atributeSearchData = $scope.filter && $scope.searchData && $scope.searchData[$scope.filter.attribute];
                                if ($scope.cacheAtributeSearchData == atributeSearchData) {
                                    return;
                                }
                                $scope.parseSearchData(atributeSearchData);
                            }, 0, false);
                        });
                    }]

                };

                return directive;
            }]);
})(angular);