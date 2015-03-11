var app = angular.module('sw_layout');

app.directive('printsectionrendered', function ($timeout, $log) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.$last === true) {
                $log.getInstance("printrendered#event").debug("Print Rendered event call");
                var eventref =scope.$on("sw_bodyrenderedevent", function(key, value) {
                    $timeout(function() {
                        scope.$emit('sw_printsectionrendered');
                        scope.$$listeners['sw_printsectionrendered'] = null;
                    }, 0, false);
                });

            }
        }
    };
});

app.directive('printSection', function (contextService) {
    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/directives/printSection.html'),
        scope: {
            schema: '=',
            datamap: '=',
        },

        controller: function ($scope, $timeout, $log, printService, tabsService, i18NService, compositionService, fieldService) {

            $scope.compositionstoprint = [];
            $scope.shouldPageBreak = true;
            $scope.showPrintSection = false;
            $scope.showPrintSectionCompostions = false;

            $scope.$on('sw_readytoprintevent', function (event, compositionData, shouldPageBreak, shouldPrintMain) {
                
                var compositionstoprint = [];
                $scope.shouldPageBreak = shouldPageBreak;
                $scope.shouldPrintMain = shouldPrintMain;
                $scope.printSchema = $scope.schema.printSchema != null ? $scope.schema.printSchema : $scope.schema;
                
                $.each(compositionData, function (key, value) {
                    var compositionToPrint = {};
                    if (value.schema != undefined) {
                        //this happens for tabs
                        compositionToPrint.schema = value.schema;
                        $scope.datamap.fields[key] = value.items;
                        compositionToPrint.title = value.title;
                    } else {
                        compositionToPrint.schema = compositionService.locatePrintSchema($scope.printSchema, key);
                        $scope.datamap.fields[key] = value;
                        compositionToPrint.title = compositionService.getTitle($scope.printSchema, key);
                    }
                    compositionToPrint.key = key;
                    compositionstoprint.push(compositionToPrint);
                });
                $scope.compositionstoprint = compositionstoprint;
                $scope.printDatamap = Array.isArray($scope.datamap) ? $scope.datamap : new Array($scope.datamap);
                $scope.showPrintSection = true;
                $scope.showPrintSectionCompostions = compositionstoprint.length > 0;
            });

            $scope.$on('sw_readytoprintdetailedlistevent', function (event, detailedListData, compositionsToExpand, shouldPageBreak, shouldPrintMain) {
                var compositionstoprint = [];
                $scope.shouldPageBreak = shouldPageBreak;
                $scope.shouldPrintMain = shouldPrintMain;
                $scope.printSchema = $scope.schema.printSchema != null ? $scope.schema.printSchema : $scope.schema;

                $.each(compositionsToExpand, function (key, value) {
                    var compositionToPrint = {};
                    compositionToPrint.schema = value.schema;
                    compositionToPrint.key = key;
                    if (value.schema.type == 'ApplicationTabDefinition') {
                        compositionToPrint.title = value.schema.label;
                    } else {
                        compositionToPrint.title = compositionService.getTitle($scope.printSchema, key);
                    }
                    compositionstoprint.push(compositionToPrint);
                });
                
                $scope.compositionstoprint = compositionstoprint;
                $scope.printDatamap = detailedListData;
                $scope.showPrintSection = true;
                $scope.showPrintSectionCompostions = compositionstoprint.length > 0;
            });

            $scope.i18nValue = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.$on('sw_printsectionrendered', function () {
                printService.hidePrintModal();
                printService.doPrint();                
                $scope.showPrintSection = false;
                $scope.showPrintSectionCompostions = false;
                $scope.printSchema = null;
            });

        }
    };
});