var app = angular.module('sw_layout');

app.directive('multiselectDropdown', function (contextService, $log, $timeout, commandService, dispatcherService, crudContextHolderService) {
    var log = $log.getInstance('sw4.multiselectDropdown');

    return {
        templateUrl: contextService.getResourceUrl('/Content/Templates/directives/multiselectDropdown.html'),
        scope: {
            checkboxFn: '&',
            commands: '@',
            datamap: '=',
            ngModel: '=',
            schema: '='
        },
        //link: function (scope, element, attrs) {
        //},
        controller: function ($scope) {
            $scope.showOnlySelected = false;

            $scope.executeService = function (command) {
                return command.service === "$scope"
                    ? $scope.executeScopeCommand(command)
                    : commandService.doCommand($scope, command);
            };

            $scope.toggleSelected = function () {
                var selectionBuffer = crudContextHolderService.getSelectionModel().selectionBuffer;
                if (!$scope.showOnlySelected && Object.keys(selectionBuffer).length == 0) {
                    return;
                }

                $scope.showOnlySelected = !$scope.showOnlySelected;
                dispatcherService.dispatchevent('sw.crud.list.toggleselected', $scope.panelid);
            };

            $scope.toggleSelectedEnabled = function () {
                //if we are viewing the selected records, alway enable (ignore checkboxes)
                if ($scope.showOnlySelected) {
                    return true;
                }

                var selectionBuffer = crudContextHolderService.getSelectionModel().selectionBuffer;
                return Object.keys(selectionBuffer).length > 0;
            };

            $scope.toggleSelectedClass = function () {
                return $scope.showOnlySelected ? 'fa-check-square-o' : 'fa-square-o';
            };

            $scope.checkToggle = function() {
                $timeout(function() {
                    $scope.checkboxFn();
                }, false);
            };

            $scope.commandIcon = function(command) {
                var icon = command.icon;
                if (icon == null) {
                    return null;
                }

                if (icon.startsWith("$scope:")) {
                    return $scope.invokeOuterScopeFn(icon);
                }
                return icon;
            };

            $scope.commandLabel = function (command) {
                var label = command.label;

                if (label == null) {
                    return null;
                }

                if (label.startsWith("$scope:")) {
                    return $scope.invokeOuterScopeFn(label);
                }

                return label;
            };

            // verifies if it is a toggle command and returns the correct child command
            $scope.getCommand = function (command) {
                if ("ToggleCommand" === command.type) {
                    // it's a toggle command
                    return command.state ? command.onCommand : command.offCommand;
                }
                return command;
            }

            $scope.getDropdownCommands = function () {
                if (!$scope.schema || !$scope.commands) {
                    return;
                }

                return commandService.getBarCommands($scope.schema, $scope.commands);
            }
        }
    }
});
