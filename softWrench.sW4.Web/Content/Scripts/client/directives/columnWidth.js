var app = angular.module('sw_layout');

app.directive('columnWidths', function ($log) {
    var log = $log.getInstance('sw4.columnwidthcss');

    return {
        link: function (scope, element, attr) {
            log.debug('Render Listgrid CSS');
     
            //look for user changing modules
            attr.$observe('columnWidths', function (value) {

                //convert string column data to object column data
                var json = angular.fromJson(value);
                var widths = {}

                //build object for columns and responsive widths
                for (id in json) {
                    //convert metadata to html columns (add 2 for select columns and 1 for index base)
                    var column = parseInt(id) + 3;

                    //new row object
                    var row = {};

                    log.debug('css', id, json[id]);

                    //if the column has rendererParameters, else defaul to 0 width
                    //log.debug(json[id]);

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

                        widths[column] = row;
                    }
                }

                log.debug('Widths Found', widths);

                //balance remaining width between missing column widths
                balanceColumns(widths, 'width');
                balanceColumns(widths, 'widthXS');
                balanceColumns(widths, 'widthSM');
                balanceColumns(widths, 'widthMD');
                balanceColumns(widths, 'widthLG');

                //build css rules
                var css = getViewRules(widths, 'width', null);
                css = css + getViewRules(widths, 'widthXS', '1px');
                css = css + getViewRules(widths, 'widthSM', '480px');
                css = css + getViewRules(widths, 'widthMD', '768px');
                css = css + getViewRules(widths, 'widthLG', '992px');

                if (css) {
                    //log.debug(css);

                    //output css rules to html
                    element.html(css);
                } else {
                    log.debug('No CSS Generated');
                }
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

        console.log('CSS', param, 'totalWidth', totalWidth, 'remainingWidth', remainingWidth, 'totalColumns', totalColumns, 'without Width', totalColumns - withWidth, 'balanceWidth', balanceWidth);

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

function getCSSrule(column, columnWidth) {
    if (columnWidth) {

        //-1 hide this column, else set width and show
        if (columnWidth === -1) {
            css = '#listgrid th:nth-child(' + column +
                '), #listgrid td:nth-child(' + column +
                ') {display: none;}';
        } else {
            css = '#listgrid th:nth-child(' + column +
                '), #listgrid td:nth-child(' + column +
                ') {width: ' + columnWidth + '%; display: table-cell;}';
        }
    }

	return css;
}

function getViewRules(widths, param, viewWidth) {
    var newCSS = '';

    //look for the viewWidth in each column
    for (column in widths) {
        if (widths[column][param]) {
            columnWidth = widths[column][param];

            //get the css rule & add it other rules
            if (columnWidth) {
                var temp = getCSSrule(column, columnWidth);
                if (temp) {
                    newCSS = newCSS + temp;
                }
            }
        }
    }

    //if a viewWidth is supplied, create a media query
    if (viewWidth) {
        if (newCSS) {
            newCSS = '@media all and (min-width: ' + viewWidth + ') {' + newCSS + '} ';
        }
    }

    return newCSS;
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