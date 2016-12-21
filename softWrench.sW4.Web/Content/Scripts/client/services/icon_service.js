
(function (angular) {
    'use strict';

  
    function iconService($log) {
      

        function loadIcon(value, metadata) {
            var expression = metadata.rendererParameters['expression'];
            if (expression != null) {
                expression = replaceAll(expression, '\'', "\"");
                try {
                    var expressionObj = JSON.parse(expression);
                    var result = expressionObj[value];
                    if (result == null) {
                        //switch case deafult
                        return expressionObj["#default"];
                    }
                    return result;
                } catch (e) {
                    $log.getInstance('compositionlist#loadicon').warn('invalid expression definition {0}'.format(expression));
                }
            }
            var iconvalue = metadata.rendererParameters['value'];
            if (iconvalue != null) {
                return iconvalue;
            }
            iconvalue = metadata.rendererParameters['icon'];
            if (iconvalue != null) {
                return iconvalue;
            }
            //forgot to declare it, just return
            return '';
        }

        var service = {
            loadIcon: loadIcon
        };

        return service;
    }

    angular
    .module('sw_layout')
    .service('iconService', ['$log', iconService]);

})(angular);