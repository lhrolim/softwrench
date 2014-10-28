var app = angular.module('sw_layout');

app.factory('iconService', function ($rootScope, $timeout, i18NService) {

    return {


        loadIcon: function (value,metadata) {
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
            //forgot to declare it, just return
            return '';
        }
     
        
    };

});


