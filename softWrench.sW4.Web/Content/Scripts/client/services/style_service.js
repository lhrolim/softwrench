var app = angular.module('sw_layout');

app.factory('styleService', function ($rootScope, $timeout, i18NService) {

    "ngInject";

    return {

        getLabelStyle: function (fieldMetadata,key) {
            var parameters = fieldMetadata.rendererParameters;
            if (fieldMetadata.header != null) {
                parameters = fieldMetadata.header.parameters;
            }
            if (parameters == null) {
                return null;
            }
            return parameters[key];

        },

      
    };

});


