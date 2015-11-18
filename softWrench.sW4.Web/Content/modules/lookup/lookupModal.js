var app = angular.module('sw_lookup');

app.directive('lookupModalWrapper', function ($compile) {
    return {
        restrict: "E",
        replace: true,
        scope: {
            lookupAssociationsCode: '=',
            lookupAssociationsDescription: '=',
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
                "lookup-associations-code='lookupAssociationsCode'" +
                "lookup-associations-description='lookupAssociationsDescription'" +
                "schema='schema' datamap='datamap' association-options='associationOptions'>" +
            "</lookup-modal>"
            );
            $compile(element.contents())(scope);
        },

    }
}),

app.directive('lookupModal', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/modules/lookup/templates/lookupModal.html'),
        scope: {
            lookupAssociationsCode: '=',
            lookupAssociationsDescription: '=',
            associationOptions: '=',
            lookupObj: '=',
            schema: '=',
            datamap: '='
        },

        controller: function ($injector, $scope, $http, $element, searchService, i18NService, associationService,
                              formatService, expressionService, focusService) {

            $scope.lookupModalSearch = function (pageNumber) {
                focusService.resetFocusToCurrent($scope.schema, $scope.lookupObj.fieldMetadata.attribute);

                associationService.getAssociationOptions($scope.schema, $scope.datamap, $scope.lookupObj, pageNumber, $scope.searchObj).success(function(data) {
                    var result = data.resultObject;
                    $scope.populateModal(result);
                });
            };

            $scope.populateModal = function (resultData) {
                for (var association in resultData) {
                    if (resultData.hasOwnProperty(association)) {
                        if ($scope.lookupObj.fieldMetadata.associationKey == association) {
                            var associationResult = resultData[association];
                            $scope.lookupObj.options = associationResult.associationData;
                            $scope.lookupObj.schema = associationResult.associationSchemaDefinition;
                            var modalPaginationData = $scope.lookupObj.modalPaginationData;

                            modalPaginationData.pageCount = associationResult.pageCount;
                            modalPaginationData.pageNumber = associationResult.pageNumber;
                            modalPaginationData.pageSize = associationResult.pageSize;
                            modalPaginationData.totalCount = associationResult.totalCount;
                            modalPaginationData.selectedPage = associationResult.pageNumber;
                            //TODO: this should come from the server side
                            modalPaginationData.paginationOptions = [10, 30, 100];
                        }
                    }
                }
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
                $scope.lookupAssociationsCode[fieldMetadata.attribute] = option.value;
                $scope.lookupAssociationsDescription[fieldMetadata.attribute] = option.label;
                associationService.updateUnderlyingAssociationObject(fieldMetadata, option, $scope);
                $element.modal('hide');
            };

            $element.on('hide.bs.modal', function (e) {

                $('body').removeClass('modal-open');
                $('.modal-backdrop').remove();
                if ($scope.lookupObj) {
                    $scope.lookupObj.description = null;
                }

                if ($scope.lookupObj.fieldMetadata == null) {
                    return;
                }

                var fieldMetadata = $scope.lookupObj.fieldMetadata;
                if (!$scope.lookupAssociationsCode) {
                    return;
                }

                if ($scope.selectedOption == null &&
                    $scope.datamap[fieldMetadata.attribute] != $scope.lookupAssociationsCode[fieldMetadata.attribute]) {
                    if ($scope.modalCanceled == true) {
                        $scope.lookupAssociationsCode[fieldMetadata.attribute] = $scope.datamap[fieldMetadata.attribute];
                    } else {
                        $scope.$apply(function () {
                            $scope.lookupAssociationsCode[fieldMetadata.attribute] = $scope.datamap[fieldMetadata.attribute];
                        });
                    }
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
                $scope.populateModal($scope.lookupObj.initialResults);
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