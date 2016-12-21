(function (angular) {
    "use strict";

angular.module('sw_layout').directive('columnWidths', function ($log, $timeout) {
    "ngInject";

    var log = $log.getInstance('sw4.columnwidthcss');

    return {
        scope: {
            schema: '='     
        },
        link: function (scope, element, attr) {
            log.debug('Render Listgrid CSS');

            scope.$watch('schema', function () {
                //scope.counter = scope.counter + 1;
                log.debug('schema', scope.schema);

                //log.debug(scope.schema.displayables);

                $timeout(function () {
                    if (typeof scope.schema == 'undefined' || typeof scope.schema.displayables == 'undefined') {
                        //for dashboards the data is lazy-loaded
                        log.debug('return');
                        return;
                    }

                    var json = scope.schema.displayables;
                    //log.debug('Raw Data', json);

                    var widths = {}
                    var column;
                    //build object for columns and responsive widths
                    for (var id in json) {
                        if(!json.hasOwnProperty(id)) continue;
                        //convert metadata to html columns (add 2 for select columns and 1 for index base)
                        if (scope.schema.stereotype === 'List') {
                            column = parseInt(id) + 3;
                        } else {
                            column = parseInt(id) + 1;
                        }

                        //log.debug(json[id]);

                        //new row object
                        var row = {};

                        //if the column has rendererParameters, else default to 0 width
                        if (!json[id].isHidden && json[id].hasOwnProperty('attribute')) {
                            if (json[id].rendererParameters) {
                                var width = removePercent(json[id].rendererParameters.width);

                                //use provided width or default to 0
                                if (width) {
                                    row.width = width;
                                } else {
                                    row.width = 0;
                                }
                            } else {
                                row.width = 0;
                            }

                            if (json[id].attribute) {
                                row.class = safeCSSselector(json[id].attribute);
                            } else {
                                row.class = '';
                            }

                            widths[column] = row;
                        }
                    }

                    //log.debug('Widths Found', widths);

                    //balance remaining width between missing column widths
                    balanceColumns(widths, 'width');

                    log.debug('Widths Found', widths);

                    //build css rules
                    var css = '';
                    css += getViewRules(widths, 'width', '768px', 'screen', scope.schema);
                    css += buildMinWidthCSS(scope.schema, scope.schema.properties['list.width.min']);
                    css += getViewRules(widths, 'width', '1px', 'print', scope.schema);

                    if (css) {
                        //log.debug(css);

                        //output css rules to html
                        element.html(css);
                    } else {
                        log.debug('No CSS Generated');
                    }
                }, 0, false);
            });
        }
    }
});

function balanceColumns(widths, param) {

    var totalColumns = Object.keys(widths).length;
    var totalWidth = 0;
    var withWidth = 0;

    //total all the column widths
    angular.forEach(widths, function (val) {
        if (val[param] === -1) {
            withWidth++;
        } else if (val[param] > 0) {
            totalWidth = totalWidth + val[param];
            withWidth++;
        }
    });
    
    var remainingWidth, balanceWidth;
    //if there are columns without widths assigned
    if (withWidth < totalColumns) {

        //if the total metadata widths are less than 100 calc the remainder, else fallback to equal width columns
        if (totalWidth <= 100) {
            remainingWidth = 100 - totalWidth;
            balanceWidth = remainingWidth / (totalColumns - withWidth);
        } else {
            remainingWidth = 0;
            balanceWidth = 100 / totalColumns;
        }

        //update the columns without widths
        angular.forEach(widths, function (val) {
            if (remainingWidth > 0) {
                var currentWidth = val[param];
                if (currentWidth === 0) {
                    val[param] = balanceWidth;
                }
            } else {
                val[param] = balanceWidth;
            }
        });
    }
}

window.balanceColumns = balanceColumns;

function getViewRules(widths, param, viewWidth, media, schema) {
    var newCSS = '';

    //look for the viewWidth in each column
    for (var column in widths) {
        if (!widths.hasOwnProperty(column)) continue;
        if (widths[column][param]) {
            var columnWidth = widths[column][param];

            //get the css rule & add it other rules
            if (columnWidth) {
                var temp = getCSSrule(column, widths[column]['class'], columnWidth, schema);
                if (temp) {
                    newCSS = newCSS + temp;
                }
            }
        }
    }

    //if a viewWidth is supplied, create a media query
    if (viewWidth) {
        if (newCSS) {
            newCSS = '@media ' + media + ' and (min-width: ' + viewWidth + ') {' + newCSS + '} ';
        }
    }

    return newCSS;
}

window.getViewRules = getViewRules;

function getCSSrule(columnIndex, columnClass, columnWidth, schema) {
    var properties = '';

    if (columnWidth) {
        //-1 hide this column, else set width and show
        if (columnWidth === -1) {
            properties = '';
        } else if (typeof columnWidth === 'string' && columnWidth.indexOf('px') > 0) {
            properties = 'width:' + columnWidth;
        } else {
            properties = 'width:' + columnWidth + '%;';
        }
    }

    return buildCSSrule(columnIndex, columnClass, properties, schema);
}

window.getCSSrule = getCSSrule;

function buildCSSrule(columnIndex, columnClass, properties, schema) {
    if (!schema.properties['list.nowrap']) {
        return buildCSSselector(columnIndex, columnClass, 'th', schema) + ',' + buildCSSselector(columnIndex, columnClass, 'td', schema) + '{' + properties + '}';
    } else {
        if (properties.indexOf('px') > 0) {
            return buildCSSselector(columnIndex, columnClass, 'th', schema) + ',' + buildCSSselector(columnIndex, columnClass, 'td', schema, true) + '{' + properties + '}';
        }
    }
}

window.buildCSSrule = buildCSSrule;

function buildCSSselector(columnIndex, columnClass, element, schema, targetWrapper) {
    var gridtype = getGridType(schema),
        selector;

    //if css class found, build selector using class, else use nth-child as a fallback
    if (columnClass) {
        selector = '#' + gridtype + '[data-application="' + schema.applicationName + '"][data-schema="' + schema.schemaId + '"] ' + element + '.' + columnClass;
    } else {
        selector = '#' + gridtype + '[data-application="' + schema.applicationName + '"][data-schema="' + schema.schemaId + '"] ' + element + ':nth-child(' + columnIndex + ')';
    }

    if (targetWrapper) {
        selector += ' .cell-wrapper';
    }

    return selector;
}

window.buildCSSselector = buildCSSselector;

function buildMinWidthCSS(schema, minWidth) {
    var css = '';

    if (!minWidth) {
        minWidth = '1024px';
    }

    //build listgrid min-width rules
    css += '@media screen and (min-width: 769px) {';
    css += '#' + getGridType(schema) + '[data-application="' + schema.applicationName + '"][data-schema="' + schema.schemaId + '"] {';
    css += 'min-width: ' + minWidth;
    css += '}';
    css += '}';

    return css;
}

window.buildMinWidthCSS = buildMinWidthCSS;

function getGridType(schema) {
    var gridtype;
    if (schema.stereotype === 'List') {
        gridtype = 'listgrid';
    } else if (schema.stereotype === 'CompositionList') {
        gridtype = 'compositionlistgrid';
    }

    return gridtype;
}

window.getGridType = getGridType;

function removePercent(value) {
    if (typeof value === 'string' && value.indexOf('px') > 0) {
        return value;
    }

    if (value) {
        var size = parseInt(value.replace('%', ''));

        //if size is not a number
        if (isNaN(size)) {
            size = 0;
        }

        return size;
    } else {
        return 0;
    }
}

window.removePercent = removePercent;

})(angular);