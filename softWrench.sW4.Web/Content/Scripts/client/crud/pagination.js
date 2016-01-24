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

            $scope.getDirectionClass = function (direction) {
                if ($scope.paginationData) {
                    var currentPage = $scope.paginationData.pageNumber;
                    var pageCount = $scope.paginationData.pageCount;
                    var classString = 'disable';

                    switch (direction) {
                        case 'prev':
                            if (currentPage == 1) {
                                return classString;
                            }

                            break;
                        case 'next':
                            if (currentPage == pageCount) {
                                return classString;
                            }
                            break;
                    }
                }
            }

            $scope.getTooltip = function (direction) {
                if ($scope.paginationData) {
                    var currentPage = $scope.paginationData.pageNumber;
                    var pageCount = $scope.paginationData.pageCount;
                    var tooltip = 'Go to {0} of ' + pageCount;

                    switch (direction) {
                        case 'prev':
                            if (currentPage > 1) {
                                return tooltip.format(currentPage - 1);
                            }

                            break;
                        case 'next':
                            if (currentPage < pageCount) {
                                return tooltip.format(currentPage + 1);
                            }
                            break;
                    }
                }
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

app.directive('paginationPages', ['contextService', '$log', function (contextService, $log) {

    var log = $log.getInstance('sw4.pagination.pages');

    return {
        restrict: 'E',
        replace: false,
        templateUrl: contextService.getResourceUrl('/Content/Templates/pagination-pages.html'),
        scope: {
            renderfn: "&",
            paginationData: '=',
        },

        controller: ['$scope', function ($scope) {
            log.debug($scope.paginationData);

            $scope.changePage = function (page) {
                log.debug($scope.paginationData);
                $scope.$parent.selectPage(page);
            };

            $scope.getLastPage = function () {
                return $scope.paginationData.pageCount
            }

            $scope.getPageArray = function () {
                if ($scope.paginationData) {
                    var pageArray = new Array($scope.paginationData.pageCount)
                    pageArray = pageArray.slice(0, -2);

                    return pageArray;
                }
            }

            $scope.getPageClass = function (index) {
                if ($scope.paginationData) {
                    if (index + 2 == $scope.paginationData.pageNumber) {
                        return 'current';
                    }
                }
            }

            $scope.getPageRange = function (currentPage) {
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

            $scope.showEllipsis = function (location) {
                if ($scope.paginationData) {
                    var currentPage = $scope.paginationData.pageNumber;
                    var pageRange = $scope.getPageRange(currentPage);

                    switch (location) {
                        case 'start':
                            return !(1 >= pageRange.min);
                            break;
                        case 'end':
                            return !($scope.paginationData.pageCount <= pageRange.max);
                            break;
                    }

                    return true;
                }
            }

            $scope.showLastPage = function () {
                if ($scope.paginationData) {
                    return $scope.paginationData.pageCount > 1;
                }
            }

            $scope.showPage = function (index) {
                var pageNumber = index + 2;
                var pageRange = $scope.getPageRange($scope.paginationData.pageNumber);

                return pageNumber >= pageRange.min && pageNumber <= pageRange.max;
            }

            //init directive
            $scope.pagesToShow = 4;
        }]
    };
}]);

})(app);