﻿var sharedController = function ($scope, contextService, commandService, $log, i18NService, securityService) {

    $scope.invokeOuterScopeFn = function (expr,throwExceptionIfNotFound) {
        var methodname = expr.substr(7);
        var fn = $scope.ctrlfns[methodname];
        if (fn != null) {
            return fn();
        } else if (throwExceptionIfNotFound){
            throw new Error("parameterless method {0} not found on outer scope".format(methodname));
        }
        return null;
    }

    $scope.getGridToolbar = function () {
        var schema = $scope.schema;
        return commandService.getBarCommands(schema, $scope.position);
    }

    $scope.url = function (path) {
        return contextService.getResourceUrl(path);
    }

    $scope.commandtooltip = function (command) {
        var tooltip = command.tooltip;
        if (tooltip == null) {
            return $scope.commandLabel(command);
        }

        if (tooltip.startsWith("$scope:")) {
            return $scope.invokeOuterScopeFn(tooltip);
        }
        return i18NService.get18nValue('_bars.gridtop' + command.id, tooltip);
    }

    $scope.commandLabel = function (command) {
        var label = command.label;
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

    $scope.executeService = function (command) {
        if (command.service == "$scope") {
            var fn = $scope.ctrlfns[command.method];
            if (fn != null) {
                fn();
            }
            return;
        }
        commandService.doCommand($scope, command);
    }

    $scope.shouldShowCommand = function (command) {
        var showExpression = command.showExpression;
        if (showExpression && showExpression.startsWith("$scope:")) {
            var result = $scope.invokeOuterScopeFn(showExpression);
            if (result == null) {
                return true;
            }
            return result;
        }
        return true;
    }

    $scope.buttonClasses = function () {
        if ($scope.position.equalsAny('detailform', 'compositionbottom')) {
            return "btn btn-primary commandButton navbar-btn";
        }
        return "btn btn-default btn-sm hidden-xs";
    }


};

app.directive('gridtoolbar', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/directives/gridtoolbar.html'),
        require: '^crudList',
        scope: {
            paginationData: '=',
            searchData: '=',
            searchOperator: '=',
            searchSort: '=',
            schema: '=',
            mode: '@',
            position: '@',
            datamap: '=',
        },

        link: function (scope, element, attrs, ctrl) {
            scope.ctrlfns = {};
            for (var fn in ctrl) {
                scope.ctrlfns[fn] = ctrl[fn];
            }
        },

        controller: sharedController

    };
});


app.directive('compositiontoolbar', function (contextService) {
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
            for (var fn in ctrl) {
                scope.ctrlfns[fn] = ctrl[fn];
            }
        },

        controller: sharedController

    };
});


app.directive('inputdetailtoolbar', function (contextService) {
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
            for (var fn in ctrl) {
                scope.ctrlfns[fn] = ctrl[fn];
            }
        },

        controller: sharedController

    };
});

