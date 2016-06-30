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
                
                const updateCommandBar = (schema, position) => {
                    const commands = offlineCommandService.getCommands(schema || $scope.schema, position || $scope.position);
                    $scope.commandBar = new CommandBar(commands);
                } 
                const init = () => updateCommandBar();

                init();
                
                $scope.$watch("schema", (newSchema, oldSchema) => {
                    if (newSchema === oldSchema || angular.equals(newSchema, oldSchema)) return;
                    updateCommandBar(newSchema);
                }, true);
                $scope.$watch("position", (newPosition, oldPosition) => {
                    if (newPosition === oldPosition) return;
                    updateCommandBar(null, newPosition);
                });
            }]
        };

        return directive;
    }]);

})(angular);