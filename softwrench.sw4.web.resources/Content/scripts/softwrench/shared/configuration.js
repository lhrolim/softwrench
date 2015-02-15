var app = angular.module('sw_layout');
var CONDITIONMODAL_$_KEY = '[data-class="conditionModal"]';

app.directive('configrendered', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.$last === true) {
                $timeout(function () {
                    $('.no-touch [rel=tooltip]').tooltip({ container: 'body' });
                    scope.$emit('sw_bodyrenderedevent');
                });
            }
        }
    };
});

app.directive('conditionmodal', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/configConditionModal.html'),
        scope: {
            profile: '=',
            module: '=',
            type: '=',
            condition: '=',
            fullkey: '='
        },
        controller: function ($scope, $http, i18NService) {

            $scope.i18N = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.init = function () {
                if ($scope.condition == null) {
                    $scope.condition = {};
                    $scope.condition.appContext = {};
                }
            };

            $scope.saveCondition = function () {
                $scope.condition.fullKey = $scope.fullkey;
                var jsonString = angular.toJson($scope.condition);
                $http.put(url("/api/generic/Configuration/CreateCondition"), jsonString)
                    .success(function (data) {
                        var modal = $(CONDITIONMODAL_$_KEY);
                        modal.modal('hide');
                    });
            };

            $scope.init();


        }
    };
});

function ConfigController($scope, $http, i18NService, alertService) {



    var noneProfile = {
        name: '-- Any --',
        id: null
    };

    var noneCondition = {
        alias: "-- No Specific --",
        id: null
    };

    var noneModule = {
        alias: "-- Any --",
        name: null,
        id: null
    };

    function navigateToCategory(categories, fullKey) {
        var resultValue = null;
        $.each(categories, function (k, value) {
            if (value.fullKey == fullKey) {
                resultValue = value;
            } else if (fullKey.startsWith(value.fullKey)) {
                resultValue = navigateToCategory(value.children, fullKey);
            }
        });
        return resultValue;
    };

    function isNullOrEqualMatching(defValue, scopeValue, property) {
        if (defValue == null) {
            return { mode: scopeValue[property] == null ? true : null };
        }
        return { mode: (defValue.isEqual(scopeValue[property],true)) };
    }

    function isNullOrEqualMatchingNumeric(defValue, scopeValue, property) {
        if (defValue == null) {
            return { mode: scopeValue[property] == null ? true : null };
        }
        return { mode: (defValue == scopeValue[property]) };
    }


    function nullOrEqualBothNulls(defValue, scopeValue, property) {
        if (defValue == null) {
            return scopeValue[property] == null;
        }
        return defValue.isEqual(scopeValue[property],true);
    }

    

    //    function zeroOrEqual(defValue, scopeValue, property) {
    //        if (defValue == 0) {
    //            return true;
    //        }
    //        return defValue == scopeValue[property];
    //    }


    $scope.$name = 'ConfigController';

    $scope.doInit = function () {
        $scope.currentCategory = {};
        $scope.showSave = false;
        $scope.categoryData = $scope.resultData.categories;
        $scope.type = $scope.resultData.type;

        $scope.currentmodule = noneModule;
        $scope.currentprofile = noneProfile;
        $scope.currentcondition = noneCondition;

        $scope.currentValues = {};
        $scope.currentDefaultValues = {};

        if ($scope.resultData.profiles != undefined) {
            $scope.resultData.profiles.unshift(noneProfile);
        }
        if ($scope.resultData.modules != undefined) {
            $scope.resultData.modules.unshift(noneModule);
        }
        if ($scope.resultData.conditions != undefined) {
            $scope.resultData.conditions.unshift(noneCondition);
        }

        $scope.profiles = $scope.resultData.profiles;
        $scope.modules = $scope.resultData.modules;
        $scope.allConditions = $scope.resultData.conditions;
    };

    $scope.restoreDefault = function (definition) {
        alertService.confirm("", "", function (result) {
            $scope.currentValues[definition.fullKey] = null;
        }, "Are you sure you want to restore the default value?");

    };

    $scope.getCurrentCondition = function () {
        if ($scope.currentCondition == null || $scope.currentCondition.id == null) {
            return null;
        }
        return $scope.currentcondition;
    };

    $scope.removeCondition = function (condition) {
        if (condition.id == null) {
            alertService.alert('This condition cannot be deleted');
            return;
        }

        alertService.confirm('condition', condition.alias, function (result) {
            var jsonString = angular.toJson(condition);
            $http.put(url("/api/generic/Configuration/DeleteCondition?currentKey=" + $scope.currentCategory.fullKey), jsonString)
                .success(function (data) {
                    data.conditions = data.resultObject;
                    if (data.conditions != undefined) {
                        data.conditions.unshift(noneCondition);
                    }
                    $scope.allConditions = data.conditions;
                    //clear cache and rebuild data
                    $scope.currentCategory.conditionsToShow = null;
                    $scope.getConditions($scope.currentCategory);
                    $scope.currentcondition = noneCondition;
                });
        });
    };

    $scope.createCondition = function () {
        var modal = $(CONDITIONMODAL_$_KEY);
        $scope.modalcondition = $scope.getCurrentCondition();
        $scope.fullkey = $scope.currentCategory.fullKey;
        modal.draggable();
        modal.modal('show');
    };

    $scope.editCondition = function (condition) {
        if (condition.id == null) {
            alertService.alert('This condition cannot be edited');
            return;
        }

        var modal = $(CONDITIONMODAL_$_KEY);
        $scope.modalcondition = condition;
        $scope.fullkey = $scope.currentCategory.fullKey;
        modal.draggable();
        modal.modal('show');
    };

    $scope.getConditions = function (category) {
        var shouldShow = false;
        if (category.conditionsToShow != null) {
            //cache
            return category.conditionsToShow;
        }

        var conditions = [];
        $.each($scope.allConditions, function (key, condition) {
            if (condition.id == null || condition.global || condition.fullKey == category.fullKey) {
                conditions.push(condition);
            }
        });

        $.each(category.definitions, function (key, definition) {
            if (definition.contextualized) {
                shouldShow = true;
            }
            if (definition.values != null) {
                $.each(definition.values, function (index, value) {
                    if (value.condition != null) {
                        conditions.push(value.condition);
                    }
                });
            }
        });
        //cache
        category.conditionsToShow = {};
        category.conditionsToShow.values = conditions;
        category.conditionsToShow.shouldShow = shouldShow;
        return category.conditionsToShow;
    };

    $scope.doInit();

    $scope.showDefinitionsOfCondition = function (currentcondition, cat) {
        $scope.currentcondition = currentcondition;
        $scope.showDefinitions(cat);
    };

    $scope.showDefinitions = function (cat) {
        $scope.currentCategory = cat;
        if (cat.definitions == null || cat.definitions.length == 0) {
            $scope.showSave = false;
            return;
        }
        $scope.showSave = true;
        //        $scope.currentCondition = noneCondition;
        for (var i = 0; i < cat.definitions.length; i++) {
            var def = cat.definitions[i];
            $scope.currentValues[def.fullKey] = null;
            $scope.currentDefaultValues[def.fullKey] = def.stringValue;
            var values = def.values;
            if (values == null) {
                //sometimes we don´t have any value but the default one
                return;
            }
            var exactMatchSet = false;
            $.each(values, function (key, propertyValue) {
                var moduleMatches = isNullOrEqualMatching(propertyValue.module, $scope.currentmodule, 'id');
                var profileMatches = isNullOrEqualMatchingNumeric(propertyValue.userProfile, $scope.currentprofile, 'id');
                var exactMatch = moduleMatches.mode == true && profileMatches.mode == true;

                if ((moduleMatches.mode == null || moduleMatches.mode == true) &&
                    (profileMatches.mode == null || profileMatches.mode == true) &&
                    nullOrEqualBothNulls(propertyValue.condition, $scope.currentcondition, 'id')) {
                    if (exactMatch) {
                        exactMatchSet = true;
                    }
                    if (exactMatch || !exactMatchSet) {
                        $scope.currentValues[def.fullKey] = propertyValue.stringValue;
                    }
                    if (propertyValue.systemStringValue != null) {
                        $scope.currentDefaultValues[def.fullKey] = propertyValue.systemStringValue;
                    }

                }
            });


        }
    };

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.Alias = function (definition) {
        if (definition.alias != null) {
            return definition.alias;
        }
        return definition.key;
    };

    $scope.$watch('resultObject.timeStamp', function (newValue, oldValue) {
        if (oldValue != newValue && $scope.resultObject.redirectURL.indexOf("Configuration.html") != -1) {
            $scope.doInit();
        }
    });

    $scope.save = function () {
        var currentCategory = $scope.currentCategory;
        currentCategory.valuesToSave = $scope.currentValues;
        currentCategory.module = $scope.currentmodule;
        currentCategory.userProfile = $scope.currentprofile.id;
        currentCategory.condition = $scope.currentcondition.id == null ? null : $scope.currentcondition;
        var jsonString = angular.toJson($scope.currentCategory);
        $http.put(url("/api/generic/Configuration/Put"), jsonString)
            .success(function (data) {
                $scope.categoryData = data.resultObject;
                $scope.currentCategory = navigateToCategory($scope.categoryData, currentCategory.fullKey);
            });
    };

    $scope.$on('sw_bodyrenderedevent', function (ngRepeatFinishedEvent) {
        $("input[type=file]").filestyle({
            image: url("/Content/Images/update_24.png"),
            imageheight: 32,
            imagewidth: 25,
            width: 250
        });
    });


};