var app = angular.module('sw_layout');


app.directive('inlineCompositionListWrapper', function ($compile) {

    return {
        restrict: 'E',
        replace: true,
        template: "<div></div>",
        scope: {
            iscollection: '=',
            parentdata: '=',
            metadata: '=',
            mode: '@'
        },

        link: function (scope, element, attrs) {

            var doLoad = function() {
                scope.compositionschemadefinition = scope.metadata.schema;
                scope.compositiondata = scope.parentdata[scope.metadata.relationship];
                element.append(
                    "<inline-composition-list parentdata='parentdata'" +
                    "metadata='metadata' iscollection='iscollection' compositionschemadefinition='compositionschemadefinition' compositiondata='compositiondata' mode='mode'/>"
                );
                $compile(element.contents())(scope);
                scope.loaded = true;
            };


            doLoad();

            scope.$on("sw_lazyloadtab", function (event, tabid) {
                if (scope.tabid == tabid && !scope.loaded) {
                    doLoad();
                }
            });

        }
    };
});


app.directive('inlineCompositionList', function (contextService) {

    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/inline_composition_list.html'),
        scope: {
            parentdata: '=',
            metadata: '=',
            iscollection: '=',
            compositionschemadefinition: '=',
            compositiondata: '=',
            mode: '@'
        },

        controller: function ($scope, $filter, $http, $element, $rootScope, tabsService) {

            $scope.contextPath = function (path) {
                return url(path);
            };
        }
    };
});