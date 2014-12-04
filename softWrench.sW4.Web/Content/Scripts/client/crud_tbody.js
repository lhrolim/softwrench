var app = angular.module('sw_layout');

function testclick(rowNumber, columnNumber, element) {
    //this is a trick to call a angular scope function from an ordinary onclick listener (same used by batarang...)
    //with this, we can generate the table without compiling it to angular, making it faster
    //first tests pointed a 100ms gain, but need to gather more data.
    var scope = angular.element(element).scope();
    scope.showDetail(scope.datamap[rowNumber], scope.schema.displayables[columnNumber]);
}

app.directive('crudtbody', function (contextService, $compile, $parse, formatService, i18NService, fieldService, commandService, $injector, $timeout, $log) {
    return {
        restrict: 'A',
        replace: false,
        scope: {
            datamap: '=',
            schema: '=',
        },
        template: "",
        link: function (scope, element, attrs) {

            scope.getFormattedValue = function (value, column) {
                var formattedValue = formatService.format(value, column);
                if (formattedValue == "-666") {
                    //this magic number should never be displayed! 
                    //hack to make the grid sortable on unions, where we return this -666 instead of null, but then remove this from screen!
                    return "";
                }
                return formattedValue == null ? "" : formattedValue;
            };

            scope.getGridColumnStyle = function (column, propertyName) {
                var property = column.rendererParameters[propertyName];

                if (property != null) {
                    return property;
                }

                if (propertyName == 'maxwidth') {
                    var high = $(window).width() > 1199;
                    if (high) {
                        return '135px';
                    }
                    return '100px';
                }
                return null;
            }

            scope.statusColor = function (status, gridname) {
                if (status.equalsAny("NEW", "WAPPR", "WSCH")) {
                    return "orange";
                }
                if (status.equalsAny("QUEUED", "INPROG", "null")) {
                    return "yellow";
                }

                if (status.equalsAny("CANCELLED", "FAIL", "CAN", "FAILPIR", "REJECTED", "NOTREQ")) {
                    return "red";
                }

                if (status.equalsAny("RESOLVED", "SLAHOLD", "SCHED", "APPR", "AUTHORIZED", "AUTH", "HOLDINPRG", "INPRG", "PLANNED", "ACC_CAT", "ASSESSES")) {
                    return "blue";
                }
                if (status.equalsAny("CLOSED", "RESOLVCONF", "IMPL", "REVIEW", "CLOSE", "HISTEDIT", "COMP", "INPRG", "PLANNED")) {
                    return "green";
                }
                if (status.equalsAny("DRAFT")) {
                    return "white";
                }
                return "transparent";
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
                var hasCheckBox = false;

                var html = '';
                for (var i = 0; i < datamap.length; i++) {
                    var rowst = "datamap[{0}]".format(i);
                    html += "<tr style='cursor: pointer' listtablerendered rel='hideRow'>";
                    var dm = datamap[i];
                    for (j = 0; j < schema.displayables.length; j++) {
                        var columnst = "columnarray[{0}]".format(j);
                        column = schema.displayables[j];
                        var formattedText = scope.getFormattedValue(datamap[i].fields[column.attribute], column);

                        var minwidthDiv = scope.getGridColumnStyle(column, 'minwidth');
                        var maxwidthDiv = scope.getGridColumnStyle(column, 'maxwidth');
                        var widthDiv = scope.getGridColumnStyle(column, 'width');

                        var minWidth = column.rendererParameters['minwidth'];
                        var maxWidth = column.rendererParameters['maxwidth'];
                        var width = column.rendererParameters['width'];
                        html += "<td {2} onclick='testclick({0},{1},this)'".format(i, j, hiddencolumnArray[j] ? 'style="display:none"' : '');
                        if (i == 0) {
                            html += "style=\" minwidth:{0};maxwidth:{1};width:{2} \" >".format(minWidth, maxWidth, width);
                        } else {
                            html += ">";
                        }
                        if (column.rendererType == 'color') {
                            var color = scope.statusColor(dm.fields[column.rendererParameters['column']] || 'null', null);
                            html += "<div class='statuscolumncolor' style='background-color:{0}'>".format(color);
                        } else if (column.rendererType == 'checkbox') {
                            var name = column.attribute;
                            html += "<div>";
                            html += "<input type='checkbox' class='check' name='{0}' ".format(name);
                            html += "ng-model=\"{0}.fields['checked']\"".format(rowst);
                            html += "ng-init=\"{0}.fields['checked']=false\" >".format(rowst);
                            html += "</div>";
                            hasCheckBox = true;
                        }else if (column.type == 'ApplicationFieldDefinition') {
                            html += "<div class='gridcolumnvalue'".format(columnst);
                            if (i == 0) {
                                html += "style=\" minwidth:{0};maxwidth:{1};width:{2} \" ".format(minwidthDiv, maxwidthDiv, widthDiv);
                            }
                            html += ">";
                            html += formattedText;
                        }
                     
                        html += "</div>";
                        html += "</td>'";
                    }
                    html += "</tr>";
                }
                element.html(html);
                if (hasCheckBox) {
                    $compile(element.contents())(scope);
                }
                var t1 = new Date().getTime();
                $log.getInstance('crudtbody#link').debug('grid compilation took {0} ms'.format(t1 - t0));
                $timeout(function (key, value) {
                    scope.$emit('listTableRenderedEvent');
                    if (!hasCheckBox) {
                        scope.$$watchers = null;
                    }
                });
            }

            scope.$on('sw_griddatachanged', function (event, datamap, schema) {
                scope.refreshGrid(datamap, schema);
            });




            $injector.invoke(BaseController, this, {
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


