(function (angular) {
    "use strict";

    angular.module("softwrench").directive("commandBar", ["commandBarDelegate", function (commandBarDelegate) {
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

            controller: ["$scope", "$rootScope", "offlineCommandService", function ($scope, $rootScope, offlineCommandService) {

                class CommandContainer {
                    constructor(commands) {
                        this.commands = commands;
                    }
                    get activeCommands() {
                        return this.hasCommands
                            ? this.commands.filter(c => !offlineCommandService.isCommandHidden($scope.datamap, $scope.schema, c))
                            : [];
                    }
                    get hasCommands() {
                        return angular.isArray(this.commands) && this.commands.length > 0;
                    }
                    get hasActiveCommands() {
                        return this.activeCommands.length > 0;
                    }
                }

                class CommandBar {
                    constructor(commands) {
                        const commandsDefined = angular.isArray(commands) && commands.length > 0;

                        this.containers = commandsDefined
                            ? commands.filter(c => c.type === "ContainerCommand").map(c => new CommandContainer(c.displayables))
                            : [];

                        this.childCommands = commandsDefined
                            ? new CommandContainer(commands.filter(c => c.type !== "ContainerCommand"))
                            : [];
                    }
                    get hasActiveCommands() {
                        return this.childCommands.hasActiveCommands || this.containers.some(c => c.hasActiveCommands);
                    }
                    get singleActiveCommand() {
                        return this.childCommands.activeCommands[0];
                    }
                }

                $scope.commandBar = new CommandBar([]);

                $scope.getCommandBarStyleClass = () => window.replaceAll($scope.position, "\\.", "-");

                $scope.executeCommand = command => offlineCommandService.executeCommand(command, $scope.schema, $scope.datamap);

                $scope.isCommandHidden = command => !command || offlineCommandService.isCommandHidden($scope.datamap, $scope.schema, command);
                
                const updateCommandBar = (schema, position) => {
                    const commands = offlineCommandService.getCommands(schema || $scope.schema, position || $scope.position) || [];
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

                $scope.$on("sw_updatecommandbar", (event, position) => {
                    if (position === $scope.position) {
                        updateCommandBar();
                    }
                });
            }],

            link: function (scope, element, attrs) {
                commandBarDelegate.positionFabCommandBar(element)
            }
        };

        return directive;
    }]);

})(angular);