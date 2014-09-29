var app = angular.module('sw_layout');

app.factory('validationService', function (i18NService,fieldService, $rootScope) {



    return {
        validate: function (displayables, datamap,innerValidation) {
            var validationArray = [];
            for (var i = 0; i < displayables.length; i++) {
                var displayable = displayables[i];
                var label = displayable.label;
                if (fieldService.isNullInvisible(displayable,datamap)) {
                    continue;
                }
                if (displayable.isRequired && nullOrEmpty(datamap[displayable.attribute])) {
                    var applicationName = i18NService.get18nValue(displayable.applicationName + ".name", displayable.applicationName);
                    if (label.endsWith('s') || label.endsWith('S')) {
                        validationArray.push(i18NService.get18nValue('messagesection.validation.isrequired', 'Field {0} for {1} are required', [label, applicationName]));
                    } else {
                        validationArray.push(i18NService.get18nValue('messagesection.validation.isrequired', 'Field {0} for {1} is required', [label, applicationName]));
                    }
                }
                if (displayable.displayables != undefined) {
                    //validating section
                    var innerArray = this.validate(displayable.displayables, datamap,true);
                    validationArray = validationArray.concat(innerArray);
                }
            }
            if (validationArray.length > 0 && !innerValidation) {
                $rootScope.$broadcast('sw_validationerrors', validationArray);
            }
            return validationArray;
        },

        

    };

});


