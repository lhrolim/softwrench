/**
 * Angular Selectize2
 * https://github.com/machineboy2045/angular-selectize
 **/

angular.module('selectize', []).value('selectizeConfig', {}).directive("selectize", ['selectizeConfig', function (selectizeConfig) {
    return {
        restrict: 'EA',
        require: '^ngModel',
        scope: { ngModel: '=', config: '=?', options: '=?', ngDisabled: '=', ngRequired: '&' },
        link: function (scope, element, attrs, modelCtrl) {

            Selectize.defaults.maxItems = null; //default to tag editor

            var selectize,
                config = angular.extend({}, Selectize.defaults, selectizeConfig, scope.config);

            modelCtrl.$isEmpty = function (val) {
                return val === undefined || val === null || !val.length; //override to support checking empty arrays
            };

            function createItem(input) {
                var data = {};
                data[config.labelField] = input.toLowerCase().trim();
                //cts:luiz --> small change here to solve SWWEB-1643
                //disregarded comment on https://github.com/machineboy2045/angular-selectize/issues/89 and solved it here
                data[config.valueField] = input.toLowerCase().trim();
                return data;
            }

            function toggle(disabled) {
                disabled ? selectize.disable() : selectize.enable();
            }

            var validate = function () {
                var isInvalid = (scope.ngRequired() || attrs.required || config.required) && modelCtrl.$isEmpty(scope.ngModel);
                modelCtrl.$setValidity('required', !isInvalid);
            };

            function generateOptions(data) {
                if (!data) {
                    return [];
                }

                data = angular.isArray(data) || angular.isObject(data) ? data : [data]

                return $.map(data, function (opt) {
                    return typeof opt === 'string' ? createItem(opt) : opt;
                });
            }

            function updateSelectize() {
                validate();

                selectize.$control.toggleClass('ng-valid', modelCtrl.$valid);
                selectize.$control.toggleClass('ng-invalid', modelCtrl.$invalid);
                selectize.$control.toggleClass('ng-dirty', modelCtrl.$dirty);
                selectize.$control.toggleClass('ng-pristine', modelCtrl.$pristine);

                if (!angular.equals(selectize.items, scope.ngModel)) {
                    selectize.addOption(generateOptions(scope.ngModel));
                    selectize.setValue(scope.ngModel);
                }
            }

            var onChange = config.onChange,
                onOptionAdd = config.onOptionAdd;

            config.onChange = function () {
                if (scope.disableOnChange)
                    return;

                if (!angular.equals(selectize.items, scope.ngModel))
                    scope.$evalAsync(function () {
                        var value = angular.copy(selectize.items);
                        if (config.maxItems == 1) {
                            value = value[0]
                        }
                        modelCtrl.$setViewValue(value);
                    });

                if (onChange) {
                    onChange.apply(this, arguments);
                }
            };

            config.onOptionAdd = function (value, data) {
                if (scope.options.indexOf(data) === -1) {
                    scope.options.push(data);

                    if (onOptionAdd) {
                        onOptionAdd.apply(this, arguments);
                    }
                }
            };

            if (scope.options) {
                // replace scope options with generated options while retaining a reference to the same array
                //                scope.options.splice(0, scope.options.length, generateOptions(scope.options));
                scope.options = generateOptions(scope.options)
            } else {
                // default options = [ngModel] if no options specified
                scope.options = generateOptions(angular.copy(scope.ngModel));
            }

            var angularCallback = config.onInitialize;

            config.onInitialize = function () {
                selectize = element[0].selectize;
                selectize.addOption(scope.options);
                selectize.setValue(scope.ngModel);

                //provides a way to access the selectize element from an
                //angular controller
                if (angularCallback) {
                    angularCallback(selectize);
                }

                //cts:luiz --> add model to list to include the cases where for some reason the element gets created with pre-filled elements, and then a lsit comes from the server without these elements 
                //(ex: reply all to an email which doesn´t belong to original list)
                //https://controltechnologysolutions.atlassian.net/browse/SWWEB-1643
                function updateOptionsWithSelectedModel(model, options) {
                    if (model == null) {
                        return;
                    }
                    options = options || [];
                    if (angular.isArray(model)) {
                        for (var i = 0; i < model.length; i++) {
                            var item = model[i];
                            if (angular.isString(item)) {
                                var newItem = createItem(item);
                                if (!options.some(function (element) {
                                    return element.value === newItem.value;
                                })) {
                                    selectize.addOption(newItem);
                                }
                            }
                        }
                    }
                }

                scope.$watch('options', function (newValue, oldValue) {
                    if (newValue === oldValue) {
                        return;
                    }
                    scope.disableOnChange = true;
                    selectize.clearOptions();
                    updateOptionsWithSelectedModel(scope.ngModel, scope.options);
                    selectize.addOption(scope.options);
                    selectize.setValue(scope.ngModel);
                    scope.disableOnChange = false;
                }, true);

                scope.$watchCollection('ngModel', updateSelectize);
                scope.$watch('ngDisabled', toggle);
            };

            // add 'remove_button' plugin: little 'x' at right side of the label to remove it
            if (!config.plugins) {
                config.plugins = ["remove_button"];
            } else if (angular.isArray(config.plugins)) {
                config.plugins.push("remove_button");
            } else {
                config.plugins["remove_button"] = {};
            }

            element.selectize(config);

            element.on('$destroy', function () {
                if (selectize) {
                    selectize.destroy();
                    element = null;
                }
            });
        }
    };
}]);