﻿app.directive('crudInputWrapper', function (contextService, $compile, $rootScope) {
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
            tabid: '@',
            ismodal: '@'
        },

        link: function (scope, element, attrs) {
            var doLoad = function () {
                element.append(
                  "<crud-input elementid='crudInputMain' schema='schema' extraparameters='extraparameters'" +
                  "datamap='datamap' association-options='associationOptions' blockedassociations='blockedassociations'" +
                  "association-schemas='associationSchemas'cancelfn='cancel(data,schema)' displayables='displayables'" +
                  "savefn='save(selecteditem, parameters)' previousschema='previousschema' previousdata='previousdata' ismodal='{{ismodal}}'/>"
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

            scope.save = function (selecteditem, parameters) {
                scope.savefn({ selecteditem: scope.selecteditem, parameters: scope.parameters});
            };

            scope.cancel = function (data,schema) {
                scope.cancelfn({ data: data, schema: schema });
                scope.$emit('sw_cancelclicked');
            };
        }
    }
});

app.directive('crudInput', function (contextService, associationService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/crud/crud_input.html'),
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
            composition: '@',
            ismodal: '@'
        },


        controller: function ($scope, $http, $injector, $element,alertService, printService, compositionService, commandService, fieldService, i18NService) {

            $scope.$name = 'crudinput';

            this.cancel = function () {
                $scope.cancelfn({ data: $scope.previousdata, schema: $scope.previousschema });
                $scope.$emit('sw_cancelclicked');
            };

            this.save = function () {
                $scope.savefn({ selecteditem: $scope.selecteditem, parameters: $scope.parameters});
            };

            this.shouldshowprint = function () {
                return $scope.composition != "true";
            }

            $scope.isEditing = function (schema, datamap) {
                var id = datamap[schema.idFieldName];
                return id != null;
            };

            if ($scope.composition == "true") {
                associationService.getEagerAssociations($scope);
            }


            $injector.invoke(BaseController, this, {
                $scope: $scope,
                i18NService: i18NService,
                fieldService: fieldService,
                commandService: commandService
            });


        }
    };
});