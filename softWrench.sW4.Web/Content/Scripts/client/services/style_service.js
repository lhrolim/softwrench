(function (angular) {
    "use strict";

angular.module('sw_layout')
    .service('styleService', function () {

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
        }
    };

});

})(angular);