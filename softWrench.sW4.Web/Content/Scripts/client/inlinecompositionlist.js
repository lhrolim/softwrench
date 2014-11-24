var app = angular.module('sw_layout');


app.directive('inlineCompositionListWrapper', function ($compile) {

    return {
        restrict: 'E',
        replace: true,
        template: "<div></div>",
        scope: {
            compositionschemadefinition: '=',
            compositiondata: '=',
            iscollection: '=',
            mode: '@',
            previousdata: '=',
            previousschema: '='
        },

        link: function (scope, element, attrs) {
            element.append(
                "<inline-composition-list compositionschemadefinition='compositionschemadefinition'" +
                         "compositiondata='compositiondata'" +
                         "mode='mode'" +
                         "iscollection='iscollection'" +
                         "previousdata='previousdata'" +
                         "previousschema='previousschema' />"
            );
            $compile(element.contents())(scope);
        }
    };
});


app.directive('inlineCompositionList', function (contextService) {

    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/inline_composition_list.html'),
        scope: {
            compositionschemadefinition: '=',
            compositiondata: '=',
            iscollection: '=',
            mode: '@',
            previousdata: '=',
            previousschema: '='
        },

        controller: function ($scope, $filter, $http, $element, $rootScope, tabsService) {

            $scope.contextPath = function (path) {
                return url(path);
            };
        }
    };
});