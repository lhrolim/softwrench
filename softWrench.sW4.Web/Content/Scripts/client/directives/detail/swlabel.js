(function (angular) {
    "use strict";

angular.module('sw_layout')
    .directive('swlabel', function (i18NService, contextService) {
    "ngInject";

    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('Content/templates/swlabel.html'),
        scope: {
            i18nKey: '@',
            i18nHelpKey: '@',
            text: '@',
            helptext: '@',
            required: '@',
            classes: '@',
            helpposition:'@'
        },
        
        link: function (scope, element, attrs) {
            scope.classes = scope.classes || "";
            scope.classtouse = " control-label " + scope.classes;

            scope.helppositiontouse = scope.helpposition || "left";

            scope.i18NText = function () {
                return i18NService.get18nValue(scope.i18nKey, scope.text);
            }

            scope.i18NHelpText = function () {
                return i18NService.get18nValue(scope.i18nKey, scope.helptext);
            }

        }
    };

});

})(angular);