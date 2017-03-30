﻿(function (app, angular) {
    "use strict";

    var sharedController = ["$scope","$q", "contextService", "expressionService", "commandService", "$log", "i18NService", "securityService", "$timeout", "fixHeaderService", "crudContextHolderService", "genericTicketService",
        function ($scope,$q, contextService, expressionService, commandService, $log, i18NService, securityService, $timeout, fixHeaderService, crudContextHolderService, genericTicketService) {

    $scope.invokeOuterScopeFn = function (expr, throwExceptionIfNotFound) {
        const methodname = expr.substr(7);
        const fn = $scope.ctrlfns[methodname];
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

    $scope.getCompositionToolbar = function () {
        return commandService.getBarCommands($scope.schema, 'commlog');
    }

    $scope.url = function (path) {
        return contextService.getResourceUrl(path);
    }

    $scope.commandtooltip = function (command) {
        const tooltip = command.tooltip; //if the label and tooltip are the same, only show the tooltip if the labels are hidden
        if (tooltip === null || tooltip === command.label) {
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

        if (commandService.isServiceMethod(tooltip)) {
            return commandService.executeClickCustomCommand(tooltip);
        }

        return i18NService.get18nValue('_bars.gridtop' + command.id, tooltip);
    }

    $scope.showLabel = function () {
        //use global property to hide/show labels
        return contextService.getFromContext("UIShowToolbarLabels", false, true);
    }

    $scope.commandLabel = function (command) {
        const label = command.label; // the labels if needed
        if (!$scope.showLabel()) {
            return "";
        }

        if (label == null) {
            return null;
        }

        if (label.startsWith("$scope:")) {
            return $scope.invokeOuterScopeFn(label);
        }

        if (label.startsWith("service:")) {
            return expressionService.evaluate(label, $scope.datamap, $scope);
        }

        return label;
    }

    $scope.commandIcon = function (command) {
        const icon = command.icon;
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
        const fn = $scope.ctrlfns[command.method];
        if (!angular.isFunction(fn)) {
            $log.get("gridtoolBar#executeScopeCommand", ["command"])
                .warn("method", command.method, "not found in the outer scope");
            return;
        }
        fn();
    };

    $scope.clickEnabled = function (command) {
        var executeClick = true;
        const expression = command.enableExpression;
        if (!expression) {
            return true;
        }

        const fn = $scope.ctrlfns[expression.split('$scope:')[1]];
        if (!!fn && angular.isFunction(fn)) {
            executeClick = fn();
        } else if (commandService.isServiceMethod(expression)) {
            const dm = crudContextHolderService.rootDataMap();
            executeClick = commandService.executeClickCustomCommand(expression, dm);
        }

        return executeClick;
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
            fixHeaderService.callWindowResize();
        }, false);

        //don't execute disabled commands
        if (!$scope.clickEnabled(command)) {
           return $q.reject();
        }

        return command.service === "$scope"
            ? $scope.executeScopeCommand(command)
            : commandService.doCommand($scope, command);
    };

    $scope.shouldShowCommand = function (command) {
        const showExpression = command.showExpression;
        if (showExpression && showExpression.startsWith("$scope:")) {
            const result = $scope.invokeOuterScopeFn(showExpression);
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
        const enableExpression = command.enableExpression;
        if (!enableExpression) {
            return true;
        }

        if (enableExpression.startsWith("$scope:")) {
            const result = $scope.invokeOuterScopeFn(enableExpression);
            if (result == null) {
                return true;
            }
            return result;
        }
        const datamap = $scope.datamap;
        return expressionService.evaluate(enableExpression, datamap);
    }

    $scope.buttonType = (command) => {
        return command.primary ? "submit" :"button";
    }

    $scope.buttonClasses = function (command) {
        var classes = "btn ";

        if (command.primary) {
            classes += " btn-primary ";
        }

        if (command.pressed) {
            classes += " active ";
        }

        if (!$scope.clickEnabled(command)) {
            classes += " disabled ";
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

    $scope.showCommLogButtons = function (commLogDatamap) {
        return !!commLogDatamap && !genericTicketService.isClosed();
    };

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
            selectedids: '=',
            commLogDatamap: '='
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

        link: function (scope, element, attrs, ctrl) {
            scope.ctrlfns = {};
            angular.forEach(ctrl, function (fn, name) {
                if (angular.isFunction(fn)) scope.ctrlfns[name] = fn;
            });
        },

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
                .inherit(sharedController, { $scope: $scope });
        }]

    };
}]);

})(app, angular);