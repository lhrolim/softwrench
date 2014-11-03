app.directive('crudInput', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_input.html'),
        scope: {
            schema: '=',
            displayables: '=',
            datamap: '=',
            associationOptions: '=',
            associationSchemas: '=',
            blockedassociations: '=',
            cancelfn: '&',
            savefn: '&',
            previousschema: '=',
            previousdata: '=',
            title: '=',
            elementid: '@',
            composition:'@'
        },

        controller: function ($scope, $http, $injector, $element, printService, compositionService, commandService, fieldService, i18NService) {
            
            $scope.$name = 'crudinput';

            this.cancel = function () {
                $scope.cancelfn({ data: $scope.previousdata, schema: $scope.previousschema });
                $scope.$emit('sw_cancelclicked');
            };

            this.save = function () {
                $scope.savefn();
            };

            this.shouldshowprint = function () {
                return $scope.composition != "true";
            }

            $scope.isEditing = function (schema, datamap) {
                var id = datamap[schema.idFieldName];
                return id != null;
            };

         

            $injector.invoke(BaseController, this, {
                $scope: $scope,
                i18NService: i18NService,
                fieldService: fieldService,
                commandService:commandService
            });


        }
    };
});