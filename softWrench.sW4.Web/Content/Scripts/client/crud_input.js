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
            elementid: '@'
        },

        controller: function ($scope, $http,$injector, $element, printService, compositionService, commandService, fieldService, i18NService) {
            $scope.$name = 'crudinput';

            $scope.cancel = function () {
                $scope.cancelfn({ data: $scope.previousdata, schema: $scope.previousschema });
                $scope.$emit('sw_cancelclicked');
            };

            $scope.save = function () {
                $scope.savefn();
            };

            $scope.isEditing = function (schema, datamap) {
                var id = datamap[schema.idFieldName];
                return id != null;
            };

            $scope.printDetail = function () {
                var schema = $scope.schema;
                printService.printDetail(schema, $scope.datamap[schema.idFieldName]);
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