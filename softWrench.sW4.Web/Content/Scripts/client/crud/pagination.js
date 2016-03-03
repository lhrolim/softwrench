(function (app) {
    "use strict";

app.directive('pagination', ['contextService', '$log', function (contextService, $log) {
    var log = $log.getInstance('sw4.pagination');

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

        controller: ["$scope", "$http", "$rootScope", "$timeout", "printService", "searchService", "i18NService", "crudContextHolderService", "sidePanelService", function ($scope, $http, $rootScope, $timeout, printService, searchService, i18NService, crudContextHolderService, sidePanelService) {
            log.debug($scope.paginationData);

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
                //exit if no data
                if (!$scope.paginationData) {
                    return '';
                }

                var classString = '';
                var currentPage = $scope.paginationData.pageNumber;
                var pageCount = $scope.paginationData.pageCount;
                
                //disable the prev/next arrow if we are on the first/last page
                if (direction == 'prev' && currentPage == 1 || direction == 'next' && currentPage == pageCount) {
                    classString = 'disable';
                }

                return classString;
            }

            $scope.getTooltip = function (direction, simpleLayout) {
                //use the direction as the default tooltip text
                var tooltip = direction.replace(/\w\S*/g, function (txt) {
                    return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
                });

                //if not pagination return the default tooltip
                if (!$scope.paginationData) {
                    return tooltip;
                }

                var currentPage = $scope.paginationData.pageNumber;
                var pageCount = $scope.paginationData.pageCount;

                //if we are on the first or last page, remove prev/next tooltips
                if (direction == 'prev' && currentPage == 1 || direction == 'next' && currentPage == pageCount) {
                    return '';
                }

                var tooltipFormat = 'Go to {0} of ' + pageCount;

                //display the current and total pages for the simple pagination
                if (simpleLayout) {
                    if (direction == 'prev' && currentPage > 1) {
                        tooltip = tooltipFormat.format(currentPage - 1);
                    } else if (direction == 'next' && currentPage < pageCount) {
                        tooltip = tooltipFormat.format(currentPage + 1);
                    }
                }          

                return tooltip;
            }

            // adds a padding right to not be behind side panels handles
            $scope.sidePanelStyle = function() {
                var style = {};
                if ("#modal" !== $scope.panelid && sidePanelService.getTotalHandlesWidth() > 210) {
                    style["padding-right"] = "16px";
                }
                return style;
            }

            function init() {
                // 'booleanizing' the values (compensates for undefined-like/null-like values)
                $scope.disablePrint = !!$scope.disablePrint;
                $scope.disableExport = !!$scope.disableExport; 

                $scope.adjustMargin(i18NService.getCurrentLanguage());
            }

            init();

        }],

        link: function(scope, element, attrs) {
            scope.setSimpleLayout(attrs.hasOwnProperty('paginationSimpleLayout'));
        }
    };
}]);

app.directive('paginationPages', ['contextService', '$log', '$timeout', function (contextService, $log, $timeout) {
    var log = $log.getInstance('sw4.pagination.pages');

    return {
        restrict: 'E',
        replace: false,
        templateUrl: contextService.getResourceUrl('/Content/Templates/pagination-pages.html'),
        scope: {
            paginationData: '=',
        },

        controller: ['$scope', function ($scope) {
            log.debug($scope, $scope.paginationData);

            $scope.changePage = function (page) {
                log.debug($scope.paginationData);
                $scope.$parent.selectPage(page);

                //update header/footer layout
                $timeout(function () {
                    $(window).trigger('resize');
                }, false);
            };

            $scope.getLastPage = function () {
                return $scope.paginationData.pageCount;
            }

            $scope.getPageArray = function () {
                //fix undefined error in modal
                if (!$scope.paginationData) {
                    return null;
                }

                var pageArray = new Array($scope.paginationData.pageCount);
                    pageArray = pageArray.slice(0, -2);

                return pageArray;
            }

            $scope.getPageClass = function (index) {
                //fix undefined error in modal
                if (!$scope.paginationData) {
                    return '';
                }

                var className = '';

                if (index + 2 == $scope.paginationData.pageNumber) {
                    className = 'current';
                }

                return className;
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
                var show = false;

                //fix undefined error in modal
                if (!$scope.paginationData) {
                    return null;
                }

                var currentPage = $scope.paginationData.pageNumber;
                var pageRange = $scope.getPageRange(currentPage);

                //show ellipsis if page is outside of page range
                if (location == 'start') {
                    show = !(1 >= pageRange.min);
                } else if (location == 'end') {
                    show = !($scope.paginationData.pageCount <= pageRange.max);
                }

                return show;
            }

            $scope.showLastPage = function () {
                //fix undefined error in modal
                if (!$scope.paginationData) {
                    return null;
                }

                return $scope.paginationData.pageCount > 1;
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