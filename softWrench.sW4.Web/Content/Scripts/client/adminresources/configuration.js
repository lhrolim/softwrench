﻿(function (angular) {
    "use strict";

var CONDITIONMODAL_$_KEY = window.CONDITIONMODAL_$_KEY = '[data-class="conditionModal"]';
    const app = angular.module('sw_layout');
    app.directive('configrendered', function ($timeout) {
    "ngInject";

    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.$last === true) {
                $timeout(function () {
                    $('.no-touch [rel=tooltip]').tooltip({container: 'body', trigger: 'hover'});
                    scope.$emit(JavascriptEventConstants.BodyRendered);
                
                });
            }
        }
    };
});

app.directive('conditionmodal', function (contextService) {
    "ngInject";

    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/configConditionModal.html'),
        scope: {
            profile: '=',
            module: '=',
            type: '=',
            condition: '=',
            fullkey: '=',
            applications:"="
        },
        controller: function ($scope, $http, $timeout, i18NService) {

            $scope.i18N = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.title = function() {
                const creating = $scope.condition.id == null;
                return creating ? "Create Condition" :"Edit Condition";
            }

            $scope.init = function () {
                if ($scope.condition == null) {
                    $scope.condition = {};
                    $scope.condition.appContext = {};
                }

                $timeout(function () {
                    $("#applicationsautocmpmodal").combobox({
                        minLength: 2,
                        pageSize: 100
                    });
                }, 0, false);

            };

            $scope.saveCondition = function () {
                $scope.condition.fullKey = $scope.fullkey;
                const iscreating = $scope.condition.id == null;
                const jsonString = angular.toJson($scope.condition);
                $http.put(url("/api/generic/Configuration/CreateCondition"), jsonString)
                    .then(function (response) {
                        const data = response.data;
                        const modal = $(CONDITIONMODAL_$_KEY);
                        modal.modal('hide');
                        $scope.$emit("sw_conditionsaved", data.resultObject);
                });
            };

            $scope.init();


        }
    };
});

app.directive("conditionTextInput", function (contextService) {
    "ngInject";

    return {
        restrict: 'A',
        require: "ngModel",
        link: function (scope, element, attrs, ngModel) {
            ngModel.$parsers.push(function (value) {
                if (value !== "") return value;
                return null;
            });
        }
    };
});

app.controller("ConfigController", configController);
function configController($scope, $http, $timeout, i18NService, alertService) {
    "ngInject";

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
        return (""+defValue).isEqual(""+scopeValue[property],true);
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
        $scope.applications = $scope.resultData.applications;

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

        if ($scope.applications != null) {
            $scope.currentapplication = $scope.applications[0];
        }

        $timeout(function() {
            $("#applicationsautocmp").combobox({
                minLength: 2,
                pageSize: 100
            });
        },0,false);

    };

    $scope.$on("sw_conditionsaved", function (event, data) {
        const currentcategory = $scope.currentCategory;
        currentcategory.conditionsToShow = null;
        insertOrUpdateArray($scope.getConditions(currentcategory).values, data);
        insertOrUpdateArray($scope.allConditions, data);
        $scope.currentcondition = data;
    });

    $scope.restoreDefault = function (definition) {
        alertService.confirm('Are you sure you want to restore the default value?')
            .then(function (result) {
                $scope.currentValues[definition.fullKey] = null;
            }
        );
    };

    $scope.getCurrentCondition = function () {
        if ($scope.currentCondition == null || $scope.currentCondition.id == null) {
            return {};
        }
        return $scope.currentcondition;
    };

    $scope.removeCondition = function (condition) {
        if (condition.id == null) {
            alertService.alert('Please, select a condition to remove');
            return;
        }

        alertService.confirm(null, 'condition', condition.alias).then(function (result) {
            const jsonString = angular.toJson(condition);
            $http.put(url("/api/generic/Configuration/DeleteCondition?currentKey=" + $scope.currentCategory.fullKey), jsonString)
                .then(function (response) {
                    const data = response.data;
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
        const modal = $(CONDITIONMODAL_$_KEY);
        $scope.modalcondition = $scope.getCurrentCondition();
        $scope.fullkey = $scope.currentCategory.fullKey;
        modal.draggable();
        modal.modal('show');
    };

    $scope.numberofrowsofValueArea=function(key) {
        return $scope.hasDefaultValue(key) ? 20:27;
    }

    $scope.hasDefaultValue = function(key) {
        return !nullOrEmpty($scope.currentDefaultValues[key]);
    }

    $scope.editCondition = function (condition) {
        if (condition.id == null) {
            alertService.alert('Please, select a condition to edit');
            return;
        }
        const modal = $(CONDITIONMODAL_$_KEY);
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
        for (let i = 0; i < cat.definitions.length; i++) {
            var def = cat.definitions[i];
            $scope.currentValues[def.fullKey] = null;
            $scope.currentDefaultValues[def.fullKey] = def.stringValue;
            const values = def.values;
            if (values == null) {
                //sometimes we don´t have any value but the default one
                return;
            }
            var exactMatchSet = false;
            $.each(values, function (key, propertyValue) {
                const moduleMatches = isNullOrEqualMatching(propertyValue.module, $scope.currentmodule, 'id');
                const profileMatches = isNullOrEqualMatchingNumeric(propertyValue.userProfile, $scope.currentprofile, 'id');
                const exactMatch = moduleMatches.mode == true && profileMatches.mode == true;
                if ((moduleMatches.mode == null || moduleMatches.mode == true) &&
                    (profileMatches.mode == null || profileMatches.mode == true) &&
                    nullOrEqualBothNulls(propertyValue.conditionId, $scope.currentcondition, 'id')) {
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

    $scope.$watch("currentapplication", function (newvalue, oldvalue) {
        if (newvalue != undefined) {
            $scope.currentCategory = navigateToCategory($scope.categoryData, "/_whereclauses/{0}/".format(newvalue.value));
            $scope.showDefinitions($scope.currentCategory);
        }
    });

    $scope.save = function () {
        var currentCategory = $scope.currentCategory;
        currentCategory.valuesToSave = $scope.currentValues;
        currentCategory.module = $scope.currentmodule;
        currentCategory.userProfile = $scope.currentprofile.id;
        currentCategory.condition = $scope.currentcondition.id == null ? null : $scope.currentcondition;
        const jsonString = angular.toJson($scope.currentCategory);
        return $http.put(url("/api/generic/Configuration/Put"), jsonString)
            .then(function (response) {
                const data = response.data;
                $scope.categoryData = data.resultObject;
//                $scope.categoryData[0].condition = currentCategory.condition;
                $scope.currentCategory = navigateToCategory($scope.categoryData, currentCategory.fullKey);
                $scope.currentCategory.condition = currentCategory.condition;
                $scope.currentCategory.conditionsToShow = null;
        });
    };

    $scope.$on(JavascriptEventConstants.BodyRendered, function (ngRepeatFinishedEvent) {
        const fileInput = $("input[type=file]");
        if (!fileInput.exists()) return;
        fileInput.filestyle({
            image: url("/Content/Images/update_24.png"),
            imageheight: 32,
            imagewidth: 25,
            width: 250
        });
    });
};

})(angular);