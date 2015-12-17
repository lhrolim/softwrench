var PRINTMODAL_$_KEY = '[data-class="printModal"]';

app.directive('printModal', function ($log, contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/printModal.html'),
        scope: {
            schema: '=',
            datamap: '=',
        },

        controller: function ($scope, printService, tabsService, i18NService) {
                        
            $scope.i18N = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.print = function () {
                if (Array.isArray($scope.datamap)) {
                    printService.printDetailedList($scope.printSchema, $scope.datamap,$scope.searchSort, $scope.buildPrintOptions());
                } else {
                    printService.printDetail($scope.printSchema, $scope.datamap, $scope.buildPrintOptions());
                }
            };

            $scope.buildPrintOptions = function () {
                var printOptions = {};
                printOptions.shouldPageBreak = $scope.shouldPageBreak;
                printOptions.shouldPrintMain = $scope.shouldPrintMain;
                printOptions.compositionsToExpand = $scope.compositionstoprint;
                return printOptions;

            };

            $scope.nonInlineCompositions = function(schema) {
                return tabsService.tabsDisplayables(schema);
            };
            
            $scope.$on('sw_hideprintmodal', function (event) {

                var modal = $(PRINTMODAL_$_KEY);
                modal.modal('hide');

            });

            $scope.$on('sw_showprintmodal', function (event, schema,searchSort) {
                $log.getInstance('printmodal').info("starting printing modal");
                $scope.compositionstoprint = {};
                $scope.printSchema = schema;
                $scope.searchSort = searchSort;

                var tabs = tabsService.tabsDisplayables($scope.printSchema);
                for (var i = 0; i < tabs.length ; i++) {
                    var tab = tabs[i];
                    if (tab.type == "ApplicationCompositionDefinition") {
                        var schemaToPrint = tab.schema.schemas.print ? tab.schema.schemas.print : tab.schema.schemas.list;
                        $scope.compositionstoprint[tab.relationship] = { value: false, schema: schemaToPrint };
                    } else {
                        $scope.compositionstoprint[tab.id] = { value: false, schema: tab };
                    }
                }
                
                var modal = $(PRINTMODAL_$_KEY);
                modal.draggable();
                modal.modal('show');

            });

            function init() {

                $scope.printSchema = $scope.schema.printSchema != null ? $scope.schema.printSchema : $scope.schema;

                //make all the ng-models´s objects true by default... angular will just work on binding upon click
                $scope.compositionstoprint = {};
                $scope.shouldPageBreak = false;
                $scope.shouldPrintMain = true;
                
            };

            init();
        }
    };
});