app.directive('pagination', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/pagination.html'),
        scope: {
            renderfn: "&",
            paginationData: '=',
            searchData: '=',
            searchOperator: '=',
            schemaId: '@',
            applicationName: '@',
            mode: '@',
            disablePrint: '@',
            disableExport: '@'
        },

        controller: function ($scope, $http, $rootScope, $timeout, printService,
            searchService, i18NService, redirectService, fileService, userPreferenceService, alertService) {

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

            $scope.selectPage = function (page) {
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

            $scope.nonSharedFilters = function () {
                return userPreferenceService.loadUserNonSharedFilters($scope.applicationName, $scope.schemaId);
            }

            $scope.sharedFilters = function () {
                return userPreferenceService.loadUserSharedFilters($scope.applicationName, $scope.schemaId);
            }

            $scope.hasSharedFilter = function () {
                return $scope.sharedFilters().length > 0;
            }

            $scope.hasFilter = function () {
                return userPreferenceService.hasFilter($scope.applicationName, $scope.schemaId);
            }

            $scope.$on("sw_redirectapplicationsuccess", function (event) {
//                $scope.searchData = {};
            });

            $scope.adjustMargin(i18NService.getCurrentLanguage());
        }
    };
});