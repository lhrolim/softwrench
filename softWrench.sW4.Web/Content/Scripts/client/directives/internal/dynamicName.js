(function (angular) {
    "use strict";

    angular.module('sw_layout').directive("dynamicName", function ($compile) {
        "ngInject";
        /// <summary>
        /// workaround for having dynamic named forms to work with angular 1.2
        /// took from http://jsfiddle.net/YAZmz/2/
        /// </summary>
        /// <param name="$compile"></param>
        /// <returns type=""></returns>
        return {
            restrict: "A",
            terminal: true,
            priority: 1000,
            link: function (scope, element, attrs) {
                element.attr('name', scope.$eval(attrs.dynamicName));
                element.removeAttr("dynamic-name");
                $compile(element)(scope);
            }
        };
    });
  

})(angular);