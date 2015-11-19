(function (app) {
    "use strict";

app.directive('pagination', ["contextService", function (contextService) {
    return {
        restrict: 'E',
        replace: false,
        templateUrl: contextService.getResourceUrl('/Content/Templates/pagination.html'),
        scope: {
            renderfn: "&",
            paginationData: '=',
            searchData: '=',
            searchOperator: '=',
            schema:'=',
            applicationName: '@',
            mode: '@',
            disablePrint: '@',
            disableExport: '@'
        },

        controller: ["$scope", "$http", "$rootScope", "$timeout", "printService", "searchService", "i18NService", function ($scope, $http, $rootScope, $timeout, printService, searchService, i18NService) {
            $scope.layout = {
                simple: false
            };

            $scope.setSimpleLayout = function(value) {
                $scope.layout.simple = value;
            };

            $scope.isPaginationEnabled = function() {
                return $scope.schema == null || "true" !== $scope.schema.properties['list.disablepagination'];
            };

            $scope.contextPath = function (path) {
                return url(path);
            };

            $scope.isHapag = function () {
                return $rootScope.clientName === "hapag";
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
                // Set the scroll position to the top of the new page
                contextService.insertIntoContext('scrollto', { 'applicationName': $scope.applicationName, 'scrollTop': 0 });
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
                if (language.toLowerCase() === 'en') {
                    marginLeft = '60px';
                }
                $('.pagination-pager').css({ 'margin-left': marginLeft });
            }

            //$scope.$on("sw_redirectapplicationsuccess", function (event) {
            //    // $scope.searchData = {};
            //});

            function init() {
                // 'booleanizing' the values (compensates for undefined-like/null-like values)
                $scope.disablePrint = !!$scope.disablePrint;
                $scope.disableExport = !!$scope.disableExport; 

                $scope.adjustMargin(i18NService.getCurrentLanguage());
            }

            init();

        }],

        link: function(scope, element, attrs) {
            scope.setSimpleLayout(attrs.hasOwnProperty("paginationSimpleLayout"));
        }
    };
}]);

})(app);