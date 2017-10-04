(function (app) {
    "use strict";

app.directive('crudOutputWrapper', function (contextService, $compile) {
    "ngInject";

    return {
        restrict: 'E',
        replace: true,
        template: "<div></div>",
        scope: {
            schema: '=',
            displayables: '=',
            datamap: '=',
            extraparameters:'=',
            cancelfn: '&',
            previousschema: '=',
            previousdata: '=',
            parentdata: '=',
            hasError: '=',
            tabid: '@',
            isMainTab: "@"
        },
        link: function (scope, element, attrs) {

            var doLoad = function () {
                element.append(
                    "<crud-output schema='schema'" +
                    "datamap='datamap'" +
                    "displayables='displayables' extraparameters='extraparameters' parentdata='parentdata'" +
                    "orientation='{{orientation}}'></crud-output-fields>"
                );
                $compile(element.contents())(scope);
                scope.loaded = true;
            }

            if (("true" === scope.isMainTab)) {
                doLoad();
            }

            scope.$on("sw_lazyloadtab", function (event, tabid) {
                if (scope.tabid === tabid && !scope.loaded) {
                    doLoad();
                }
            });

            scope.save = function () {
                scope.savefn();
            };

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
            extraparameters: '=',
            datamap: '=',
            cancelfn: '&',
            previousschema: '=',
            previousdata: '=',
            hasError: '=',
            hideempty: '='
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
                commandService: commandService,
                formatService: formatService
            });
        }
    };
});

})(app);