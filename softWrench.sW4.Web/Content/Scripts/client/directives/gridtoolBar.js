(function (app, angular) {
    "use strict";

    var sharedController = ["$scope", "contextService", "expressionService", "commandService", "$log", "i18NService", "securityService", "$timeout", "crudContextHolderService",
        function ($scope, contextService, expressionService, commandService, $log, i18NService, securityService, $timeout, crudContextHolderService) {

    $scope.invokeOuterScopeFn = function (expr, throwExceptionIfNotFound) {
        var methodname = expr.substr(7);
        var fn = $scope.ctrlfns[methodname];
        if (angular.isFunction(fn)) {
            return fn();
        } else if (throwExceptionIfNotFound) {
            throw new Error("parameterless method {0} not found on outer scope".format(methodname));
        }
        return null;
    }

    $scope.getGridToolbar = function () {
        return commandService.getBarCommands($scope.schema, $scope.position);
    }

    $scope.url = function (path) {
        return contextService.getResourceUrl(path);
    }

    $scope.commandtooltip = function (command) {
        var tooltip = command.tooltip;

        //if the label and tooltip are the same, only show the tooltip if the labels are hidden
        if (tooltip === command.label) {
            if ($scope.showLabel()) {
                return "";
            }
        }

        if (tooltip == null) {
            return $scope.commandLabel(command);
        }

        if (tooltip.startsWith("$scope:")) {
            return $scope.invokeOuterScopeFn(tooltip);
        }

        return i18NService.get18nValue('_bars.gridtop' + command.id, tooltip);
    }

    $scope.showLabel = function () {
        //use global property to hide/show labels
        return contextService.getFromContext("UIShowToolbarLabels", false, true);
    }

    $scope.commandLabel = function (command) {
        var label = command.label;

        // the labels if needed
        if (!$scope.showLabel()) {
            return "";
        }

        if (label == null) {
            return null;
        }

        if (label.startsWith("$scope:")) {
            return $scope.invokeOuterScopeFn(label);
        }

        return label;
    }

    $scope.commandIcon = function (command) {
        var icon = command.icon;
        if (icon == null) {
            return null;
        }

        if (icon.startsWith("$scope:")) {
            return $scope.invokeOuterScopeFn(icon);
        }
        return icon;
    }

    $scope.validateRole = function (command) {
        securityService.validateRoleWithErrorMessage(command.role);
    }

    $scope.executeScopeCommand = function (command) {
        var fn = $scope.ctrlfns[command.method];
        if (!angular.isFunction(fn)) {
            $log.get("gridtoolBar#executeScopeCommand", ["command"])
                .warn("method", command.method, "not found in the outer scope");
            return;
        }
        fn();
    };

    $scope.executeService = function (command, toggleParentCommand) {

        // if toggle parent command is passed toggles it state
        if (toggleParentCommand) {
            toggleParentCommand.state = !toggleParentCommand.state;
        }

        $timeout(function () {
            $(".no-touch [rel=tooltip]").tooltip({ container: "body", trigger: "hover" });
            $(".no-touch [rel=tooltip]").tooltip("hide");

            //update header/footer layout
            $(window).trigger("resize");
        }, false);

        return command.service === "$scope"
            ? $scope.executeScopeCommand(command)
            : commandService.doCommand($scope, command);
    };

    $scope.shouldShowCommand = function (command) {
        var showExpression = command.showExpression;
        if (showExpression && showExpression.startsWith("$scope:")) {
            var result = $scope.invokeOuterScopeFn(showExpression);
            if (result == null) {
                return true;
            }
            return result;
        } 
        return !commandService.isCommandHidden($scope.datamap, $scope.schema, command);
    }

    function calcToggleExpression(expression) {
        if (expression && expression.startsWith("$scope:")) {
            return true === $scope.invokeOuterScopeFn(expression);
        } else {
            return true === commandService.evalToggleExpression($scope.datamap, expression);
        }
    }

    function calcToggleInitialState(command) {
        if (command.initialStateExpression) {
            command.state = calcToggleExpression(command.initialStateExpression);
        }
    }

    $scope.initIfToggleCommand = function(command) {
        if ("ToggleCommand" !== command.type) {
            return;
        }
        crudContextHolderService.addToggleCommand(command, $scope.panelid);
        calcToggleInitialState(command);
    }

    $scope.isCommandEnabled = function (command) {
        var enableExpression = command.enableExpression;
        if (enableExpression && enableExpression.startsWith("$scope:")) {
            var result = $scope.invokeOuterScopeFn(enableExpression);
            if (result == null) {
                return true;
            }
            return result;
        }
        var datamap = $scope.datamap;
        var expressionToEval = expressionService.getExpression(enableExpression, datamap);
        return eval(expressionToEval);
    }

    $scope.buttonClasses = function (command) {
        var classes = "btn ";
        if (command.pressed) {
            classes += "active ";
        }
        return classes + command.cssClasses;
    };

    // verifies if it is a toggle command and returns the correct child command
    $scope.getCommand = function(command) {
        if ("ToggleCommand" === command.type) {
            // it's a toggle command
            return command.state ? command.onCommand : command.offCommand;
        }
        return command;
    }

    $scope.toggleCommandOrNull = function(command) {
        return "ToggleCommand" === command.type ? command : null;
    }

}];

app.directive('gridtoolbar', ["contextService", function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/directives/gridtoolbar.html'),
        require: '^crudList',
        scope: {
            paginationData: '=',
            searchData: '=',
            searchTemplate: '=',
            searchOperator: '=',
            searchSort: '=',
            advancedsearchdata : '=',
            schema: '=',
            mode: '@',
            position: '@',
            datamap: '=',
            panelid: '='
        },

        link: function (scope, element, attrs, ctrl) {
            scope.ctrlfns = {};
            angular.forEach(ctrl, function (fn, name) {
                if(angular.isFunction(fn)) scope.ctrlfns[name] = fn;
            });

            //scope.setShowLabel(false);
        },

        controller: sharedController

    };
}]);


app.directive('compositiontoolbar', ["contextService", function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/directives/gridtoolbar.html'),
        require: '^compositionList',
        scope: {
            paginationData: '=',
            searchData: '=',
            searchOperator: '=',
            searchSort: '=',
            /*only appliable for compositions, otherwise this will be null*/
            parentschema: '=',
            schema: '=',
            mode: '@',
            position: '@',
            /*only appliable for compositions, otherwise this will be null*/
            parentdatamap: '=',
            datamap: '=',
            //holds the selected ids amongst the ones prensent on the datamap 
            selectedids: '='
        },

        link: function (scope, element, attrs, ctrl) {
            scope.ctrlfns = {};
            angular.forEach(ctrl, function (fn, name) {
                if (angular.isFunction(fn)) scope.ctrlfns[name] = fn;
            });
        },

        controller: sharedController

    };
}]);

app.directive('masterdetailtoolbar', ["contextService", function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/directives/gridtoolbar.html'),
        require: '^compositionMasterDetails',
        scope: {
            paginationData: '=',
            searchData: '=',
            searchOperator: '=',
            searchSort: '=',
            /*only appliable for compositions, otherwise this will be null*/
            parentschema: '=',
            schema: '=',
            mode: '@',
            position: '@',
            /*only appliable for compositions, otherwise this will be null*/
            parentdatamap: '=',
            datamap: '=',
            //holds the selected ids amongst the ones prensent on the datamap 
            selectedids: '='
        },

        link: function (scope, element, attrs, ctrl) {
            scope.ctrlfns = {};
            angular.forEach(ctrl, function (fn, name) {
                if (angular.isFunction(fn)) scope.ctrlfns[name] = fn;
            });
        },

        controller: sharedController

    };
}]);


app.directive('inputdetailtoolbar', ["contextService", function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/directives/gridtoolbar.html'),
        require: '^crudInput',
        scope: {
            /*only appliable for compositions, otherwise this will be null*/
            schema: '=',
            mode: '@',
            position: '@',
            datamap: '=',
        },

        link: function (scope, element, attrs, ctrl) {
            scope.ctrlfns = {};
            angular.forEach(ctrl, function (fn, name) {
                if (angular.isFunction(fn)) scope.ctrlfns[name] = fn;
            });
        },

        controller: sharedController

    };
}]);

app.directive('generictoolbar', ["contextService", function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/directives/gridtoolbar.html'),
        scope: {
//            /*only appliable for compositions, otherwise this will be null*/
            schema: '=',
            mode: '@',
            position: '@',
            datamap: '=',
        },

//        link: function (scope, element, attrs, ctrl) {
//            scope.ctrlfns = {};
//            angular.forEach(ctrl, function (fn, name) {
//                if (angular.isFunction(fn)) scope.ctrlfns[name] = fn;
//            });
//        },

        controller: sharedController

    };
}]);

app.directive("crudbodydetailtoolbar", ["contextService", function (contextService) {
    return {
        restrict: "E",
        replace: true,
        templateUrl: contextService.getResourceUrl("/Content/Templates/directives/gridtoolbar.html"),
        require: "^crudBody",
        scope: {
            /*only appliable for compositions, otherwise this will be null*/
            schema: "=",
            mode: "@",
            position: "@",
            datamap: "="
        },

        link: function (scope, element, attrs, ctrl) {
            scope.ctrlfns = {};
            angular.forEach(ctrl, function (fn, name) {
                if (angular.isFunction(fn)) scope.ctrlfns[name] = fn;
            });
        },

        controller: ["$scope", "controllerInheritanceService", function($scope, controllerInheritanceService) {

            controllerInheritanceService.begin(this)
                .inherit(sharedController, { $scope: $scope })
//                .overrides()
//                .scope($scope, "executeScopeCommand", function (original, params, context) {
//                    var command = params[0];
//                    var method = $scope.ctrlfns[command.method];
//                    // command can be execute in the current scope
//                    if (angular.isFunction(method)) {
//                        return original.apply(context, params);
//                    }
//                    // $broadcast event so it can be intercepted and executed in the correct scope
//                    return $scope.$root.$broadcast("sw:command:scope", command.method);
//                });
        }]

    };
}]);

})(app, angular);