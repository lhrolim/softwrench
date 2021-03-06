﻿app.directive('crudOutputWrapper', function (contextService, $compile, $rootScope) {
    "ngInject";
    return {
        restrict: 'E',
        replace: true,
        template: "<div></div>",
        scope: {
            schema: '=',
            displayables: '=',
            datamap: '=',
            cancelfn: '&',
            previousschema: '=',
            previousdata: '=',
            hasError: '=',
            tabid: '@',
            isMainTab:"@"
        },
        link: function (scope, element, attrs) {

            var doLoad = function () {
                element.append(
                    "<crud-output schema='schema'" +
                    "datamap='datamap'" +
                    "displayables='displayables'" +
                    "orientation='{{orientation}}'></crud-output-fields>"
                );
                $compile(element.contents())(scope);
                scope.loaded = true;
            }

            if (scope.schema.mode == "output" && ("true" == scope.isMainTab)) {
                doLoad();
            }

            scope.$on("sw_lazyloadtab", function(event,tabid) {
                if (scope.tabid == tabid && !scope.loaded) {
                    doLoad();
                }
            });

        }
    }
});


app.directive('crudOutput', function (contextService) {
    "ngInject";
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

        controller: function ($scope,$injector, formatService, printService, tabsService, fieldService, commandService, redirectService, i18NService) {
            $scope.$name = 'crudoutput';

            $scope.cancel = function (previousdata,previousschema) {
                $('#crudmodal').modal('hide');
                if (GetPopUpMode() == 'browser') {
                    close();
                }
                $scope.cancelfn({ data: previousdata, schema: previousschema });
                $scope.$emit('sw_cancelclicked');
            };

            //TODO:RemoveThisGambi
            $scope.redirectToHapagHome = function () {
                redirectService.redirectToAction(null, 'HapagHome', null, null);
            };

            $scope.redirectToAction = function (title, controller, action, parameters) {
                redirectService.redirectToAction(title, controller, action, parameters);
            };

            $scope.printDetail = function () {
                printService.printDetail($scope.schema, $scope.datamap[$scope.schema.idFieldName]);
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