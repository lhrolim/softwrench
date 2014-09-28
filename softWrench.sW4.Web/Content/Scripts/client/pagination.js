app.directive('pagination', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/pagination.html'),
        scope: {
            renderfn: "&",
            paginationData: '=',
            schemaId: '@',
            applicationName: '@',
            mode: '@',
            disablePrint: '@',
            disableExport: '@'
        },

        controller: function ($scope, $rootScope, $timeout, printService, searchService, i18NService, contextService) {

            $scope.contextPath = function (path) {
                return url(path);
            };

            $scope.isHapag = function () {
                return $rootScope.clientName == "hapag";
            };

            $scope.i18N = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.previousPage = function () {
                var pageNumber = $scope.paginationData.pageNumber;
                if (pageNumber > 1) {
                    $scope.selectPage(pageNumber - 1);
                }
            };

            $scope.nextPage = function () {
                var pageNumber = $scope.paginationData.pageNumber;
                if (pageNumber < $scope.paginationData.pageCount) {
                    $scope.selectPage(pageNumber + 1);
                }
            };

            $scope.exportToExcel = function (schemaId) {
                printPageSize = null;
                var parameters = {};
                parameters.key = {};
                parameters.key.schemaId = $scope.$parent.schema.schemaId;
                parameters.key.mode = $scope.$parent.schema.mode;
                parameters.key.platform = "web";
                parameters.application = $scope.$parent.schema.applicationName;
                if ((nullOrUndef(schemaId))) {
                    if ($scope.isHapag() && parameters.application == "asset") {
                        showModalExportToExcel(parameters);
                        return null;
                    }
                } else {
                    parameters.key.schemaId = schemaId;
                }
                var searchDTO = searchService.buildSearchDTO($scope.$parent.searchData, $scope.$parent.searchSort, $scope.$parent.searchOperator, $scope.paginationData.filterFixedWhereClause);
                var currentModule = contextService.currentModule();
                if (currentModule != null) {
                    parameters.module = currentModule;
                }
                searchDTO.pageNumber = $scope.paginationData.pageNumber;
                searchDTO.totalCount = $scope.paginationData.totalCount;
                searchDTO.pageSize = $scope.paginationData.pageSize;

                parameters.searchDTO = searchDTO;
                var parameterstoexcel = {
                    application: parameters.application,
                    key: parameters.key,
                    searchDTO: parameters.searchDTO,
                    module: parameters.module
                };

                var urlToInvoke = redirectService.getActionUrl('ExportApi', 'SetExcelFile', parameterstoexcel);
                $http.get(urlToInvoke).
                success(function (data, status, headers, config) {
                    var parameterstogetfile = {
                        fileName: getFileName(parameterstoexcel.application, parameterstoexcel.key.schemaId) + 'Export'
                    };
                    window.location = removeEncoding(url("/Application/ExportToExcel" + "?" + $.param(parameterstogetfile)));
                }).
                error(function (data, status, headers, config) {
                    return null;
                });
            };

            function getFileName(application, schemaId) {
                if (application == 'asset') {
                    var assetname = '';
                    if (schemaId == 'categories') {
                        assetname = 'Categories';
                    } else if (schemaId == 'exportallthecolumns') {
                        assetname = 'List';
                    }
                    return 'Asset' + assetname;
                } else {
                    return capitaliseFirstLetter(application);
                }
            }

            function showModalExportToExcel(parameters) {
                var modalTitle = $scope.i18N('_exportotoexcel.export', 'Export') + " " + parameters.application +
                " " + $scope.i18N('_exportotoexcel.toexcel', 'to excel');
                var selectText = $scope.i18N('_exportotoexcel.selectexportmode', 'Select export mode');
                var exportModeGridOnlyText = $scope.i18N('_exportotoexcel.gridonly', 'Grid only');
                var exportModeAllTheColumnsText = $scope.i18N('_exportotoexcel.allthecolumns', 'Asset List');
                var exportModeCategoriesText = $scope.i18N('_exportotoexcel.categories', 'Categories');

                bootbox.dialog({
                    message: "<form id='infos' action=''>" +
                              "<label class='control-label'>" + selectText + ":</label><br>" +
                              "<label class='control-label' for='gridonlyid'>" +
                              "<input type='radio' name='exportMode' id='gridonlyid' value='list' checked='checked' /> "
                              + exportModeGridOnlyText +
                              "</label><br>" +
                              "<label class='control-label' for='allthecolumnsid'>" +
                              "<input type='radio' name='exportMode' id='allthecolumnsid' value='exportallthecolumns' /> "
                              + exportModeAllTheColumnsText +
                              "</label><br>" +
                              "<label class='control-label' for='categoriesid'>" +
                              "<input type='radio' name='exportMode' id='categoriesid' value='categories' /> "
                              + exportModeCategoriesText +
                              "</label>" +
                              "</form>",
                    title: modalTitle,
                    buttons: {
                        cancel: {
                            label: $scope.i18N('_exportotoexcel.cancel', 'Cancel'),
                            className: "btn btn-default",
                            callback: function () {
                                return null;
                            }
                        },
                        main: {
                            label: $scope.i18N('_exportotoexcel.export', 'Export'),
                            className: "btn-primary",
                            callback: function (result) {
                                if (result) {
                                    var exportModeSelected = $('input[name=exportMode]:checked', '#infos').val();
                                    if (exportModeSelected != parameters.key.schemaId) {
                                        $scope.exportToExcel(exportModeSelected);
                                    }
                                    else {
                                        $scope.exportToExcel(parameters.key.schemaId);
                                    }
                                }
                            }
                        }
                    },
                    className: "hapag-modal-exporttoexcel"
                });
            }

            var printPageSize;

            $scope.exportToPrint = function () {
                printService.printList($scope.paginationData.totalCount,
                    $scope.paginationData.pageSize, $scope.renderfn, $scope.$parent.schema);
            };

            $scope.selectPage = function (page) {
                printPageSize = null;
                $scope.renderfn({ pageNumber: page });
            };

            if ($scope.disablePrint === undefined) {
                $scope.disablePrint = false;
            }
            if ($scope.disableExport === undefined) {
                $scope.disableExport = false;
            }

            $scope.adjustMargin = function (language) {
                if (!$scope.isHapag()) {
                    return;
                }
                var marginLeft = '30px';
                if (language.toLowerCase() == 'en') {
                    marginLeft = '60px';
                }
                $('.pagination-pager').css({ 'margin-left': marginLeft });
            }

            $scope.adjustMargin(i18NService.getCurrentLanguage());
        }
    };
});