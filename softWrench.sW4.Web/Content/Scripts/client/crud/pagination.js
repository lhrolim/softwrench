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
            disableExport: '@',
            panelid: '='
        },

        controller: ["$scope", "$http", "$rootScope", "$timeout", "printService", "searchService", "i18NService", "crudContextHolderService", function ($scope, $http, $rootScope, $timeout, printService, searchService, i18NService, crudContextHolderService) {
            $scope.showPagination = true;

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

            $scope.shouldShowPagination = function() {
                return !crudContextHolderService.getSelectionModel($scope.panelid).showOnlySelected;
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

app.directive('paginationPages', ['contextService', function (contextService) {
    return {
        restrict: 'E',
        replace: false,
        templateUrl: contextService.getResourceUrl('/Content/Templates/pagination-pages.html'),
        scope: {
            renderfn: "&",
            paginationData: '=',
            //searchData: '=',
            //searchOperator: '=',
            //schema: '=',
            //applicationName: '@',
            //mode: '@',
            //disablePrint: '@',
            //disableExport: '@',
            //panelid: '='
        },

        controller: ['$scope', '$log', function ($scope, $log) {
            var log = $log.getInstance('sw4.pagination.pages');
            log.debug($scope.paginationData);

            $scope.getPageArray = function () {
                return new Array($scope.paginationData.pageCount);
            }

            $scope.getPageClass = function (index) {
                if (index + 1 == $scope.paginationData.pageNumber) {
                    return 'current';
                }
            }

            $scope.getPageRange = function (pageNumber, currentPage) {
                var pageRange = {
                    min: 1,
                    max: $scope.paginationData.pageCount
                }

                var rangeMargin = $scope.pagesToShow / 2;

                //the current page is first or near the begining 
                if (currentPage == 1 || currentPage - rangeMargin <= 2) {
                    pageRange.max = 1 + $scope.pagesToShow;
                    return pageRange;
                }

                //the current page is last or near the end
                if (currentPage == $scope.paginationData.pageCount || $scope.paginationData.pageCount - currentPage - rangeMargin < 2) {
                    pageRange.min = $scope.paginationData.pageCount - $scope.pagesToShow;
                    return pageRange;
                }

                //set the page range
                pageRange.min = currentPage - rangeMargin;
                pageRange.max = currentPage + rangeMargin;

                return pageRange;
            }

            $scope.showPage = function (index) {
                var pageNumber = index + 1;
                var pageRange = $scope.getPageRange(pageNumber, $scope.paginationData.pageNumber);


                if (pageNumber >= pageRange.min && pageNumber <= pageRange.max) {
                    return true;
                }
            }

            //init directive
            $scope.pagesToShow = 4;
        }]

        //link: function (scope, element, attrs) {
        //    scope.setSimpleLayout(attrs.hasOwnProperty("paginationSimpleLayout"));
        //}
    };
}]);

})(app);