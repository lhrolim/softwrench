(function (angular) {
    "use strict";

angular.module('sw_components')
    .directive('dateTime', function ($timeout, formatService, expressionService, crudContextHolderService) {
    "ngInject";

    function parseBooleanValue(attrValue) {
        return (attrValue == undefined || attrValue === "") ? true : attrValue.toLowerCase() === "true";
    }

    function parseBooleanValueDefaultFalse(attrValue) {
        return (attrValue == undefined || attrValue === "") ? false : attrValue.toLowerCase() === "true";
    }

    const safe = function (object, key) {
        if (!object[key]) {
            object[key] = {};
        }
        return object[key];
    }

    const getRowDm = function (datamap, tableData) {
        const tableDm = safe(datamap, tableData.tableAttribute);
        return safe(tableDm, tableData.rownIndex);
    }

    function updateView(ngModel, metadata, element, datamap, angularDateFormat, isFromCompositionInline, tableData) {
        if (!metadata) {
            return;
        }

        if (ngModel.$modelValue && angularDateFormat && ngModel.$modelValue.length < angularDateFormat.length) {
            return;
        }

        const value = formatService.formatDate(ngModel.$modelValue, angularDateFormat);
         
        if (datamap) {
            if (!isFromCompositionInline) {
                let originalUnformattedAttribute = `${metadata.attribute}_formatted`;
                if (originalUnformattedAttribute[0] !== "#") {
                    originalUnformattedAttribute = "#" + originalUnformattedAttribute;
                }
                const baseOriginalDm = crudContextHolderService.originalDatamap();
                const originalDm = tableData.isTableCell ? getRowDm(baseOriginalDm, tableData) : baseOriginalDm;
                if (originalDm[originalUnformattedAttribute] === undefined) {
                    originalDm[originalUnformattedAttribute] = value;
                    //                    //setting the original value for the first time
                    //                    datamap[originalUnformattedAttribute] = datamap[scope.fieldMetadata.attribute];
                }
            }
            datamap[metadata.attribute] = value;
        }
        ngModel.$setViewValue(value);
        element.val(value);
        ngModel.$render();
    }

    function datetimeclassHandler(timeOnly,element) {
        const datetime = $('.datetime-class', element.parent());
        const calendar = 'glyphicon glyphicon-calendar';
        const time = 'glyphicon glyphicon-time';
        datetime.removeClass(calendar);
        datetime.removeClass(time);
        const classToAdd = timeOnly ? time : calendar;
        datetime.addClass(classToAdd);
    }

    return {
        restrict: 'A',
        require: '?ngModel',
        link: function (scope, element, attrs, ngModel) {

            if (!ngModel) {
                return;
            }

            // crud table info
            const isTableCell = parseBooleanValueDefaultFalse(attrs.tableCell);
            const tableData = {
                isTableCell,
                rowIndex: scope.rowindex,
                tableAttribute: scope.fieldMetadata && scope.fieldMetadata.attribute
            }

            const showTime = parseBooleanValue(attrs.showTime);
            const showDate = parseBooleanValue(attrs.showDate);
            const showMinutes = parseBooleanValue(attrs.showMinutes);
            var dateFormat = formatService.adjustDateFormatForPicker(attrs.formatString, showTime, showMinutes);
            var angularDateFormat = formatService.adjustDateFormatForAngular(attrs.formatString, showTime);
            attrs.language = (userLanguage !== '') ? userLanguage : 'en';
            const istimeOnly = showTime && !showDate;
            var datamap = isTableCell ? getRowDm(scope.datamap, tableData) : (scope.datamap || scope.compositionitem);
            const metadata = isTableCell ? scope.column : (scope.fieldMetadata || scope.column);
            const isFromCompositionInline = !!scope.column && !isTableCell;

            datetimeclassHandler(istimeOnly,element);

            scope.$watch(function () {
                return ngModel.$modelValue;
            }, function (newValue) {
                //let´s make sure that the view keeps getting update if the model changes via an api call
                updateView(ngModel, metadata, element, datamap, angularDateFormat, isFromCompositionInline, tableData);
            });

            $timeout(function () {
                updateView(ngModel, metadata, element, datamap, angularDateFormat, isFromCompositionInline, tableData);
            });

            if (dateFormat !== '' && dateFormat != undefined) {
                const allowfuture = parseBooleanValue(attrs.allowFuture);
                const allowpast = parseBooleanValue(attrs.allowPast);
                let position = !!attrs.position ? attrs.position : "bottom";
                position = metadata && metadata.rendererParameters && metadata.rendererParameters["position"] ? metadata.rendererParameters["position"] : position;

                var startDate = false;
                var useCurrent = false;
                if (!allowpast) {
                    useCurrent = true;
                    if (!showTime) {
                        startDate = new Date();
                        startDate.setHours(0, 0, 0, 0);
                    } else {
                        startDate = new Date();
                    }
                }
                var endDate = false;
                if (!allowfuture) {
                    endDate = new Date();
                    endDate.setHours(23, 59, 59, 999);
                }

                
                var minStartDateExpression = attrs.minDateexpression;

                if (minStartDateExpression != null && minStartDateExpression !== '') {
                    startDate = expressionService.evaluate(minStartDateExpression, datamap);
                    startDate = Date.parse(formatService.formatDate(startDate, attrs.dateFormat));
                    const variablesToWatch = expressionService.getVariablesForWatch(minStartDateExpression);
                    scope.$watchCollection(variablesToWatch, function (newVal, oldVal) {
                        if (newVal !== oldVal) {
                            startDate = expressionService.evaluate(minStartDateExpression, datamap);
                            startDate = formatService.formatDate(startDate, attrs.dateFormat);
                            element.data('datetimepicker').startDate = Date.parse(startDate);
                        }
                    });
                }

                //only show close button if both date and time are shown
                var showCloseButton = false;
                if (dateFormat.indexOf("/") > -1 && dateFormat.indexOf(":") > -1) {
                    showCloseButton = true;
                }

                $timeout(function () {
                    //timeout to avoid $digest is already in progress exception... using false keyword postergates this to next digest loop
                    // element.datetimepicker({
                    const elementToUse = !attrs.attachDatepickerToParent ? element : element.parent();
                    const datetimepicker = elementToUse.datetimepicker({
                        format: dateFormat,
                        locale: attrs.language,
                        maxDate: endDate,
                        minDate: startDate,
                        sideBySide: true,
                        showClose: showCloseButton,
                        toolbarPlacement: 'top',
                        useCurrent: useCurrent,
                        widgetPositioning: {
                            vertical: position
                        }
//                        debug: true
                    });

                    datetimepicker.on("dp.show", function (e) {
                        $timeout(() => {
                                scope.$emit(JavascriptEventConstants.ForceResize,true);
                            },0,false);
                    });

                    datetimepicker.on("dp.hide", function (e) {
                        $timeout(() => {
                            scope.$emit(JavascriptEventConstants.ForceResize, false);
                        }, 0, false);
                    });

                    if (isIE()) {
                        //https://controltechnologysolutions.atlassian.net/browse/SWWEB-2198
                        datetimepicker.on("dp.change", function (e) {
                            scope.$apply(function() {
                                ngModel.$modelValue = e.formattedDate;
                                const value = formatService.formatDate(ngModel.$modelValue, angularDateFormat);
                                if (datamap != undefined && metadata != undefined) {
                                    datamap[metadata.attribute] = value;
                                }
//                                updateView(ngModel, scope, element, datamap, angularDateFormat);
                            });

                        });
                    }
                }, 0, false);
            }

          
        }
    };
});

})(angular);
