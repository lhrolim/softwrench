var app = angular.module('sw_layout');

app.directive('lookupModalWrapper', function ($compile) {
    return {
        restrict: "E",
        replace: true,
        scope: {
            lookupAssociationsCode: '=',
            lookupAssociationsDescription: '=',
            associationOptions: '=',
            lookupObj: '=',
            schema: '=',
            datamap: '='
        },
        template: "<div></div>",
        link: function (scope, element, attrs) {
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
        templateUrl: contextService.getResourceUrl('/Content/Templates/lookupModal.html'),
        scope: {
            lookupAssociationsCode: '=',
            lookupAssociationsDescription: '=',
            associationOptions: '=',
            lookupObj: '=',
            schema: '=',
            datamap: '='
        },

        controller: function ($injector, $scope, $http, $element, searchService, i18NService, associationService,
                              formatService, expressionService) {

            $scope.lookupModalSearch = function (pageNumber) {
                var schema = $scope.schema;
                var fields = $scope.datamap;
                var lookupObj = $scope.lookupObj;

                var parameters = {};
                parameters.application = schema.applicationName;
                parameters.key = {};
                parameters.key.schemaId = schema.schemaId;
                parameters.key.mode = schema.mode;
                parameters.key.platform = platform();
                parameters.associationFieldName = lookupObj.fieldMetadata.associationKey;

                var lookupApplication = lookupObj.fieldMetadata.schema.rendererParameters["application"];
                var lookupSchemaId = lookupObj.fieldMetadata.schema.rendererParameters["schemaId"];
                if (lookupApplication != null && lookupSchemaId != null) {
                    parameters.associationApplication = lookupApplication;
                    parameters.associationKey = {};
                    parameters.associationKey.schemaId = lookupSchemaId;
                    parameters.associationKey.platform = platform();
                }

                var totalCount = 0;
                var pageSize = 30;
                if ($scope.modalPaginationData != null) {
                    totalCount = $scope.modalPaginationData.totalCount;
                    pageSize = $scope.modalPaginationData.pageSize;
                }
                if (pageNumber === undefined) {
                    pageNumber = 1;
                }

                if (lookupObj.schema != null) {
                    var defaultLookupSearchOperator = searchService.getSearchOperationById("CONTAINS");
                    var searchValues = $scope.searchObj;
                    var searchOperators = {}
                    for (var field in searchValues) {
                        searchOperators[field] = defaultLookupSearchOperator;
                    }
                    parameters.hasClientSearch = true;
                    parameters.SearchDTO = searchService.buildSearchDTO(searchValues, {}, searchOperators);
                    parameters.SearchDTO.pageNumber = pageNumber;
                    parameters.SearchDTO.totalCount = totalCount;
                    parameters.SearchDTO.pageSize = pageSize;
                } else {
                    parameters.valueSearchString = lookupObj.code == null ? "" : lookupObj.code;
                    parameters.labelSearchString = lookupObj.description == null ? "" : lookupObj.description;
                    parameters.hasClientSearch = true;
                    parameters.SearchDTO = {
                        pageNumber: pageNumber,
                        totalCount: totalCount,
                        pageSize: pageSize
                    };
                }

                var urlToUse = url("/api/generic/ExtendedData/UpdateAssociation?" + $.param(parameters));
                var jsonString = angular.toJson(fields);
                $http.post(urlToUse, jsonString).success(function (data) {
                    var result = data.resultObject;
                    for (var association in result) {
                        if (lookupObj.fieldMetadata.associationKey == association) {
                            var associationResult = result[association];
                            lookupObj.options = associationResult.associationData;
                            lookupObj.schema = associationResult.associationSchemaDefinition;

                            $scope.modalPaginationData = {};
                            $scope.modalPaginationData.pageCount = associationResult.pageCount;
                            $scope.modalPaginationData.pageNumber = associationResult.pageNumber;
                            $scope.modalPaginationData.pageSize = associationResult.pageSize;
                            $scope.modalPaginationData.totalCount = associationResult.totalCount;
                            $scope.modalPaginationData.selectedPage = associationResult.pageNumber;
                            //TODO: this should come from the server side
                            $scope.modalPaginationData.paginationOptions = [10, 30, 100];
                        }
                    }
                }).error(function data() {
                });
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

                $scope.datamap[fieldMetadata.target] = option.value;
                $scope.lookupAssociationsCode[fieldMetadata.attribute] = option.value;
                $scope.lookupAssociationsDescription[fieldMetadata.attribute] = option.label;

                associationService.updateUnderlyingAssociationObject(fieldMetadata, option, $scope);


                $element.modal('hide');
            };

            $element.on('hide.bs.modal', function (e) {

                $('body').removeClass('modal-open');
                $('.modal-backdrop').remove();

                if ($scope.lookupObj == null) {
                    return;
                }

                var fieldMetadata = $scope.lookupObj.fieldMetadata;
                if ($scope.datamap != null && ($scope.datamap[fieldMetadata.target] == null || $scope.datamap[fieldMetadata.target] == " ")) {
                    $scope.$apply(function () {
                        $scope.lookupAssociationsCode[fieldMetadata.attribute] = null;
                        $scope.lookupAssociationsDescription[fieldMetadata.attribute] = null;
                    });
                }

            });

            $scope.hideLookupModal = function() {
                var modals = $('[data-class="lookupModal"]');
                modals.modal('hide');
            };

            $element.on('shown.bs.modal', function (e) {
                $scope.searchObj = {};
                if ($scope.lookupObj != undefined) {
                    $scope.lookupModalSearch();
                }
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