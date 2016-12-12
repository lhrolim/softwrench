(function (app) {
    "use strict";

var PRINTMODAL_$_KEY = window.PRINTMODAL_$_KEY = '[data-class="printModal"]';

app.directive('printModal', function ($log, contextService) {
    "ngInject";

    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/printModal.html'),
        scope: {
            schema: '=',
            datamap: '=',
        },

        controller: function ($scope, printService, tabsService, i18NService, crudContextHolderService) {
            $scope.lastPage = 1;

            $scope.i18N = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.print = function () {
                if ($scope.isList) {
                    printService.printList($scope.paginationData, $scope.printSchema, $scope.listOptions);
                    return;
                }

                if (Array.isArray($scope.datamap)) {
                    printService.printDetailedList($scope.printSchema, $scope.datamap, buildDetailPrintOptions());
                } else {
                    printService.printDetail($scope.printSchema, $scope.datamap, buildDetailPrintOptions());
                }
            };

            function buildDetailPrintOptions() {
                const printOptions = {};
                printOptions.shouldPageBreak = $scope.shouldPageBreak;
                printOptions.shouldPrintMain = $scope.shouldPrintMain;
                printOptions.compositionsToExpand = $scope.compositionstoprint;
                return printOptions;
            };

            $scope.pageRangeChange = function() {
                $scope.listOptions.pageOption = "pages";
            }

            $scope.nonInlineCompositions = function(schema) {
                return tabsService.tabsPrintDisplayables(schema);
            };
            
            $scope.$on(JavascriptEventConstants.PrintHideModal, function (event) {

                var modal = $(PRINTMODAL_$_KEY);
                modal.modal('hide');

            });

            $scope.$on(JavascriptEventConstants.PrintShowModal, function (event, schema, isList, paginationData) {
                $log.getInstance('printmodal').info("starting printing modal");
                $scope.isList = isList;
                $scope.compositionstoprint = {};
                $scope.shouldPrintMain = true;
                $scope.printSchema = schema;
                $scope.paginationData = paginationData;

                if (isList) {
                    $scope.listOptions = {
                        pageOption: "current",
                        startPage: 1,
                        endPage: paginationData.pageCount
                    };
                    $scope.lastPage = paginationData.pageCount;
                }

                const activetab = crudContextHolderService.getActiveTab();
                const tabs = tabsService.tabsDisplayables($scope.printSchema);
                for (var i = 0; i < tabs.length ; i++) {
                    const tab = tabs[i];
                    if (tab.type === "ApplicationCompositionDefinition") {
                        if (tab.relationship.isEqual(activetab)) {
                            $scope.compositionstoprint[tab.relationship] = { value: true, schema: tab.schema.schemas.list };
                            $scope.shouldPrintMain = false;
                        } else {
                            $scope.compositionstoprint[tab.relationship] = { value: false, schema: tab.schema.schemas.list };
                        }
                    } else {
                        $scope.compositionstoprint[tab.id] = { value: false, schema: tab };
                    }
                }
                const modal = $(PRINTMODAL_$_KEY);
                modal.draggable();
                modal.modal("show");
            });

            function init() {

                $scope.printSchema = $scope.schema.printSchema != null ? $scope.schema.printSchema : $scope.schema;

                //make all the ng-models´s objects true by default... angular will just work on binding upon click
                //$scope.compositionstoprint = {};
                $scope.shouldPageBreak = false;
                $scope.shouldPrintMain = true;
                
            };

            init();
        }
    };
});

})(app);