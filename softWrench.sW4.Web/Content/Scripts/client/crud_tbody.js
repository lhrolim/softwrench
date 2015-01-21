﻿var app = angular.module('sw_layout');

function griditemclick(rowNumber, columnNumber, element) {
    //this is a trick to call a angular scope function from an ordinary onclick listener (same used by batarang...)
    //with this, we can generate the table without compiling it to angular, making it faster
    //first tests pointed a 100ms gain, but need to gather more data.
    var scope = angular.element(element).scope();
    if (scope.showDetail) {
        scope.showDetail(scope.datamap[rowNumber], scope.schema.displayables[columnNumber]);
    }
}

function defaultAppending(formattedText) {
    var st = "<div>";
    if (formattedText) {
        st += formattedText;
    }
    return st;
}

function buildStyle(minWidth, maxWidth, width, isdiv) {
    if (minWidth == undefined && maxWidth == undefined && width == undefined) {
        return "";
    }
    var style = "style=\"";
    if (minWidth != undefined) {
        style += 'min-width:' + minWidth + ";";
    }
    if (isdiv && maxWidth != undefined) {
        //we cannot set max-widths for the tds
        style += 'max-width:' + maxWidth + ";";
    }
    if (width != undefined) {
        style += 'width:' + width + ";";
    }
    return style + " \"";
};



app.directive('crudtbody', function (contextService,$rootScope, $compile, $parse, formatService, i18NService, fieldService, commandService, statuscolorService, $injector, $timeout, $log) {
    return {
        restrict: 'A',
        replace: false,
        scope: {
            datamap: '=',
            schema: '=',
        },
        template: "",
        link: function (scope, element, attrs) {


            scope.cursortype = function () {
                var editDisabled = scope.schema.properties['list.disabledetails'];
                return "true" != editDisabled ? "pointer" : "default";
            };

            scope.statusColor = function (status, gridname) {
                return statuscolorService.getColor(status, scope.schema.applicationName);
            };

            scope.getGridColumnStyle = function (column, propertyName, highResolution) {
                if (column.rendererParameters != null) {
                    //sections for instance dont have it
                    var property = column.rendererParameters[propertyName];

                    if (property != null) {
                        return property;
                    }
                }

                if (propertyName == 'maxwidth') {
                    if (highResolution) {
                        return '135px';
                    }
                    return '100px';
                }
                return null;
            }


            scope.appendDateTimeComponent = function (columnSt, rendererParameters, attribute, openCalendarTooltip) {

                var st = "<input type=\"text\" ng-model=\"{0}.fields[{1}]\" data-date-time  class=\"form-control\" ".format(columnSt, attribute);
                st += " data-show-time=\"{0}\" ".format(rendererParameters['showtime']);
                st += " data-show-date=\"{0}\"".format(rendererParameters['showdate']);
                st += " data-date-format=\"{0}\"".format(rendererParameters['format']);
                st += " data-show-meridian=\"{0}\"".format(rendererParameters['showmeridian']);
                st += " data-allow-past=\"{0}\"".format(rendererParameters['allowpast']);
                st += " data-allow-future=\"{0}\" >".format(rendererParameters['allowfuture']);
                st += "<span class=\"input-group-addon\" data-calendericon=\"true\" rel=\"tooltip\" ";
                st += " data-original-title=\"{0}\" style=\"cursor: pointer;\">".format(openCalendarTooltip);
                st += "<i class=\"datetime-class\"></i></span>";
                return st;
            }

            scope.refreshGrid = function (datamap, schema) {
                scope.datamap = datamap;
                scope.schema = schema;
                var t0 = new Date().getTime();;
                var columnarray = scope.columnarray = [];
                var hiddencolumnArray = [];
                for (var j = 0; j < schema.displayables.length; j++) {
                    var column = schema.displayables[j];
                    columnarray.push(column);
                    hiddencolumnArray.push(scope.isFieldHidden(schema, column));
                }
                var needsWatchers = false;
                var hasSection = false;
                var hasMultipleSelector = schema.properties['list.selectionstyle'] == 'multiple';

                var html = '';

                var highResolution = $(window).width() > 1199;
                var cursortype = scope.cursortype();
                var openCalendarTooltip = i18NService.get18nValue('calendar.date_tooltip', 'Open the calendar popup');

                for (var i = 0; i < datamap.length; i++) {
                    var rowst = "datamap[{0}]".format(i);

                    html += "<tr style='cursor: {0}' listtablerendered rel='hideRow'>".format(cursortype);


                    html += "<td class='select-multiple' {0}>".format(!hasMultipleSelector ? 'style="display:none"' : '');
                    html += "<input type='checkbox' ng-model=\"{0}.fields['_#selected']\">".format(rowst);
                    html += "</td>";


                    html += "<td class='select-single style\"display:none\"'>";
                    //TODO: to be implemented
                    html += "</td>";


                    var dm = datamap[i];
                    for (j = 0; j < schema.displayables.length; j++) {
                        var columnst = "columnarray[{0}]".format(j);
                        column = schema.displayables[j];
                        var attribute = column.attribute;
                        var formattedText = scope.getFormattedValue(datamap[i].fields[attribute], column, datamap);

                        if (!column.rendererParameters) {
                            column.rendererParameters = {};
                        }

                        var editable = scope.isColumnEditable(column);

                        var isHidden = hiddencolumnArray[j];

                        html += "<td {2} onclick='griditemclick({0},{1},this)' class=\"xoupscolumns\" ".format(i, j, isHidden ? 'style="display:none"' : '');
                        html += ">";
                        if (column.rendererType == 'color') {
                            var color = scope.statusColor(dm.fields[column.rendererParameters['column']] || 'null', schema.applicationName);
                            html += "<div class='statuscolumncolor' style='background-color:{0}'>".format(color);
                        } else if (column.rendererType == 'checkbox') {
                            var name = attribute;
                            html += "<div>";
                            html += "<input type='checkbox' class='check' name='{0}' ".format(name);
                            html += "ng-model=\"{0}.fields['checked']\"".format(rowst);
                            html += "ng-init=\"{0}.fields['checked']=false\" >".format(rowst);
                            needsWatchers = true;
                        } else if (column.rendererType == "datetime") {

                            if (editable) {
                                needsWatchers = true;
                                html += "<div class=\"input-group\" data-datepicker=\"true\">";
                                html += scope.appendDateTimeComponent(columnst, column.rendererParameters, attribute, openCalendarTooltip);
                            } else {
                                html += defaultAppending(formattedText);
                            }
                        }


                        else if (column.type == 'ApplicationFieldDefinition') {
                            if (!editable) {
                                html += defaultAppending(formattedText);
                            } else {
                                needsWatchers = true;
                                var maxlength = column.rendererParameters['maxlength'];
                                html += "<div class=\"input-group\" data-datepicker=\"true\">";
                                html += "<input type=\"text\" ng-model=\"{0}[column.attribute]\" class=\"hidden-phone form-control\" ".format(columnst);
                                html += "data-ng-maxlength=\"{0}\" />".format(maxlength);
                            }
                        }

                        else if (column.type == "OptionField") {
                            if (column.rendererParameters['filteronly'] == 'true') {
                                html += defaultAppending(formattedText);
                            } else {
                                if (column.rendererType == "combo") {
                                    needsWatchers = true;
                                    html += "<div class=\"sw-combobox-container\">";
                                    html += "<select class=\"hidden-phone form-control combobox\"";
                                    html += "ng-model=\"{0}.fields[column.target]\" ".format(columnst);
                                    html += " ng-options=\"option.value as i18NOptionField(option,column,schema) for option in GetAssociationOptions(column)\" ".format(columnst);
                                }
                            }
                        } else if (column.type == "ApplicationSection") {
                            var contextPath = scope.contextPath(column.resourcepath);
                            hasSection = true;
                            html += "<div>";
                            //ng-if= true is needed to create a new scope here
                            html += "<div ng-include=\"'{0}'\" href=\"#\" style=\"width: 100%\" ng-init=\"dm={1}\"  ng-if=\"'true'\">".format(contextPath, rowst);
                        }
                        html += "</div></td>";
                    }
                    html += "</tr>";
                }
                element.html(html);
                if (!$rootScope.printRequested && (hasSection || needsWatchers)) {
                    $compile(element.contents())(scope);
                }
                var t1 = new Date().getTime();
                $log.getInstance('crudtbody#link').debug('grid compilation took {0} ms'.format(t1 - t0));
                $timeout(function (key, value) {
                    scope.$emit('listTableRenderedEvent');
                    if (!$rootScope.printRequested && !needsWatchers) {
                        scope.$$watchers = null;
                    }
                });
            }



            scope.$on('sw_griddatachanged', function (event, datamap, schema) {
                scope.refreshGrid(datamap, schema);
            });





            $injector.invoke(BaseList, this, {
                $scope: scope,
                i18NService: i18NService,
                fieldService: fieldService,
                commandService: commandService
            });

            //first call when the directive is linked (listener was not yet in place)
            scope.refreshGrid(scope.datamap, scope.schema);

        }
    }


});


