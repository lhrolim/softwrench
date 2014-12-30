var app = angular.module('sw_layout');

app.factory('validationService', function (i18NService, fieldService, $rootScope, dispatcherService) {



    return {

        getInvalidLabels: function (displayables, datamap) {
            /// <summary>
            ///  Similar to the validate method, but only returning the array of items, for custom handling
            /// </summary>
            /// <param name="displayables"></param>
            /// <param name="datamap"></param>
            /// <param name="innerValidation"></param>
            /// <returns type=""></returns>
            var validationArray = [];
            for (var i = 0; i < displayables.length; i++) {
                var displayable = displayables[i];
                var label = displayable.label;
                if (fieldService.isNullInvisible(displayable, datamap)) {
                    continue;
                }
                if (displayable.requiredExpression && nullOrEmpty(datamap[displayable.attribute])) {
                    validationArray.push(label);
                }
                if (displayable.displayables != undefined) {
                    //validating section
                    var innerArray = this.validate(displayable.displayables, datamap);
                    validationArray = validationArray.concat(innerArray);
                }
            }

            return validationArray;
        },


        validate: function (schema, displayables, datamap, innerValidation) {
            var validationArray = [];
            for (var i = 0; i < displayables.length; i++) {
                var displayable = displayables[i];
                var label = displayable.label;
                if (fieldService.isNullInvisible(displayable,datamap)) {
                     continue;
                }
                if (displayable.requiredExpression && nullOrEmpty(datamap[displayable.attribute])) {
                    var applicationName = i18NService.get18nValue(displayable.applicationName + ".name", displayable.applicationName);
                    if (label.endsWith('s') || label.endsWith('S')) {
                        validationArray.push(i18NService.get18nValue('messagesection.validation.requiredExpression', 'Field {0} for {1} are required', [label, applicationName]));
                    } else {
                        validationArray.push(i18NService.get18nValue('messagesection.validation.requiredExpression', 'Field {0} for {1} is required', [label, applicationName]));
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
                $rootScope.$broadcast('sw_validationerrors', validationArray);

            }
            return validationArray;
        },

        setDirty: function () {
            $rootScope.isDirty = true;

        },

        getDirty: function () {
            return $rootScope.isDirty;

        },

        clearDirty: function () {
            $rootScope.isDirty = false;

        }


    };

});


