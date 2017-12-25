(function (angular) {
    "use strict";

    angular.module('sw_layout').directive('columnWidths', function ($log, $timeout) {
        "ngInject";

        var log = $log.getInstance('sw4.columnwidthcss', ['style']);

        return {
            scope: {
                schema: '=',
                inline: '=',
                tableMetadata: '=',
            },
            link: function (scope, element, attr) {
                log.debug('Render Listgrid CSS');

                const buildWidth = function (displayable) {
                    if (displayable.isHidden || !displayable.hasOwnProperty('attribute')) return null;

                    //new row object
                    const row = {};
                    if (displayable.rendererParameters) {
                        const width = removePercent(displayable.rendererParameters.width);

                        //use provided width or default to 0
                        if (width) {
                            row.width = width;
                        } else {
                            row.width = 0;
                        }
                    } else {
                        row.width = 0;
                    }

                    if (displayable.attribute) {
                        row.class = safeCSSselector(displayable.attribute);
                    } else {
                        row.class = '';
                    }

                    return row;
                }

                const buildCss = function (widths) {
                    //log.debug('Widths Found', widths);

                    //balance remaining width between missing column widths
                    balanceColumns(widths, 'width');

                    log.debug('Widths Found', widths);

                    //build css rules
                    var css = '';
                    const tableAttr = scope.tableMetadata ? scope.tableMetadata.attribute : null;
                    css += getViewRules(widths, 'width', '768px', 'screen', scope.schema, tableAttr);
                    css += buildMinWidthCSS(scope.schema, scope.inline, scope.schema.properties['list.width.min']);
                    css += getViewRules(widths, 'width', '1px', 'print', scope.schema, tableAttr);

                    if (css) {
                        //log.debug(css);

                        //output css rules to html
                        element.html(css);
                    } else {
                        log.debug('No CSS Generated');
                    }
                }

                scope.$watch('schema', function () {
                    //scope.counter = scope.counter + 1;
                    log.debug('schema', scope.schema);

                    if (scope.tableMetadata) return;

                    //log.debug(scope.schema.displayables);

                    $timeout(function () {
                        if (typeof scope.schema == 'undefined' || typeof scope.schema.displayables == 'undefined') {
                            //for dashboards the data is lazy-loaded
                            log.debug('return');
                            return;
                        }

                        const json = scope.schema.displayables;
                        //log.debug('Raw Data', json);

                        const widths = {};
                        var column;
                        //build object for columns and responsive widths
                        for (let id in json) {
                            if (!json.hasOwnProperty(id)) {
                                continue;
                            }

                            //convert metadata to html columns (add 2 for select columns and 1 for index base)
                            if (scope.schema.stereotype === 'List') {
                                column = parseInt(id) + 3;
                            } else {
                                column = parseInt(id) + 1;
                            }

                            //log.debug(json[id]);

                            //if the column has rendererParameters, else default to 0 width
                            let displayable = json[id];


                            //new row object
                            const row = buildWidth(displayable);
                            if (row) {
                                widths[column] = row;
                            }
                        }

                        buildCss(widths);
                    }, 0, false);
                });

                scope.$watch('tableMetadata', function () {
                    $timeout(function () {
                        if (!scope.tableMetadata || !scope.tableMetadata.rows || scope.tableMetadata.rows.length === 0) {
                            return;
                        }

                        const metadataRow = scope.tableMetadata.rows[0];
                        if (!metadataRow || metadataRow.length === 0) return;

                        const widths = {};
                        //build object for columns and responsive widths
                        metadataRow.forEach((displayable, column) => {
                            //new row object
                            const row = buildWidth(displayable);
                            if (row) {
                                widths[column] = row;
                            }
                        });


                        buildCss(widths);
                    }, 0, false);
                });
            }
        }
    });

    function balanceColumns(widths, param) {

        const totalColumns = Object.keys(widths).length;
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
                    const currentWidth = val[param];
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

    function getViewRules(widths, param, viewWidth, media, schema, tableAttr) {
        var newCSS = '';

        //look for the viewWidth in each column
        for (let column in widths) {
            if (!widths.hasOwnProperty(column)) continue;
            if (widths[column][param]) {
                const columnWidth = widths[column][param];

                //get the css rule & add it other rules
                if (columnWidth) {
                    const temp = getCSSrule(column, widths[column]['class'], columnWidth, schema, tableAttr);
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

    function getCSSrule(columnIndex, columnClass, columnWidth, schema, tableAttr) {
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

        return buildCSSrule(columnIndex, columnClass, properties, schema, tableAttr);
    }

    window.getCSSrule = getCSSrule;

    function buildCSSrule(columnIndex, columnClass, properties, schema, tableAttr) {
        if (!schema.properties['list.nowrap']) {
            return buildCSSselector(columnIndex, columnClass, 'th', schema) + ',' + buildCSSselector(columnIndex, columnClass, 'td', schema, false, tableAttr) + '{' + properties + '}';
        } else {
            if (properties.indexOf('px') > 0) {
                return buildCSSselector(columnIndex, columnClass, 'th', schema) + ',' + buildCSSselector(columnIndex, columnClass, 'td', schema, true, tableAttr) + '{' + properties + '}';
            }
        }
    }

    window.buildCSSrule = buildCSSrule;

    function buildCSSselector(columnIndex, columnClass, element, schema, targetWrapper, tableAttr) {
        const gridtype = getGridType(schema);
        var selector = `#${gridtype}[data-application="${schema.applicationName}"][data-schema="${schema.schemaId}"]`;

        if (tableAttr) {
            selector += `[data-attribute="${safeCSSselector(tableAttr)}"]`;
        }

        //if css class found, build selector using class, else use nth-child as a fallback
        if (columnClass) {
            selector += ` ${element}.${columnClass}`;
        } else {
            selector += ` ${element}:nth-child(${columnIndex})`;
        }

        if (targetWrapper) {
            selector += ' .cell-wrapper';
        }

        return selector;
    }

    window.buildCSSselector = buildCSSselector;

    function buildMinWidthCSS(schema, inline, minWidth) {


        var css = '';
        if (inline) {
            return css;
        }

        if (!minWidth) {
            minWidth = '800px';
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
        if (schema.stereotype === 'List') {
            return 'listgrid';
        } else if (schema.stereotype === 'CompositionList') {
            return 'compositionlistgrid';
        }
        return "crudtable";
    }

    window.getGridType = getGridType;

    function removePercent(value) {
        if (typeof value === 'string' && value.indexOf('px') > 0) {
            return value;
        }

        if (value) {
            let size = parseInt(value.replace('%', ''));

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