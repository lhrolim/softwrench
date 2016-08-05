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

            controller: ["$scope", "$rootScope", "offlineCommandService", function ($scope, $rootScope, offlineCommandService) {

                class CommandBar {
                    constructor(commands) {
                        this.commands = commands;
                        if (!this.hasActiveCommands) {
                            return;
                        }
                        const activeCommands = this.commands.filter(c => !offlineCommandService.isCommandHidden($scope.datamap, $scope.schema, c));
                        this.isSingleCommand = activeCommands.length === 1;
                        this.singleCommand = activeCommands[0];
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

                $scope.$on("sw_updatecommandbar", (event, position) => {
                    if (position === $scope.position) {
                        updateCommandBar();
                    }
                });
            }],

            link: function (scope, element, attrs) {
                //set the crud details list height
                var toolbarPrimary = $('.bar-header.bar-positive:visible').outerHeight(true);
                var toolbarSecondary = $('.bar-subheader.bar-dark:visible').outerHeight(true);
                var headerTitle = $('.crud-details .crud-title:visible').outerHeight(true);
                var headerDescription = $('.crud-details .crud-description:visible').outerHeight(true);
                var componetHeights = toolbarPrimary + toolbarSecondary + headerTitle + headerDescription;

                //TODO: impove this solution, move the fab to the crud details.html (outside of the content) and use only css
                //set the inital position of the floating action button
                var windowHeight = $(window).height();
                var offset = windowHeight - componetHeights - 134;
                $(element).css('top', offset);
            }
        };

        return directive;
    }]);

})(angular);