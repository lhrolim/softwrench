(function (angular) {
    "use strict";

    

    angular.module("softwrench").directive("commandBar", [function () {
        const directive = {
            restrict: "E",
            templateUrl: "Content/Mobile/templates/directives/commandbar.html",
            replace: false,
            scope: {
                position: "@",
                schema: "=",
                datamap: "=",
                label: "@"
            },

            controller: ["$scope", "offlineCommandService", function ($scope, offlineCommandService) {

                class CommandBar {
                    constructor(commands) {
                        this.commands = commands;
                    }
                    get hasActiveCommands() {
                        return angular.isArray(this.commands) && 
                            this.commands.length > 0 && 
                            this.commands.some(c => !offlineCommandService.isCommandHidden($scope.datamap, $scope.schema, c));
                    }
                }

                $scope.commandBar = new CommandBar([]);

                $scope.getCommandBarStyleClass = () => window.replaceAll($scope.position, "\\.", "-");

                $scope.executeCommand = command => offlineCommandService.executeCommand(command, $scope.schema, $scope.datamap);

                $scope.isCommandHidden = command => !command || offlineCommandService.isCommandHidden($scope.datamap, $scope.schema, command);
                

                const init = () => {
                    const commands = offlineCommandService.getCommands($scope.schema, $scope.position);
                    $scope.commandBar.commands = commands;
                };
                const commandBarWatcher = (newValue, oldValue) => {
                    if (newValue === oldValue) return;
                    init();
                };

                init();
                
                $scope.$watch("schema", commandBarWatcher);
                $scope.$watch("position", commandBarWatcher);
            }]
        };

        return directive;
    }]);

})(angular);