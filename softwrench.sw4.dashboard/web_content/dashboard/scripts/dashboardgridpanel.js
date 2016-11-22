(function (angular) {
    "use strict";


angular.module('sw_layout')
   .directive('dashboardgridpanel',
    ["$timeout", "$log", "$rootScope", "contextService", "searchService", 
        function ($timeout, $log, $rootScope, contextService, searchService) {

    function doInit(scope) {
        scope.schema = {
            applicationName: scope.paneldatasource.panel.application,
            schemaId: scope.paneldatasource.panel.schemaRef,
            stereotype: 'list',
            properties: {},
            commandSchema:{}
        };
        scope.application = scope.paneldatasource.panel.application;
        // TODO: We'll need to define an unique name for it. 

        const panelAlias = scope.paneldatasource.panel.alias;
        if (panelAlias) {
            scope.metadataid = "dashboard:" + panelAlias;
        }
        
        scope.fieldstodisplay = scope.paneldatasource.panel['appFields'];
        scope.datamap = {};
    }

    return {
        restrict: 'E',
        templateUrl: contextService.getResourceUrl('/Content/Shared/dashboard/templates/dashboardgridpanel.html'),
        scope: {
            paneldatasource: '=',
            dashboardid:'='
        },

        controller: ["$scope", "$compile", "$element", function ($scope, $compile, $element) {
            function init() {
                doInit($scope);
                searchService.refreshGrid({}, null, {
                    panelid: $scope.paneldatasource.id,
                    metadataid: $scope.metadataid,
                    pageSize: $scope.paneldatasource.panel['limit'],
                    searchSort: $scope.paneldatasource.panel['defaultSortField'],
                    fieldstodisplay: $scope.paneldatasource.panel['appFields'],
                    multiSort: searchService.parseMultiSort($scope.paneldatasource.panel['defaultSortField'])
                });
            }

            init();

            $scope.$watch("paneldatasource.panel", function(newValue, oldValue) {
                if (newValue === oldValue || !newValue) return;
                init();
               
            }, true);

            $scope.getMetadataId = function(paneldatasource) {
                return "dashboard:" + paneldatasource.panel.alias;
            }

        }],

        link: function (scope, element, attrs) {
            //building a schema object representation to propagate to crud_list.html
            doInit(scope);

            function setPaginationWidth() {
                var pagination = $('.affix-pagination-panel:visible', element);
                var grid = $('#listgrid', element);

                if (!pagination || !grid) {
                    return;
                }

                pagination.width(grid[0].clientWidth);
            }

            $(window).resize(window.debounce(setPaginationWidth, 250));

            //update pagination width
            scope.$watch(
                function () {
                    return element[0].innerHTML.length;
                },
                function (newValue, oldValue) {
                    if (newValue !== oldValue) {
                        setPaginationWidth();
                    }
                }
            );
        }
    };
}]);

})(angular);