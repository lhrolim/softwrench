var app = angular.module('sw_layout');

app.directive('columnWidths', function ($log, $timeout) {
    var log = $log.getInstance('sw4.columnwidthcss');

    return {
        link: function (scope, element, attr) {
            log.debug('Render Listgrid CSS');

            //look for user changing modules
            attr.$observe('columns', function (value) {

                $timeout(function () {

                    if (value == "") {
                        //for dashboards the data is lazy-loaded
                        return;
                    }
                    //convert string column data to object column data
                    var json = angular.fromJson(value);
                  

                    log.debug('Raw Data', json);

                    var widths = {}

                    //build object for columns and responsive widths
                    for (id in json) {
                        //convert metadata to html columns (add 2 for select columns and 1 for index base)
                        if (attr.gridtype === 'listgrid') {
                            var column = parseInt(id) + 3;
                        } else {
                            var column = parseInt(id) + 1;
                        }

                        //new row object
                        var row = {};

                        //if the column has rendererParameters, else default to 0 width
                        if (!json[id].isHidden) {
                            if (json[id].rendererParameters) {
                                width = removePercent(json[id].rendererParameters.width);

                                //if width is set override responsive widths, else add responsive widths
                                if (width) {
                                    row.width = width;
                                    row.widthXS = width;
                                    row.widthSM = width;
                                    row.widthMD = width;
                                    row.widthLG = width;
                                } else {
                                    row.width = width;
                                    row.widthXS = removePercent(json[id].rendererParameters.widthXS);
                                    row.widthSM = removePercent(json[id].rendererParameters.widthSM);
                                    row.widthMD = removePercent(json[id].rendererParameters.widthMD);
                                    row.widthLG = removePercent(json[id].rendererParameters.widthLG);
                                }
                            } else {
                                row.width = 0;
                                row.widthXS = 0;
                                row.widthSM = 0;
                                row.widthMD = 0;
                                row.widthLG = 0;
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
                    balanceColumns(widths, 'widthXS');
                    balanceColumns(widths, 'widthSM');
                    balanceColumns(widths, 'widthMD');
                    balanceColumns(widths, 'widthLG');

                    log.debug('Widths Found', widths);

                    //build css rules
                    var css = '';
                    css += getViewRules(widths, 'width', null, 'screen', attr);
                    css += getViewRules(widths, 'widthXS', '1px', 'screen', attr);
                    css += getViewRules(widths, 'widthSM', '480px', 'screen', attr);
                    css += getViewRules(widths, 'widthMD', '768px', 'screen', attr);
                    css += getViewRules(widths, 'widthLG', '992px', 'screen', attr);
                    css += getViewRules(widths, 'widthLG', '1px', 'print', attr);

                    if (css) {
                        log.debug(css);
                        log.debug(attr.applicationname);

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
    for (column in widths) {
        if (widths[column][param] === -1) {
            withWidth++;
        } else if (widths[column][param] > 0) {
            totalWidth = totalWidth + widths[column][param];
            withWidth++;
        }
    }

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
        for (column in widths) {
            if (remainingWidth > 0) {
                currentWidth = widths[column][param];
                if (currentWidth === 0) {
                    widths[column][param] = balanceWidth;
                }
            } else {
                widths[column][param] = balanceWidth;
            }
        }
    }
}

function getViewRules(widths, param, viewWidth, media, attr) {
    var newCSS = '';

    //look for the viewWidth in each column
    for (column in widths) {
        if (widths[column][param]) {
            columnWidth = widths[column][param];

            //get the css rule & add it other rules
            if (columnWidth) {
                var temp = getCSSrule(column, widths[column]['class'], columnWidth, attr);
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

function getCSSrule(columnIndex, columnClass, columnWidth, attr) {
    var properties = '';

    if (columnWidth) {
        //-1 hide this column, else set width and show
        if (columnWidth === -1) {
            properties = 'display:none;';
        } else {
            properties = 'width:' + columnWidth + '%;display:table-cell;';
        }
    }

    return buildCSSrule(columnIndex, columnClass, properties, attr);
}

function buildCSSrule(columnIndex, columnClass, properties, attr) {
    return buildCSSselector(columnIndex, columnClass, 'th', attr) + ',' + buildCSSselector(columnIndex, columnClass, 'td', attr) + '{' + properties + '}'
}

function buildCSSselector(columnIndex, columnClass, element, attr) {

    //if css class found, build selector using class, else use nth-child as a fallback
    if (columnClass) {
        return '#' + attr.gridtype + '[data-application="' + attr.applicationname + '"] ' + element + '.' + columnClass;
    } else {
        return '#' + attr.gridtype + '[data-application="' + attr.applicationname + '"] ' + element + ':nth-child(' + columnIndex + ')';
    }
}

function removePercent(value) {
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