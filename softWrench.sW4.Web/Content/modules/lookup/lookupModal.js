(function (angular) {
    "use strict";

    angular.module('sw_lookup')
        .directive('lookupModalWrapper', function ($compile, $timeout) {
            "ngInject";

            return {
                restrict: "E",
                replace: true,
                scope: {
                    lookupObj: '=',
                    schema: '=',
                    datamap: '='
                },
                template: "<div></div>",
                link: function (scope, element, attrs) {
                    if (!scope.datamap) {
                        scope.datamap = {};
                    }

                    element.append(
                    "<lookup-modal lookup-obj='lookupObj'" +
                        "schema='schema' datamap='datamap'>" +
                    "</lookup-modal>"
                    );

                    $timeout(function () {
                        //lazy load the real directive gaining in performance
                        $compile(element.contents())(scope);

                    }, 0, false);
                },

            };
        })
    .directive('lookupModal', function (contextService) {
        "ngInject";

        return {
            restrict: 'E',
            replace: true,
            templateUrl: contextService.getResourceUrl('/Content/modules/lookup/templates/lookupModal.html'),
            scope: {
                lookupObj: '=',
                schema: '=',
                datamap: '='
            },

            controller: function ($injector, $scope, $http, $element, searchService, i18NService, associationService,
                                  formatService, expressionService, focusService) {

                $scope.lookupModalSearch = function (pageNumber) {
                    focusService.resetFocusToCurrent($scope.schema, $scope.lookupObj.fieldMetadata.attribute);

                    $scope.lookupObj.quickSearchDTO = {
                        quickSearchData: $scope.lookupsearchdata
                    }
                    associationService.getLookupOptions($scope.schema, $scope.datamap, $scope.lookupObj, pageNumber, $scope.searchObj).then(function (data) {
                        var result = data.resultObject;
                        $scope.populateModal(result);
                    });
                };

                $scope.populateModal = function (associationResult) {
                    $scope.lookupObj.options = associationResult.associationData;
                    $scope.lookupObj.schema = associationResult.associationSchemaDefinition;
                    var modalPaginationData = $scope.lookupObj.modalPaginationData;
                    modalPaginationData.pageCount = associationResult.pageCount;
                    modalPaginationData.pageNumber = associationResult.pageNumber;
                    modalPaginationData.pageSize = associationResult.pageSize;
                    modalPaginationData.totalCount = associationResult.totalCount;
                    modalPaginationData.selectedPage = associationResult.pageNumber;
                    //TODO: this should come from the server side
                    modalPaginationData.paginationOptions = associationResult.paginationOptions || [10, 30, 100];
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
                $scope.lookupModalSelect = function (option) {
                    var fieldMetadata = $scope.lookupObj.fieldMetadata;
                    $scope.selectedOption = option;
                    $scope.datamap[fieldMetadata.target] = option.value;
                    associationService.updateUnderlyingAssociationObject(fieldMetadata, option, $scope);
                    $element.modal('hide');
                };

                $element.on('hide.bs.modal', function (e) {
                    $scope.lookupObj.quickSearchDTO = null;
                    $('body').removeClass('modal-open');
                    $('.modal-backdrop').remove();
                    if ($scope.lookupObj) {
                        $scope.lookupObj.description = null;
                    }

                    if ($scope.lookupObj.fieldMetadata == null) {
                        return;
                    }

                });

                $scope.hideLookupModal = function (target) {
                    $scope.modalCanceled = true;
                    $scope.lookupObj.description = null;
                    var modals = $('[data-attribute="{0}"]'.format(target));
                    modals.modal('hide');
                };

                $element.on('shown.bs.modal', function (e) {
                    if ($scope.lookupObj.item) {
                        $scope.datamap = $scope.lookupObj.item;
                    }
                    $scope.modalCanceled = false;
                    $scope.selectedOption = null;
                    $scope.searchObj = {};
                    //                $scope.populateModal($scope.lookupObj);
                    $scope.$digest();
                });

                $injector.invoke(BaseList, this, {
                    $scope: $scope,
                    formatService: formatService,
                    expressionService: expressionService,
                    searchService: searchService
                });
            }
        };
    });

})(angular);