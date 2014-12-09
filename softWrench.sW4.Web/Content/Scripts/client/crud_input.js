app.directive('crudInputWrapper', function (contextService, $compile, $rootScope) {
    return {
        restrict: 'E',
        replace: true,
        template: "<div></div>",

        scope: {
            schema: '=',
            displayables: '=',
            datamap: '=',
            extraparameters: '=',
            associationOptions: '=',
            associationSchemas: '=',
            blockedassociations: '=',
            cancelfn: '&',
            savefn: '&',
            previousschema: '=',
            previousdata: '=',
            title: '=',
            elementid: '@',
            isMainTab: '@',
            tabid:'@'
        },

        link: function (scope, element, attrs) {
            var doLoad = function () {
                element.append(
                  "<crud-input elementid='crudInputMain' schema='schema' extraparameters='extraparameters'" +
                  "datamap='datamap' association-options='associationOptions' blockedassociations='blockedassociations'" +
                  "association-schemas='associationSchemas'cancelfn='toListSchema(data,schema)' displayables='displayables'" +
                  "savefn='save(selecteditem, parameters)' previousschema='previousschema' previousdata='previousdata' />"
               );
                $compile(element.contents())(scope);
                scope.loaded = true;
            }

            if (scope.schema.mode == "input" && ("true" == scope.isMainTab)) {
                doLoad();
            }

            scope.$on("sw_lazyloadtab", function (event, tabid) {
                if (scope.tabid == tabid && !scope.loaded) {
                    doLoad();
                }
            });

            scope.save = function () {
                scope.savefn();
            };

            scope.cancel = function () {
                scope.cancelfn({ data: scope.previousdata, schema: scope.previousschema });
                scope.$emit('sw_cancelclicked');
            };
        }
    }
});

app.directive('crudInput', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_input.html'),
        scope: {
            schema: '=',
            displayables: '=',
            datamap: '=',
            extraparameters:'=',
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
                $scope.$parent.cancel();
//                $scope.$emit('sw_cancelclicked');
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