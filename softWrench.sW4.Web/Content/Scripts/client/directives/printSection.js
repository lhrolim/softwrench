(function (angular) {
    "use strict";

var app = angular.module('sw_layout');

app.directive('printsectionrendered', function ($timeout) {
    "ngInject";

    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.$last === true || attr.list === "true") {
                $timeout(function () {
                    scope.$emit('sw_printsectionrendered');
                }, 1000);
            }
        }
    };
});

app.directive('printSection', function (contextService) {
    "ngInject";

    return {
        restrict: 'E',
        replace: true,
        templateUrl: contextService.getResourceUrl('/Content/Templates/directives/printSection.html'),
        scope: {
            schema: '=',
            datamap: '=',
        },

        controller: function ($scope, $rootScope, $timeout, $log, printService, tabsService, i18NService, compositionService, fieldService, fixHeaderService, historyService) {
            $scope.compositionstoprint = [];
            $scope.shouldPageBreak = true;
            $scope.showPrintSection = false;
            $scope.showPrintSectionCompostions = false;
            $scope.printCallback = null;
            $scope.showPrintLogo = false;
            $scope.displayableID = null;

            historyService.getRouteInfo().then(info => {
                if ("otb" === $rootScope.clientName) {
                    $scope.printLogo = info.contextPath + "/Content/Images/logo-pdf.png";
                } else {
                    $scope.printLogo = info.contextPath + "/Content/Customers/" + $rootScope.clientName + "/images/logo-pdf.png";
                }
                $.ajax({
                    url: $scope.printLogo, type: "HEAD",
                    success: function () {
                        $scope.showPrintLogo = true;
                    }
                });
            });

            var buildDisplayableID = function() {
                $scope.displayableID = null;
                if (!$scope.printSchema || !$scope.printSchema.stereotype || !$scope.printSchema.stereotype.startsWith("Detail") || !$scope.printSchema.userIdFieldName) {
                    return;
                }
                if (!$scope.printDatamap || $scope.printDatamap.length < 1) {
                    return;
                }

                var fields = $scope.printDatamap[0];
                var id = fields[$scope.printSchema.userIdFieldName];
                if (!id) {
                    return;
                }

                var prefix = $scope.printSchema.idDisplayable ? $scope.printSchema.idDisplayable : "#";
                $scope.displayableID = prefix + " " + id;
            }

            $scope.printComposition = function (item, composition) {
                return item[composition.key].length > 0;
            };

            $scope.doStartPrint = function (compositionData, shouldPageBreak, shouldPrintMain, printCallback, printDatamap) {
                fixHeaderService.unfix();
                var compositionstoprint = [];
                $scope.shouldPageBreak = shouldPageBreak;
                $scope.shouldPrintMain = shouldPrintMain;
                $scope.printSchema = $scope.schema.printSchema != null ? $scope.schema.printSchema : $scope.schema;

                $.each(compositionData, function (key, value) {
                    var compositionToPrint = {};
                    if (value.schema != undefined) {
                        //this happens for tabs
                        compositionToPrint.schema = value.schema;
                        $scope.datamap[key] = value.items;
                        compositionToPrint.title = value.title;
                    } else {
                        compositionToPrint.schema = compositionService.locatePrintSchema($scope.printSchema, key);
                        $scope.datamap[key] = value;
                        compositionToPrint.title = compositionService.getTitle($scope.printSchema, key);
                    }
                    compositionToPrint.key = key;
                    compositionstoprint.push(compositionToPrint);
                });
                $scope.compositionstoprint = compositionstoprint;
                $scope.printDatamap = printDatamap ? printDatamap : (Array.isArray($scope.datamap) ? $scope.datamap : new Array($scope.datamap));
                buildDisplayableID();
                $scope.printCallback = printCallback;
                $scope.showPrintSection = true;
                $scope.showPrintSectionCompostions = compositionstoprint.length > 0;
                fixHeaderService.fixThead($scope.schema);
            }

            $scope.$on('sw_readytoprintevent', function (event, compositionData, shouldPageBreak, shouldPrintMain, printCallback) {
                $scope.isList = false;
                $scope.doStartPrint(compositionData, shouldPageBreak, shouldPrintMain, printCallback);
            });

            $scope.$on('sw_readytoprintdetailedlistevent', function (event, detailedListData, compositionsToExpand, shouldPageBreak, shouldPrintMain) {
                $scope.isList = false;
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

            $scope.$on("sw_readytoprintlistevent", function (event, datamap) {
                $scope.isList = true;
                $scope.doStartPrint({}, false, false, null, datamap);
            });

            $scope.i18nValue = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.$on('sw_printsectionrendered', function () {
                if (sessionStorage.mockprint) {
                    return;
                }
                if ($scope.printCallback) {
                    $scope.printCallback();
                } else {
                    printService.hidePrintModal();
                    printService.doPrint($scope.isList, $scope.printSchema);
                }
                $scope.showPrintSection = false;
                $scope.showPrintSectionCompostions = false;
                $scope.printSchema = null;
                $scope.printCallback = null;
            });

            if (sessionStorage.mockprint && contextService.isDev()) {
                $scope.doStartPrint([], true, true, true);
            }
        }
    };
});

})(angular);