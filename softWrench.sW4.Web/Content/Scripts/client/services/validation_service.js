var app = angular.module('sw_layout');

app.factory('validationService', function (i18NService, fieldService, $rootScope, dispatcherService, expressionService) {

    "ngInject";

    return {
        validate: function (schema, displayables, datamap, innerValidation) {
            var validationArray = [];
            for (var i = 0; i < displayables.length; i++) {
                var displayable = displayables[i];
                var label = displayable.label;
                if (fieldService.isNullInvisible(displayable, datamap)) {
                    continue;
                }

                var isRequired = !!displayable.requiredExpression ? expressionService.evaluate(displayable.requiredExpression, datamap) : false;

                if (isRequired && nullOrEmpty(datamap[displayable.attribute])) {
                    var applicationName = i18NService.get18nValue(displayable.applicationName + ".name", displayable.applicationName);
                    if (label.endsWith('s') || label.endsWith('S')) {
                        validationArray.push(i18NService.get18nValue('messagesection.validation.isrequired', 'Field {0} for {1} are required', [label, applicationName]));
                    } else {
                        validationArray.push(i18NService.get18nValue('messagesection.validation.isrequired', 'Field {0} for {1} is required', [label, applicationName]));
                    }
                }
                if (displayable.displayables != undefined) {
                    //validating section
                    var innerArray = this.validate(schema, displayable.displayables, datamap, true);
                    validationArray = validationArray.concat(innerArray);
                }
            }
            if (!innerValidation) {
                var validationService = schema.properties['oncrudsaveevent.validationservice'];
                if (validationService != null) {
                    var service = validationService.split('.')[0];
                    var method = validationService.split('.')[1];
                    var fn = dispatcherService.loadService(service, method);
                    var customErrorArray = fn(schema, datamap);
                    if (customErrorArray != null && Array.isArray(customErrorArray)) {
                        validationArray = validationArray.concat(customErrorArray);
                    }
                }
                if (validationArray.length > 0) {
                    $rootScope.$broadcast('sw_validationerrors', validationArray);
                }

            }
            return validationArray;
        },



    };

});


