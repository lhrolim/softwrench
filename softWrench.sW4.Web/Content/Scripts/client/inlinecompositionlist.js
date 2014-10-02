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
        },

        link: function (scope, element, attrs) {
            element.append(
                "<inline-composition-list compositionschemadefinition='compositionschemadefinition'" +
                         "compositiondata='compositiondata'" +
                         "iscollection='iscollection' />"
            );
            $compile(element.contents())(scope);
        }
    };
});


app.directive('inlineCompositionList', function (contextService, commandService) {

    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/inline_composition_list.html'),
        scope: {
            compositionschemadefinition: '=',
            compositiondata: '=',
            iscollection: '=',
        },

        controller: function ($scope, $filter, $http, $element, $rootScope, tabsService) {

            $scope.contextPath = function (path) {
                return url(path);
            };

            $scope.handleItemClick = function (item, schema) {
                if (!nullOrUndef(schema.rendererParameters) && !nullOrEmpty(schema.rendererParameters.onClickService)) {
                    commandService.executeClickCustomCommand(schema.rendererParameters.onClickService, item, schema);
                }
            }

        }
    };
});