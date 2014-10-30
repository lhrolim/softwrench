app.directive('crudOutput', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_output.html'),
        scope: {
            schema: '=',
            displayables: '=',
            datamap: '=',
            cancelfn: '&',
            previousschema: '=',
            previousdata: '=',
            hasError: '='
        },

        controller: function ($scope, $injector, formatService, printService, tabsService, fieldService, commandService, redirectService, i18NService) {
            $scope.$name = 'crudoutput';


            this.shouldshowprint = function () {
                return $scope.composition != "true";
            }

            this.shouldshowsave = function () {
                return false;
            }

            this.cancel = function () {
                $('#crudmodal').modal('hide');
                if (GetPopUpMode() == 'browser') {
                    close();
                }
                $scope.cancelfn({ data: $scope.previousdata, schema: $scope.previousschema });
                $scope.$emit('sw_cancelclicked');
            };

            //TODO:RemoveThisGambi
            $scope.redirectToHapagHome = function () {
                redirectService.redirectToAction(null, 'HapagHome', null, null);
            };

            $scope.redirectToAction = function (title, controller, action, parameters) {
                redirectService.redirectToAction(title, controller, action, parameters);
            };

            $scope.nonTabFields = function (displayables) {
                return fieldService.nonTabFields(displayables);
            };

            $injector.invoke(BaseController, this, {
                $scope: $scope,
                i18NService: i18NService,
                fieldService: fieldService,
                commandService: commandService
            });

        }

    };
});