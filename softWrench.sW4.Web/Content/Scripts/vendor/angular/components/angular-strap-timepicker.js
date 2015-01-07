/**
 * AngularStrap - Twitter Bootstrap directives for AngularJS
 * @version v0.7.4 - 2013-05-26
 * @link http://mgcrea.github.com/angular-strap
 * @author Olivier Louvignes <olivier@mg-crea.com>
 * @license MIT License, http://www.opensource.org/licenses/MIT
 */
angular.module('$strap.config', []).value('$strapConfig', {});
angular.module('$strap.filters', ['$strap.config']);
angular.module('$strap.directives', ['$strap.config']);
angular.module('$strap', [
  '$strap.filters',
  '$strap.directives',
  '$strap.config'
]);
'use strict';
angular.module('$strap.directives').directive('dateTimePicker', function () {
    return {
        restrict: 'E',
        replace: true,
        scope: {
            recipient: '='
        },
        template:
          '<div>' +
          '<input type="text" readonly data-date-format="yyyy-mm-dd hh:ii" name="recipientDateTime" data-date-time required>'+
          '</div>',
        link: function(scope, element, attrs, ngModel) {
            var input = element.find('input');
 
            input.datetimepicker({
                format: "mm/dd/yyyy hh:ii",
                showMeridian: true,
                autoclose: true,
                todayBtn: true,
                todayHighlight: true
            });
 
            element.bind('blur keyup change', function(){
                scope.recipient.datetime = input.val();
            });
        }
    }
});