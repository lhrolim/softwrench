var app = angular.module('sw_layout');


app.directive('inlineCompositionListWrapper', function ($compile) {
    "ngInject";
    return {
        restrict: 'E',
        replace: true,
        template: "<div></div>",
        scope: {
            parentdata: '=',
            metadata: '=',
            iscollection : '='
        },

        link: function (scope, element, attrs) {

            var doLoad = function () {
                scope.compositionschemadefinition = scope.metadata.schema;
                scope.compositiondata = scope.parentdata[scope.metadata.relationship];
                element.append(
                    "<inline-composition-list parentdata='parentdata'" +
                             "metadata='metadata' iscollection='iscollection' compositionschemadefinition='compositionschemadefinition' compositiondata='compositiondata'/>"
                );
                $compile(element.contents())(scope);
                scope.loaded = true;
            }


            doLoad();

            scope.$on("sw_lazyloadtab", function (event, tabid) {
                if (scope.tabid == tabid && !scope.loaded) {
                    doLoad();
                }
            });

        }
    };
});


app.directive('inlineCompositionList', function (contextService, commandService) {
    "ngInject";
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/inline_composition_list.html'),
        scope: {
            parentdata: '=',
            metadata: '=',
            iscollection: '=',
            compositionschemadefinition: '=',
            compositiondata:'=',
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