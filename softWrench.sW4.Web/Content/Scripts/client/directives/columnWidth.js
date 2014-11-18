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

                    width = removePercent(json[id].rendererParameters.width);
                                
                    //if width is set override responsive widths
                    if (width) {
                        row.width = width;
                        row.widthXS = width;
                        row.widthSM = width;
                        row.widthMD = width;
                        row.widthLG = width;
                    } else { //else add responsive width
                        row.width = width;
                        row.widthXS = removePercent(json[id].rendererParameters.widthXS);
                        row.widthSM = removePercent(json[id].rendererParameters.widthSM);
                        row.widthMD = removePercent(json[id].rendererParameters.widthMD);
                        row.widthLG = removePercent(json[id].rendererParameters.widthLG);
                    }

                    widths[column] = row;
                }

                
                //check if widths found
                //if (Object.keys(widths).length > 0) {
                    log.debug('Widths Found', widths);

                //build css rules
                var css = getViewRules(widths, 'width', null);
                css = css + getViewRules(widths, 'widthXS', '1px');
                css = css + getViewRules(widths, 'widthSM', '480px');
                css = css + getViewRules(widths, 'widthMD', '768px');
                css = css + getViewRules(widths, 'widthLG', '992px');

                //}
                //else {
                //    log.debug('No widths found for', json.length, 'columns');

                //    var width = 100 / json.length;
                //    var columnWidth = Math.floor(width) + '%';

                //    css = getCSSrule(null, columnWidth);
                //}

                if (css) {
                    log.debug(css);

                    //output css rules to html
                    element.html(css);
                } else {
                    log.debug('No CSS Generated');
                }
            });
        }
    }
});

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

        if (isNaN(size)) {
            size = 0;
        }

        return size;
    } else {
        return 0;
    }
}