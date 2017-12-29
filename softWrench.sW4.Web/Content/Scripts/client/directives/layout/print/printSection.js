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
                        scope.$emit(JavascriptEventConstants.PrintSectionRendered);
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

            controller: function ($scope, $q, $rootScope, $timeout, $log, printService, tabsService, i18NService, compositionService, fieldService, fixHeaderService, printAwaitableService) {
                $scope.compositionstoprint = [];
                $scope.shouldPageBreak = true;
                $scope.showPrintSection = false;
                $scope.showPrintSectionCompostions = false;
                $scope.printCallback = null;
                $scope.showPrintLogo = false;
                $scope.displayableID = null;

                var path = "/sw4";
                if (!angular.mock && !window.cordova) {
                    path = $(routes_basecontext)[0].value;
                }

                if ("otb" === $rootScope.clientName) {
                    $scope.printLogo = path + "/Content/Images/logo-pdf.png";
                } else {
                    $scope.printLogo = path + "/Content/Customers/" + $rootScope.clientName + "/images/logo-pdf.png";
                }
                $.ajax({
                    url: $scope.printLogo, type: "HEAD",
                    success: function () {
                        $scope.showPrintLogo = true;
                    }

                });

                var buildDisplayableID = function () {
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

                $scope.footerLoaded = function () {
                    if ($scope.footerLoadedeDeferred) {
                        $scope.footerLoadedeDeferred.resolve();
                        $scope.footerLoadedeDeferred = null;
                    }
                }

                $scope.headerLoaded = function () {
                    if ($scope.headerLoadedeDeferred) {
                        $scope.headerLoadedeDeferred.resolve();
                        $scope.headerLoadedeDeferred = null;
                    }
                }

                $scope.printformId = function () {
                    if (!$scope.printSchema) {
                        return null;
                    }
                    return $scope.printSchema.applicationName + "_" + $scope.printSchema.schemaId;
                }

                $scope.applyCustomStyleRules = function (schema) {
                    let styleProps = schema.properties["print.styleprops"];
                    const head = document.head || document.getElementsByTagName('head')[0];
                    const style = document.createElement('style');

                    if ($scope.hasCustomStyleApplied) {
                        const lastChild = document.head.lastElementChild;
                        if (lastChild.type === "text/css") {
                            //double checking to avoid removing a wrong element
                            document.head.removeChild(lastChild);
                            $scope.hasCustomStyleApplied = false;
                        }
                    } else if (!styleProps) {
                        return;
                    }

                    if (!styleProps) {
                        styleProps = "@page{ size:portrait }";
                    } else {
                        $scope.hasCustomStyleApplied = true;    
                    }

                    style.type = 'text/css';
                    style.media = 'print';
                    if (style.styleSheet) {
                        style.styleSheet.cssText = styleProps;
                    } else {
                        style.appendChild(document.createTextNode(styleProps));
                    }

                    
                    head.appendChild(style);
                }

                $scope.handleCustomTimeout = function (printSchema) {
                    const customTimeout = printSchema.properties["print.timeout"];
                    if (!customTimeout) {
                        return;
                    }

                    if ($scope.customTimeoutDeferred) {
                        $scope.customTimeoutDeferred.reject();
                        $scope.customTimeoutDeferred = null;
                    }
                    $scope.customTimeoutDeferred = $q.defer();
                    printAwaitableService.registerAwaitable($scope.customTimeoutDeferred.promise);
                    $timeout(() => {
                        $scope.customTimeoutDeferred.resolve();
                    }, customTimeout);
                }

                $scope.handleHeaderAndFooter = function (schema) {
                    $scope.hasCustomHeader = false;
                    $scope.hasCustomFooter = false;
                    $scope.customHeaderURL = null;
                    $scope.customFooterURL = null;
                    if ($scope.headerLoadedeDeferred) {
                        $scope.headerLoadedeDeferred.reject();
                        $scope.headerLoadedeDeferred = null;
                    }

                    if ($scope.footerLoadedeDeferred) {
                        $scope.footerLoadedeDeferred.reject();
                        $scope.footerLoadedeDeferred = null;
                    }

                    const customHeader = schema.properties["print.customheader"];
                    const customFooter = schema.properties["print.customfooter"];
                    if (customHeader) {
                        $scope.hasCustomHeader = true;
                        $scope.customHeaderURL = url(customHeader);
                        $scope.headerLoadedeDeferred = $q.defer();
                        printAwaitableService.registerAwaitable($scope.headerLoadedeDeferred.promise);
                    }

                    if (customFooter) {
                        $scope.hasCustomFooter = true;
                        $scope.customFooterURL = url(customFooter);
                        $scope.footerLoadedeDeferred = $q.defer();
                        printAwaitableService.registerAwaitable($scope.footerLoadedeDeferred.promise);
                    }
                }

                $scope.printComposition = function (item, composition) {
                    return item[composition.key].length > 0;
                };



                $scope.doStartPrint = function (compositionData, shouldPageBreak, shouldPrintMain, printCallback, printDatamap, printSchema) {
                    fixHeaderService.unfix();
                    var compositionstoprint = [];
                    $scope.shouldPageBreak = shouldPageBreak;
                    $scope.shouldPrintMain = shouldPrintMain;


                    $scope.printSchema = $scope.schema.printSchema != null ? $scope.schema.printSchema : $scope.schema;
                    $scope.printSchema = printSchema || $scope.printSchema;

                    $scope.handleHeaderAndFooter($scope.printSchema);
                    $scope.applyCustomStyleRules($scope.printSchema);
                    $scope.handleCustomTimeout($scope.printSchema);

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
                    let datamapToUse = printDatamap ? printDatamap : $scope.datamap;

                    $scope.printDatamap = Array.isArray(datamapToUse) ? datamapToUse : new Array(datamapToUse);
                    buildDisplayableID();
                    $scope.printCallback = printCallback;
                    $scope.showPrintSection = true;
                    $scope.showPrintSectionCompostions = compositionstoprint.length > 0;
                    fixHeaderService.fixThead($scope.schema);
                }

                $scope.$on(JavascriptEventConstants.ReadyToPrint, function (event, compositionData, shouldPageBreak, shouldPrintMain, printCallback, printSchema, printDatamap) {

                    $scope.isList = false;
                    $scope.doStartPrint(compositionData, shouldPageBreak, shouldPrintMain, printCallback, printDatamap, printSchema);
                });

                $scope.$on(JavascriptEventConstants.PrintReadyForDetailedList, function (event, detailedListData, compositionsToExpand, shouldPageBreak, shouldPrintMain, printSchema) {
                    $scope.isList = false;
                    var compositionstoprint = [];
                    $scope.shouldPageBreak = shouldPageBreak;
                    $scope.shouldPrintMain = shouldPrintMain;
                    $scope.printSchema = printSchema;

                    $.each(compositionsToExpand, function (key, value) {
                        var compositionToPrint = {};
                        compositionToPrint.schema = value.schema;
                        compositionToPrint.key = key;
                        if (value.schema.type === 'ApplicationTabDefinition') {
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

                $scope.$on(JavascriptEventConstants.PrintReadyForList, function (event, datamap) {
                    $scope.isList = true;
                    $scope.doStartPrint({}, false, false, null, datamap);
                });

                $scope.i18nValue = function (key, defaultValue, paramArray) {
                    return i18NService.get18nValue(key, defaultValue, paramArray);
                };

                $scope.$on(JavascriptEventConstants.PrintSectionRendered, function () {
                    const promise = printService.awaitToPrint();

                    promise.then(() => {
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
                });

                if (sessionStorage.mockprint && contextService.isDev()) {
                    $scope.doStartPrint([], true, true, true);
                }

            }
        };
    });

})(angular);