(function (angular, BaseController, BaseList, $) {
    "use strict";

    angular.module("sw_layout")
        .directive("crudFilterDropdown", ["contextService", "$timeout", function (contextService, $timeout) {
            var directive = {
                restrict: "E",
                templateUrl: contextService.getResourceUrl("/Content/Templates/crud/filter/crud_filter_dropdown.html"),
                scope: {
                    filter: "=", // filter to show
                    schema: "=", // model's current schema
                    datamap: "=", // model's current datamap
                    searchData: "=", // shared dictionary [column : current filter search value]
                    searchOperator: "=", // shared dictionary [column : current filter operator object]
                    selectAll: "=", // shared boolean flag indicating if multiple select in filters is selected
                    advancedFilterMode: "=", // shared boolean flag indicating if advanced filter mode is activated
                    filterApplied: "&", // callback executed when the filters are applied
                    panelid: "="
                },
                //#region controller
                controller: ["$scope", "$injector", "i18NService", "fieldService", "commandService", "formatService", "expressionService", "searchService", "filterModelService", "modalService", "schemaCacheService", "restService", "dispatcherService", "$q", "modalFilterService", "crudContextHolderService", "gridSelectionService",
                    function ($scope, $injector, i18NService, fieldService, commandService, formatService, expressionService, searchService, filterModelService, modalService, schemaCacheService, restService, dispatcherService, $q, modalFilterService, crudContextHolderService, gridSelectionService) {

                        $scope.layout = {
                            standalone: false
                        }

                        var config = {
                            /** 'don't filter' operator: helper to clear current filter */
                            noopoperator: { id: "NF", symbol: "", begin: "", end: "", title: "No Filter" }
                        };

                        $scope.markDefaultOperator = function (filter) {
                            var operator = $scope.searchOperator[filter.attribute];
                            if (!operator || operator.id === "NF") {
                                if (filter.type === "MetadataDateTimeFilter") {
                                    $scope.searchOperator[filter.attribute] = searchService.getSearchOperationById("GTE");
                                } else {
                                    $scope.searchOperator[filter.attribute] = searchService.defaultSearchOperation();
                                }
                            }
                        }

                        $scope.hasFilter = function (filter) {
                            if (filter.rendererType === 'statusicons') {
                                return filter.options.some(option => $scope.searchData[option.value] === '1');
                            }

                            var operator = $scope.searchOperator[filter.attribute];
                            if (!operator) {
                                return false;
                            }

                            var search = $scope.searchData[filter.attribute];
                            if (operator.id === "BLANK") {
                                return true;
                            }

                            return !!search && !operator.id.equalsAny("", "NF");
                        }

                        /**
                         * Sets the operator in $scope.searchOperator[columnName]
                         * 
                         * @param String columnName 
                         * @param {} operator 
                         */
                        $scope.selectOperator = function (columnName, operator) {
                            if ($scope.searchOperator[columnName] && $scope.searchOperator[columnName].id === operator.id) {
                                $scope.filterIsActive = true;
                                this.filterBarApplied();
                                return;
                            }
                            $scope.searchOperator[columnName] = operator;
                            var searchData = $scope.searchData;

                            if (operator.id.equalsAny("", "NF")) {
                                searchData[columnName] = "";
                            } else if (operator.id === "BTW") {
                                //changing to btw requires second item to be added
                                return;
                            }
                            else if (searchData[columnName] != null && searchData[columnName] !== "") {
                                //if there is a search value, apply on click
                                this.filterBarApplied();
                            } else if (operator.id === "BLANK") {
                                searchData[columnName] = "";
                                this.filterBarApplied();
                            }
                        };

                        /**
                         * Clears the filter associated to columnName 
                         * and invokes the 'filterApplied' callback.
                         * 
                         * @param String columnName 
                         */
                        $scope.clearFilter = function (filter) {
                            if (filter.options) {
                                filter.options.forEach(function(option) {
                                    delete $scope.searchData[option.value];
                                    delete $scope.searchOperator[option.value];
                                });
                            }

                            var filterAttribute = filter.attribute;
                            $(".dropdown.open").removeClass("open");
                            $scope.selectOperator(filterAttribute, config.noopoperator);
                            $scope.filterApplied();
                            $scope.$broadcast("sw.filter.clear", filterAttribute);
                        };

                        /**
                         * Intermediary function to perform operations specific to this directive before delegating to the & delegated function
                         */
                        $scope.filterBarApplied = function (keepitOpen) {
                            if (true !== keepitOpen) {
                                $(".dropdown.open").removeClass("open");
                            }

                            $scope.markDefaultOperator($scope.filter);

                            Object.keys($scope.searchOperator).forEach(function (item) {
                                if ($scope.searchOperator[item] && $scope.searchOperator[item].id === "BTW") {
                                    delete $scope.searchOperator[item + "_end"];
                                    if (!$scope.searchData[item + "_end"]) {
                                        //to prevent exceptions if the user hits the apply without having both values set
                                        $scope.searchData[item + "_end"] = $scope.searchData[item];
                                    }
                                }
                            });

                            $scope.filterApplied();
                        }

                        /**
                         * Immediately applies the filter associated to columnName.
                         * 
                         * @param String columnName 
                         */
                        $scope.filterSearch = function (columnName) {
                            if (!$scope.searchOperator[columnName] || $scope.searchOperator[columnName].symbol === "") {
                                $scope.searchOperator[columnName] = searchService.defaultSearchOperation();
                            }
                            var searchString = $scope.searchData[columnName];
                            if (!searchString) {
                                $scope.searchOperator[columnName] = searchService.getSearchOperationById("BLANK");
                                $scope.searchData[columnName] = " ";
                            }
                            $(".dropdown.open").removeClass("open");
                            this.filterBarApplied();
                        };

                        /**
                         * Marks the $scope.datamap's values's fields as checked ("_#selected" key)
                         * 
                         * @param Boolean checked 
                         */
                        $scope.toggleSelectAll = function (checked) {
                            angular.forEach($scope.datamap, function (value) {
                                value.fields["_#selected"] = checked;
                            });
                        };

                        $scope.getFilterText = function (filter) {
                            var filterText = filterModelService.getFilterText(filter, $scope.searchData, $scope.getOperator(filter.attribute));
                            // adding some spaces on filter text to enable the tooltip break lines and do not overflow in width
                            if (filterText) {
                                filterText = filterText.split(",").join(", ");
                            }
                            return filterText;
                        }

                        function collapseOperatorList($event, mode) {
                            // required to 'stop' the event in input groups
                            // some inputs (like datepicker) trigger focus
                            $event.preventDefault();
                            $event.stopPropagation();
                            $event.stopImmediatePropagation();
                            // toggling list collapse
                            $($event.delegateTarget)
                                .parents(".js_filter_content")
                                .find("ul.js_operator_list")
                                .collapse(mode);
                        }

                        $scope.toggleCollapseOperatorList = function ($event) {
                            collapseOperatorList($event, "toggle");
                        }

                        $scope.closeCollapseOperatorList = function ($event) {
                            collapseOperatorList($event, "hide");
                        }

                        /**
                         * Retrieves the tooltip of the current search operator applied to the attribute.
                         * If no operator is selected for the attribute it uses the default search operator.
                         * 
                         * @param String attribute 
                         * @returns String 
                         */
                        $scope.getOperatorTooltip = function (attribute) {
                            var operator = $scope.getOperator(attribute);
                            return !!operator && !!operator.id ? operator.tooltip : $scope.getDefaultOperator().tooltip;
                        };

                        /**
                         * Retrieves the icon of the current search operator applied to the attribute.
                         * If no operator is selected for the attribute it uses the default search icon
                         * (icon for the default operator).
                         * 
                         * @param String attribute 
                         * @returns String 
                         */
                        $scope.getOperatorIcon = function (attribute) {
                            var icon = $scope.getSearchIcon(attribute);
                            return !!icon ? icon : $scope.getDefaultSearchIcon();
                        };

                        /**
                         * Filters the operations that should be displayed.
                         * 
                         * @param {} filter
                         * @returns Array<Operation> 
                         */
                        $scope.displayableSearchOperations = function (filter) {
                            var operations = $scope.searchOperations();
                            if (!operations) return operations;
                            return operations.filter(function (operation) {
                                return $scope.shouldShowFilter(operation, filter);
                            });
                        }

                        $scope.closeFilterDropdown = function ($event) {
                            $($event.delegateTarget).parents(".dropdown.open").removeClass("open");
                        }

                        $scope.setStandaloneMode = function (value) {
                            $scope.layout.standalone = value;
                        };

                        $scope.isModal = function (filter) {
                            return !filter || "MetadataModalFilter" === filter.type;
                        }

                        $scope.initModal = function () {
                            $scope.modalDatamap = {};
                            modalFilterService.getModalFilterSchema($scope.filter, $scope.schema);
                        }

                        $scope.showModal = function (filter) {
                            if (!$scope.isModal(filter)) {
                                return;
                            }
                            $scope.innerShowModal(filter);
                        }

                        $scope.innerShowModal = function (filter) {
                            var datamap = $scope.hasFilter(filter) && $scope.modalDatamap ? $scope.modalDatamap[filter.attribute] : {};
                            datamap = datamap ? datamap : {};
                            var filterI18N = $scope.i18N("_grid.filter.filter", "Filter");
                            var properties = {
                                title: filter.label + " " + filterI18N,
                                cssclass: "crud-grid-modal"
                            };

                            modalFilterService.getModalFilterSchema(filter, $scope.schema).then(function (modalSchema) {
                                modalService.show(modalSchema, datamap, properties, $scope.appyModal);
                            });
                        }

                        $scope.appyModal = function (datamap, modalSchema) {
                            var att = $scope.filter.attribute;
                            $scope.modalDatamap[att] = datamap;
                            $scope.filterIsActive = true;
                            modalService.hide();

                            var serviceParams = [datamap, modalSchema, $scope.filter];
                            var searchData = dispatcherService.invokeServiceByString($scope.filter.service, serviceParams);
                            $scope.searchData[att] = searchData;
                            var searchOperator = searchService.getSearchOperator(searchData);
                            $scope.selectOperator(att, searchOperator);
                        }

                        $scope.booleanFilterTrueLabel = $scope.filter && $scope.filter.trueLabel ? $scope.filter.trueLabel : i18NService.get18nValue("_grid.filter.boolean.true", "Yes");
                        $scope.booleanFilterTrueValue = $scope.filter && $scope.filter.trueValue ? $scope.filter.trueValue : "1";
                        $scope.booleanFilterFalseLabel = $scope.filter && $scope.filter.falseLabel ? $scope.filter.falseLabel : i18NService.get18nValue("_grid.filter.boolean.false", "No");
                        $scope.booleanFilterFalseValue = $scope.filter && $scope.filter.falseValue ? $scope.filter.falseValue : "0";

                        $scope.$on("sw.crud.list.filter.modal.clear", function (event, args) {
                            var promise = modalFilterService.getModalFilterSchema($scope.filter, $scope.schema);
                            promise.then(function (modalSchema) {
                                if (modalSchema !== args[0]) {
                                    return;
                                }

                                // if the modal is a grid clears the selection and refreshs the grid
                                if (modalSchema.stereotype.toLocaleLowerCase().startsWith("list")) {
                                    gridSelectionService.clearSelection(null, null, modalService.panelid);
                                    dispatcherService.dispatchevent("sw.crud.list.clearQuickSearch", modalService.panelid);
                                    searchService.refreshGrid({}, null, { panelid: modalService.panelid });
                                }

                                $scope.clearFilter($scope.filter.attribute);

                                // only hides the modal if the filter is a modalFilter
                                if ($scope.isModal($scope.filter)) {
                                    modalService.hide();
                                }
                            });
                        });

                        $scope.$on("sw.crud.list.filter.modal.cancel", function (event, args) {
                            var promise = modalFilterService.getModalFilterSchema($scope.filter, $scope.schema);
                            promise.then(function (modalSchema) {
                                if (modalSchema !== args[0]) {
                                    return;
                                }
                                modalService.hide();
                            });
                        });

                        $scope.$on("sw.crud.list.filter.modal.show", function (event, filter) {
                            if (filter !== $scope.filter) {
                                return;
                            }
                            $scope.innerShowModal(filter);
                        });

                        $injector.invoke(BaseController, this, {
                            $scope: $scope,
                            i18NService: i18NService,
                            fieldService: fieldService,
                            commandService: commandService,
                            formatService: formatService
                        });

                        // 'inherit' from BaseList controller
                        $injector.invoke(BaseList, this, {
                            $scope: $scope,
                            formatService: formatService,
                            expressionService: expressionService,
                            searchService: searchService,
                            commandService: commandService
                        });

                    }],
                //#endregion
                //#region postlink
                link: function (scope, element, attrs) {
                    scope.setStandaloneMode(attrs.hasOwnProperty("filterStandalone"));

                    var prepareUi = function () {
                        // don't let dropdowns close automatically when clicked inside
                        var dropdowns = angular.element(element[0].querySelectorAll('.js_filter .dropdown .dropdown-menu'));
                        dropdowns.click(function (event) {
                            event.stopPropagation();
                        });

                        //if the filter is on the right side of the screen, reposition if needed
                        var windowWidth = $(window).width();
                        var dropdown = angular.element(element[0].querySelectorAll('.crud-grid-modal .js_filter .dropdown'));

                        //if the dropdown is inside a modal
                        if (dropdown.length > 0) {
                            windowWidth = $('.crud-grid-modal').width();
                        } else {
                            dropdown = angular.element(element[0].querySelectorAll('.js_filter .dropdown'));
                        }

                        //when the dropdown open check position and widths
                        dropdown.on('shown.bs.dropdown', function () {
                            var dropdownMenu = dropdown.children('.dropdown-menu');
                            if (dropdown.position().left + dropdownMenu.width() > windowWidth) {
                                var widthOffset = dropdownMenu.width() - dropdown.width();
                                dropdownMenu.css({ left: '-' + widthOffset + 'px' });;
                            }
                        });

                        //prevent clicking outside of datepicker from closing the filter dropdown
                        dropdown.on('hide.bs.dropdown', function () {
                            var dd = $('.dropdown-menu.keep-open', dropdown);

                            if (dd.length > 0) {
                                return false;
                            } else {
                                return true;
                            }
                        });

                        //toogle keep-open class when the datepicker is shown/hidden
                        $('[data-datepicker="true"]', dropdown).on('dp.show', function () {
                            $('.dropdown-menu', dropdown).addClass('keep-open');
                        });

                        $('[data-datepicker="true"]', dropdown).on('dp.hide', function (e) {
                            $timeout(function () {
                                $('.dropdown-menu', dropdown).removeClass('keep-open');
                            }, 200, false);
                        });

                        //autofocus the search input when the dropdown opens
                        $(".js_filter .dropdown").on("show.bs.dropdown", function (event) {
                            $timeout(function () {
                                $(event.target).find("input[type=search]").focus();
                            });
                        });
                    }

                    $timeout(prepareUi, 0, false);

                    scope.$on("sw_griddatachanged", function () {
                        // need to register this call for whenever the grid changes
                        $timeout(prepareUi, 0, false);
                    });
                }
                //#endregion
            };
            return directive;
        }]);

})(angular, BaseController, BaseList, jQuery);