(function (angular) {
    "use strict";

    angular.module('sw_lookup')
        .directive('lookupModalWrapper', function ($compile) {
            "ngInject";

            return {
                restrict: "E",
                replace: true,
                scope: {
                    lookupObj: '=',
                    schema: '=',
                    datamap: '=',
                    loadedmodals: '='
                },
                template: "<div></div>",
                link: function (scope, element) {
                    if (!scope.datamap) {
                        scope.datamap = {};
                    }
                    scope.name = "lookupmodalWrapper";

                    element.append(
                    "<lookup-modal loadedmodals='loadedmodals' lookup-obj='lookupObj'" +
                        "schema='schema' datamap='datamap'>" +
                    "</lookup-modal>"
                    );

                    //lazy load the real directive gaining in performance
                    $compile(element.contents())(scope);
                }

            };
        })
    .directive('lookupModal', function (contextService, lookupService) {
        "ngInject";

        return {
            restrict: 'E',
            replace: true,
            templateUrl: contextService.getResourceUrl('/Content/modules/lookup/templates/lookupModal.html'),
            scope: {
                lookupObj: '=',
                schema: '=',
                datamap: '=',
                loadedmodals: '='
            },

            link: function (scope, element) {
                scope.name = "lookupmodal";
                element.draggable();
                element.modal('show');
            },


            controller: function ($q,$injector, $scope, $http, $element, searchService, i18NService, associationService,
                                  formatService, expressionService,  contextService, crudContextHolderService) {

                $scope.searchData = {};
                $scope.searchOperator = {};
                $scope.searchSort = {};
                $scope.searchObj = {};

                $scope.lookupModalSearch = function (pageNumber) {
                    if (!$scope.loadedmodals[$scope.lookupObj.fieldMetadata.attribute]) {
                        //handling the case where an esc key is hit, closing the modal, but the ng-change is still active.
                        return null;
                    }


                    $scope.searchObj = searchService.buildSearchDTO($scope.searchData, $scope.searchSort, $scope.searchOperator, null, {pageNumber}, $scope.searchTemplate, {quickSearchData: $scope.lookupsearchdata});
                    $scope.searchObj.addPreSelectedFilters = false;

                    return lookupService.getLookupOptions($scope.lookupObj, $scope.searchObj, $scope.datamap);
                };

                $scope.shouldShowPagination = function () {
                    if (!$scope.lookupObj.modalPaginationData || !$scope.lookupObj.modalPaginationData.paginationOptions) {
                        return false;
                    }

                    return !crudContextHolderService.getSelectionModel($scope.panelid).showOnlySelected && !!$scope.lookupObj.modalPaginationData && $scope.lookupObj.modalPaginationData.paginationOptions.some(function (option) {
                        // totalCount is bigger than at least one option
                        return option !== 0 && $scope.lookupObj.modalPaginationData.totalCount > option;
                    });;
                }

//                $scope.populateModal = function (associationResult) {
//                    $scope.lookupObj.options = associationResult.associationData;
//                    $scope.lookupObj.schema = associationResult.associationSchemaDefinition;
//                    const modalPaginationData = $scope.lookupObj.modalPaginationData;
//                    modalPaginationData.pageCount = associationResult.pageCount;
//                    modalPaginationData.pageNumber = associationResult.pageNumber;
//                    modalPaginationData.pageSize = associationResult.pageSize;
//                    modalPaginationData.totalCount = associationResult.totalCount;
//                    modalPaginationData.selectedPage = associationResult.pageNumber;
//                    //TODO: this should come from the server side
//                    modalPaginationData.paginationOptions = associationResult.paginationOptions || [10, 30, 100];
//                };

                $scope.filterForColumn = function (column) {
                    return $scope.lookupObj.schema.schemaFilters.filters.find(function (filter) {
                        return filter.attribute === column.attribute;
                    });
                };

                $scope.selectAllChecked = false;

                $scope.filterApplied = function () {
                    $scope.lookupModalSearch(1);
                };

                $scope.checkboxIconClass = function (value) {
                    return (value === 1 || value === true || "yes".equalsIc(value)) ? "fa-check-square-o" : "fa-square-o";
                }

                $scope.sort = function (column) {
                    if (!$scope.shouldShowHeaderLabel(column) || "none" === $scope.schema.properties["list.sortmode"]) {
                        return;
                    }
                    const columnName = column.attribute;
                    const sorting = $scope.searchSort;
                    if (sorting.field != null && sorting.field === columnName) {
                        sorting.order = sorting.order === "desc" ? "asc" : "desc";
                    } else {
                        sorting.field = columnName;
                        sorting.order = "asc";
                    }
                    $scope.lookupModalSearch(1);
                };



                $scope.sortLabel = function (column) {
                    if (!column) {
                        return $scope.i18N("_grid.filter.clicksort", "Click here to sort");
                    }
                    if (column.rendererParameters.showsort === "false") {
                        return "";
                    }
                    return $scope.i18N("_grid.filter.clicksort", "{0}, Click to sort".format(column.toolTip ? column.toolTip : column.label));
                }

                $scope.shouldShowSort = function (column, orientation) {
                    const defaultCondition = !!column.attribute && ($scope.searchSort.field === column.attribute || $scope.searchSort.field === column.rendererParameters["sortattribute"]) && $scope.searchSort.order === orientation;
                    return defaultCondition;
                };

                $scope.showDescription = function () {

                    if ($scope.lookupObj.fieldMetadata == undefined) {
                        return true;
                    }

                    if ($scope.lookupObj.fieldMetadata.hideDescription == undefined) {
                        return true;
                    }

                    return !$scope.lookupObj.fieldMetadata.hideDescription;
                };

                $scope.i18N = function (key, defaultValue, paramArray) {
                    return i18NService.get18nValue(key, defaultValue, paramArray);
                };

                $scope.i18NLabel = function (fieldMetadata) {
                    return i18NService.getI18nLabel(fieldMetadata, $scope.lookupObj.schema);
                };

                $scope.getLookUpDescriptionLabel = function (fieldMetadata) {
                    return i18NService.getLookUpDescriptionLabel(fieldMetadata);
                };
                
                /**
                 * Called when the user hits an option of the lookup modal, selecting the value de facto and closing it.
                 * @param {} option 
                 * @returns {} 
                 */
                $scope.lookupModalSelect = function (option) {
                    const fieldMetadata = $scope.lookupObj.fieldMetadata;
                    $scope.selectedOption = option;
                    $scope.datamap[fieldMetadata.target] = option.value;
                    associationService.updateUnderlyingAssociationObject(fieldMetadata, new AssociationOptionDTO(option), $scope);
                    $element.modal('hide');
                };

                $element.on('hide.bs.modal', ()=> {
                    $scope.lookupObj.quickSearchDTO = null;
                    $('body').removeClass('modal-open');
                    $('.modal-backdrop').remove();
                    if ($scope.lookupObj) {
                        $scope.lookupObj.description = null;
                    }

                    if ($scope.lookupObj.fieldMetadata == null) {
                        return;
                    }
                    //this flag is present at the lookupinput.html to include/exclude the modals from the screen
                    //this way we make sure that there´s only dom for the modal when it´s opened
                    //whenever we close the modal angular will remove it
                    delete $scope.loadedmodals[$scope.lookupObj.fieldMetadata.attribute];


                });

                $scope.hideLookupModal = function (target) {
                    $scope.modalCanceled = true;
                    $scope.lookupObj.description = null;
                    const modals = $('[data-attribute="{0}"]'.format(target));
                    modals.modal('hide');
                };

                $element.on('shown.bs.modal', ()=> {
                    if ($scope.lookupObj.item) {
                        $scope.datamap = $scope.lookupObj.item;
                    }

                    $scope.searchData = $scope.lookupObj.searchData || {};
                    $scope.searchOperator = $scope.lookupObj.searchOperator || {};
                    $scope.modalCanceled = false;
                    $scope.selectedOption = null;
                    $scope.$digest();
                    $('[data-type="lookupsearchinput"]').focus();
                });

                $injector.invoke(BaseList, this, {
                    $scope: $scope,
                    formatService,
                    expressionService,
                    searchService
                });
            }
        };
    });

})(angular);