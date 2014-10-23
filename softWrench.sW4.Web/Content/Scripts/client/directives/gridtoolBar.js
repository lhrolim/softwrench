var sharedController = function ($scope, contextService, commandService, $log, i18NService, securityService) {

    $scope.getGridToolbar = function () {
        var schema = $scope.schema;
        return commandService.getBarCommands(schema, $scope.position);
    }

    $scope.url = function (path) {
        return contextService.getResourceUrl(path);
    }

    $scope.commandtooltip = function (command) {
        return i18NService.get18nValue('_bars.gridtop' + command.id, command.tooltip);
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
            var methodname = showExpression.substr(7);
            var fn = $scope.ctrlfns[methodname];
            if (fn != null) {
                return fn();
            }
        }
        return true;
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

