(function (angular) {
    "use strict";

    angular.module('sw_layout').directive('bodyrendered', function ($timeout, $log, menuService) {
        "ngInject";
        return {
            restrict: 'A',
            link: function (scope, element, attr) {
                if (scope.schema.mode !== 'output' && scope.isSelectEnabled) {
                    element.data('selectenabled', scope.isSelectEnabled(scope.fieldMetadata));
                }
                if (scope.$last === true) {
                    $timeout(function () {
                        const parentElementId = scope.elementid;
                        $log.getInstance('application_dir#bodyrendered').debug('sw_body_rendered will get dispatched');
                        menuService.adjustHeight();
                        scope.$emit(JavascriptEventConstants.BodyRendered, parentElementId);
                    });
                }
            }
        };
    });
  

})(angular);