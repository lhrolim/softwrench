app.directive('pagination', function (contextService) {
    "ngInject";
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/pagination.html'),
        scope: {
            renderfn: "&",
            paginationData: '=',
            showactions: "@",
            schemaId: '@',
            applicationName: '@',
            mode: '@',
            disablePrint: '@',
            disableExport: '@'
        },

        controller: function ($scope,
            $http,
            $rootScope,
            $timeout,
            printService,
            searchService,
            i18NService,
            redirectService,
            contextService,
            excelService) {

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
                excelService.exporttoexcel($scope.$parent.schema, $scope.$parent.searchData, $scope.$parent.searchSort, $scope.$parent.searchOperator, $scope.paginationData);
            };

         

       

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