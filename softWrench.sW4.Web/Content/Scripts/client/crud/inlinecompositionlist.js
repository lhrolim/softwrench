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
                // TODO: Find a better fix for this other than checking if the parent data is empty.
                //       The configuration screen does not have the parent data when loaded so we need to instantiate it.
                //       The labor reporting on the other had already has the parent data with the #laborlist_ on it which is not present on this parent data.
                if (jQuery.isEmptyObject(scope.parentdata)) {
                    scope.parentdata = { id:scope.parentdata.id, fields: scope.parentdata };
                }
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