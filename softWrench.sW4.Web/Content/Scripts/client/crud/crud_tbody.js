(function (angular) {
    "use strict";

    var app = angular.module('sw_layout');

    function griditemclick(event,rowNumber, columnAttribute, element, forceEdition) {
        //this is a trick to call a angular scope function from an ordinary onclick listener (same used by batarang...)
        //with this, we can generate the table without compiling it to angular, making it faster
        //first tests pointed a 100ms gain, but need to gather more data.
        var scope = angular.element(element).scope();


        if (scope.showDetail) {
            scope.showDetail(scope.datamap[rowNumber], columnAttribute, forceEdition);
        }
        // to process the checkbox values and select-all state from parent (crud_list) too
        scope.$root.$digest();
        event.stopImmediatePropagation();
    }


    window.griditemclick = griditemclick;

    function defaultAppending(formattedText, updatable, rowst, column, background, foreground) {
        var st = "";
        if (updatable) {
            var limit = column.rendererParameters["limit"] || null;
            if (limit) {
                st += "<div swcontenteditable >{{limitTextIfNeeded({0}.fields['{1}'], '{2}')}}".format(rowst, column.attribute, limit);
            } else {
                st += "<div swcontenteditable ng-model=\"{0}.fields['{1}']\">".format(rowst, column.attribute);
            }
        } else {
            st += '<div class="cell-wrapper"';

            if (background) {
                st += 'style="background:' + background + ';color:' + foreground + '"';
            }

            st += '>';

            if (formattedText != null) {
                st += formattedText;
            } else {
                st += '&nbsp';
            }
        }

        return st;
    }

    window.defaultAppending = defaultAppending;

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

    window.buildStyle = buildStyle;

    /// <summary>
    /// create a class based on column value
    /// </summary>
    /// <param name="column"></param>
    /// <param name="formattedText"></param>
    /// <returns type="string">html class</returns>
    function hasDataClass(column, formattedText) {
        var classString = '';

        if ((formattedText != null && formattedText != "") || column.rendererType == 'color' || column.rendererType == 'statusicons') {
            classString = 'has-data';
        } else {
            classString = 'no-data';
        }

        return classString;
    }

    window.hasDataClass = hasDataClass;

    function parseBooleanValue(attrValue) {
        return attrValue == undefined || attrValue == "" ? true : attrValue.toLowerCase() == "true";
    }

    window.parseBooleanValue = parseBooleanValue;

    app.directive('crudtbody', function (contextService, $rootScope, $compile, $parse, formatService, i18NService,
    fieldService, commandService, statuscolorService, printService, $injector, $timeout, $log, searchService, iconService, gridSelectionService, crudContextHolderService, classificationColorService, prioritycolorService) {
        "ngInject";

        return {
            restrict: 'A',
            replace: false,
            scope: {
                datamap: '=',
                schema: '=',
                panelid: '='
            },
            template: "",
            link: function (scope, element, attrs) {

                scope.selectionModel = crudContextHolderService.getSelectionModel();

                scope.cursortype = function () {
                    var editDisabled = scope.schema.properties['list.disabledetails'];
                    return "true" !== editDisabled ? "pointer" : "default";
                };

                scope.classificationColor = function (classification, gridname) {
                    return classificationColorService.getColor(classification, scope.schema.applicationName);
                };

                scope.statusColor = function (status, gridname) {
                    return statuscolorService.getColor(status, scope.schema.applicationName);
                };

                scope.handleIcon = function (iconClass, column, text, foreground, rowIdx) {
                    var iconHTML = "";
                    iconHTML += "<i class=\"fa {0}\" onclick='griditemclick(event,{1},\"{2}\",this)'".format(iconClass, rowIdx, column.attribute);

                    if (foreground) {
                        iconHTML += 'style = "color: {0}"'.format(foreground);
                    }

                    //create html tooltip with label and count
                    if (text != null) {
                        var toolTip = "<span style='white-space: nowrap;'>";
                        toolTip += column.toolTip ? column.toolTip : column.label;
                        if (column.rendererParameters && column.rendererParameters["hideValueOnTooltip"] !== "true") {
                            toolTip += ': ';
                            toolTip += text;
                        }
                        toolTip += '</span>';

                        iconHTML += " rel=\"tooltip\" data-html=\"true\" data-original-title=\"{0}\"".format(toolTip);
                    }

                    iconHTML += "></i>";
                    return iconHTML;
                }

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
                    st += " data-show-time=\"{0}\" ".format(parseBooleanValue(rendererParameters['showtime']));
                    st += " data-show-date=\"{0}\"".format(parseBooleanValue(rendererParameters['showdate']));
                    st += " data-date-format=\"{0}\"".format(rendererParameters['format']);
                    st += " data-show-meridian=\"{0}\"".format(parseBooleanValue(rendererParameters['showmeridian']));
                    st += " data-allow-past=\"{0}\"".format(parseBooleanValue(rendererParameters['allowpast']));
                    st += " data-allow-future=\"{0}\" >".format(parseBooleanValue(rendererParameters['allowfuture']));
                    st += "<span class=\"input-group-addon\" data-calendericon=\"true\" rel=\"tooltip\" ";
                    st += " data-original-title=\"{0}\" style=\"cursor: pointer;\">".format(openCalendarTooltip);
                    st += "<i class=\"datetime-class\"></i></span>";
                    return st;
                }

                scope.loadIcon = function (value, metadata) {
                    return iconService.loadIcon(value, metadata);
                };

                scope.innerLoadIcon = function (rowIndex, columnIndex) {
                    var column = scope.schema.displayables[columnIndex];
                    var row = scope.datamap[rowIndex];
                    if (!row.fields) {
                        return "";
                    }
                    return scope.loadIcon(row.fields[column.attribute], column);
                }

                scope.limitTextIfNeeded = function (text, limit) {
                    if (!limit || !text) {
                        return text;
                    }
                    var limitNumber = Number.parseInt(limit);
                    if (Number.isNaN(limitNumber) || limitNumber < 0 || limitNumber >= (text.length + 3)) {
                        return text;
                    }
                    return text.substring(0, limitNumber) + "...";
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
                    var hasMultipleSelector = schema.properties['list.selectionstyle'] === 'multiple';
                    var disableddetails = "true" === schema.properties['list.disabledetails'];

                    var selectionMode = scope.selectionModel.selectionMode;

                    var html = '';

                    var highResolution = $(window).width() > 1199;
                    var cursortype = scope.cursortype();
                    var openCalendarTooltip = i18NService.get18nValue('calendar.date_tooltip', 'Open the calendar popup');

                    //map grid columns to an object
                    var displayableObject = {};
                    schema.displayables.forEach(function (field) {
                        displayableObject[field.attribute] = field;
                    });

                    for (var i = 0; i < datamap.length; i++) {
                        var rowst = "datamap[{0}]".format(i);

                        var rowClass = 'odd';
                        if (i % 2 == 0) {
                            rowClass = 'even';
                        }

                        var rowTextColor = scope.classificationColor(datamap[i].fields.classificationid, schema.applicationName);

                        html += "<tr class='{0}' style='cursor: {1};color: {2}' listtablerendered rel='hideRow'>".format(rowClass, cursortype, rowTextColor);
                        needsWatchers = hasMultipleSelector;

                        html += "<td class='select-multiple' {0}>".format(!hasMultipleSelector ? 'style="display:none"' : '');
                        html += "<input type='checkbox' ng-model=\"{0}.fields['_#selected']\" ng-change=\"selectChanged({0}, datamap)\">".format(rowst);
                        html += "</td>";


                        html += '<td class="select-single" style="display:none">';
                        //TODO: to be implemented
                        html += '</td>';

                        //console.log(datamap[i]);

                        var dm = datamap[i];
                        for (j = 0; j < schema.displayables.length; j++) {
                            var columnst = "columnarray[{0}]".format(j);
                            column = schema.displayables[j];
                            var attribute = column.attribute;
                            var formattedText = scope.getFormattedValue(datamap[i].fields[attribute], column, datamap[i]);
                            formattedText = scope.limitTextIfNeeded(formattedText, column.rendererParameters["limit"]);

                            if (!column.rendererParameters) {
                                column.rendererParameters = {};
                            }

                            var editable = scope.isColumnEditable(column);
                            var updatable = scope.isColumnUpdatable(column);

                            var isHidden = hiddencolumnArray[j];

                            html += "<td {2} onclick='griditemclick(event,{0},\"{1}\",this)' class='{3} {4} {5}'".format(i, attribute, isHidden ? 'style="display:none"' : '', safeCSSselector(column.attribute), hasDataClass(column, formattedText), column.rendererType);
                            html += "data-title='{0}'".format(column.label);
                            html += ">";
                            if (column.rendererType === 'color') {
                                var color = scope.statusColor(dm.fields[column.rendererParameters['column']] || 'null', schema.applicationName);
                                html += "<div class='statuscolumncolor' style='background-color:{0}'>".format(color);
                            }
                            else if (column.rendererType === 'checkbox') {
                                var name = attribute;
                                html += "<div class='cell-wrapper'>";
                                if (column.rendererParameters["editable"] === "true") {
                                    html += "<input type='checkbox' class='check' name='{0}' ".format(name);
                                    html += "ng-model=\"{0}.fields['{1}']\" >".format(rowst, name);
                                } else {
                                    var field = dm.fields[attribute];
                                    var icon = field === true || field === "true" || field === 1 ? "fa-check-square-o" : "fa-square-o";
                                    html += "<i class=\"fa {0}\" />".format(icon);
                                }

                                needsWatchers = true;
                            }
                            else if (column.rendererType === "datetime") {
                                if (editable) {
                                    needsWatchers = true;
                                    html += "<div class=\"input-group\" data-datepicker=\"true\">";
                                    html += scope.appendDateTimeComponent(columnst, column.rendererParameters, attribute, openCalendarTooltip);
                                } else {
                                    html += defaultAppending(formattedText, updatable, rowst, column, null, null);
                                }
                            }
                            else if (column.rendererType === "icon") {
                                html += scope.handleIcon(scope.innerLoadIcon(i, j), column, formattedText, i);
                            }
                            else if (column.rendererType === "iconbutton") {
                                var innerIcon = scope.innerLoadIcon(i, j);

                                html += '<div>';

                                if (innerIcon) {
                                    html += '<button class="btn btn-default btn-sm">';
                                    html += scope.handleIcon(innerIcon, column, formattedText, i);
                                    html += '</button>';
                                }
                            }
                            else if (column.rendererType === 'priorityicon') {
                                var foreground = prioritycolorService.getColor(formattedText, column.rendererParameters);
                                var icon = scope.loadIcon(null, column);

                                html += '<div class="cell-wrapper">' + scope.handleIcon(icon, column, formattedText, foreground, i);
                            }
                            else if (column.rendererType === 'statusicons') {
                                html += '<div class="cell-wrapper">';
                                var iconHTML = '';

                                //create the icons
                                if (column.rendererParameters.iconcolumns) {
                                    var iconColumns = column.rendererParameters.iconcolumns.split(',');

                                    iconColumns.forEach(function(field) {
                                        var iconColumn = displayableObject[field];
                                        var value = datamap[i].fields[field];

                                        if (value) {
                                            //if not the first icon add a spacer
                                            if (iconHTML != '') {
                                                iconHTML += '&emsp;';
                                            }

                                            var foreground = null;
                                            var icon = scope.loadIcon(value, iconColumn);

                                            if (iconColumn.rendererParameters.qualifier == "priority") {
                                                foreground = prioritycolorService.getColor(value, iconColumn.rendererParameters);
                                            }

                                            iconHTML += scope.handleIcon(icon, iconColumn, value, foreground, i);
                                        }
                                    });
                                }

                                html += iconHTML;
                            }

                            else if (column.type === 'ApplicationFieldDefinition') {
                                if (!editable) {
                                    if (column.rendererType === 'statuscolor') {
                                        
                                        var background = scope.statusColor(dm.fields[column.attribute], schema.applicationName);
                                        var foreground = statuscolorService.foregroundColor(background);

                                        html += defaultAppending(formattedText, updatable, rowst, column, background, foreground);
                                    } else {
                                        html += defaultAppending(formattedText, updatable, rowst, column, null, null);
                                    }
                                } else {
                                    needsWatchers = true;
                                    var maxlength = column.rendererParameters['maxlength'];
                                    html += "<div class=\"input-group\" data-datepicker=\"true\">";
                                    html += "<input type=\"text\" ng-model=\"{0}['{1}']\" class=\"hidden-phone form-control\" ".format(columnst, column.attribute);
                                    html += "data-ng-maxlength=\"{0}\" />".format(maxlength);
                                }
                            }

                            else if (column.type == "OptionField") {
                                if (column.rendererParameters['filteronly'] == 'true') {
                                    html += defaultAppending(formattedText, updatable, rowst, column, null, null);
                                } else {
                                    if (column.rendererType == "combo") {
                                        needsWatchers = true;
                                        html += "<div class=\"sw-combobox-container\">";
                                        html += "<select class=\"hidden-phone form-control combobox\"";
                                        html += "ng-model=\"{0}.fields['{1}']\" ".format(rowst, column.target);
                                        html += " ng-options=\"option.value as i18NOptionField(option,{0},schema) for option in GetAssociationOptions({0})\" ".format(columnst);
                                    }
                                }
                            }
                            else if (column.type == "ApplicationSection") {
                                var contextPath = scope.contextPath(column.resourcepath);
                                hasSection = true;
                                html += "<div>";
                                //ng-if= true is needed to create a new scope here
                                html += "<div ng-include=\"'{0}'\" href=\"#\" style=\"width: 100%\" ng-init=\"dm={1}\"  ng-if=\"'true'\">".format(contextPath, rowst);
                            }
                            html += "</div></td>";





                        }
                        if (hasMultipleSelector && !disableddetails) {
                            html += "<td ng-show=\"selectionModel.selectionMode\" class=\"edit-detail\">";
                            html += "<i class=\"fa fa-edit\" onclick='griditemclick({0},null,this,true)' rel=\"tooltip\" data-original-title=\"View Details\" ></i>".format(i, j);
                            html += "</td>";
                        }

                        html += "</tr>";
                    }
                    element.html(html);
                    if (!$rootScope.printRequested && (hasSection || needsWatchers)) {
                        $compile(element.contents())(scope);
                    }
                    if ($rootScope.printRequested != null && $rootScope.printRequested) {
                        printService.doPrint();
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

                // called whenever a selector checkbox changes state
                // updates the buffer and possibly the selectall state
                scope.selectChanged = function (row) {
                    gridSelectionService.selectionChanged(row, scope.schema, true, scope.panelid);
                }

                scope.$on('sw_griddatachanged', function (event, datamap, schema, panelid) {
                    if (panelid === scope.panelid) {
                        scope.refreshGrid(datamap, schema);
                    }
                });

                $injector.invoke(BaseList, this, {
                    $scope: scope,
                    i18NService: i18NService,
                    fieldService: fieldService,
                    commandService: commandService,
                    searchService: searchService,
                    formatService: formatService,
                    gridSelectionService: gridSelectionService
                });
            }
        }
    });

})(angular);