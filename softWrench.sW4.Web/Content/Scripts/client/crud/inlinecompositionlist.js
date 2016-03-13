(function (angular) {
    "use strict";

var app = angular.module('sw_layout');

app.directive('inlineCompositionListWrapper', function ($compile) {
    "ngInject";

    return {
        restrict: 'E',
        replace: true,
        template: "<div></div>",
        scope: {
            iscollection: '=',
            parentdata: '=',
            metadata: '=',
            parentschema:'=',
            mode: '@',
            ismodal: '@'
        },

        link: function (scope, element, attrs) {

            var doLoad = function() {
                scope.compositionschemadefinition = scope.metadata.schema;
                scope.compositiondata = scope.parentdata[scope.metadata.relationship];
                element.append(
                    "<inline-composition-list parentdata='parentdata' parentschema='parentschema'" +
                    "metadata='metadata' iscollection='iscollection' compositionschemadefinition='compositionschemadefinition' compositiondata='compositiondata' mode='mode' ismodal='ismodal'/>"
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
            compositiondata: '=',
            parentschema: '=',
            mode: '=',
            ismodal: '='
        },

        controller: function ($scope) {
            $scope.contextPath = function (path) {
                return url(path);
            };
        }
    };
});

})(angular);