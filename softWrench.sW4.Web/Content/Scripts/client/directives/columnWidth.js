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

                    //get widths from metadata
                    var width = json[id].rendererParameters.width;
                    var widthXS = json[id].rendererParameters.widthXS;
                    var widthSM = json[id].rendererParameters.widthSM;
                    var widthMD = json[id].rendererParameters.widthMD;
                    var widthLG = json[id].rendererParameters.widthLG;

                    //new row object
                    var row = {};
                    
                    //if width is set override responsive widths
                    if (width) {
                        row['width'] = width;
                    } else { //else add responsive width
                        if (widthXS) {
                            row['widthXS'] = widthXS;
                        }

                        if (widthSM) {
                            row['widthSM'] = widthSM;
                        }

                        if (widthMD) {
                            row['widthMD'] = widthMD;
                        }

                        if (widthLG) {
                            row['widthLG'] = widthLG;
                        }
                    }

                    //if widths found add row to the object
                    if (width || widthXS || widthSM || widthMD || widthLG) {
                        widths[column] = row;
                    }
                }

                log.debug(widths);
                var css = '';

                //build css rules
                for (column in widths) {
                    //log.debug(widths[row]);

                    if (widths[column].width) {
                        columnWidth = widths[column].width;
                    } 
                    else if (widths[column].widthXS) {
                        columnWidth = widths[column].widthXS;
                    }

                    //get the css rule & add it other rules
                    if (columnWidth) {
                        var temp = getCSSrule(column, columnWidth);
                        if (temp) {
                            css = css + temp;
                        }
                    }
                }

                css = css + '@media all and (min-width: 480px) {';
                for (column in widths) {
                    //log.debug(widths[row]);

                    if (widths[column].widthSM) {
                        columnWidth = widths[column].widthSM;

                        //get the css rule & add it other rules
                        var temp = getCSSrule(column, columnWidth);
                        if (temp) {
                            css = css + temp;
                        }
                    }
                }
                css = css + '}';

                css = css + '@media all and (min-width: 768px) {';
                for (column in widths) {
                    //log.debug(widths[row]);

                    if (widths[column].widthMD) {
                        columnWidth = widths[column].widthMD;

                        //get the css rule & add it other rules
                        var temp = getCSSrule(column, columnWidth);
                        if (temp) {
                            css = css + temp;
                        }
                    }
                }
                css = css + '}';

                css = css + '@media all and (min-width: 992px) {';
                for (column in widths) {
                    //log.debug(widths[row]);

                    if (widths[column].widthLG) {
                        columnWidth = widths[column].widthLG;

                        //get the css rule & add it other rules
                        var temp = getCSSrule(column, columnWidth);
                        if (temp) {
                            css = css + temp;
                        }
                    }
                }
                css = css + '}';

                log.debug(css);

                //output css rules to html
                element.html(css);
            });
        }
    }
});

function getCSSrule(column, columnWidth) {
    if (columnWidth) {
        if (columnWidth === '0%') {
            css = '#listgrid th:nth-child(' + column +
                '), #listgrid td:nth-child(' + column +
                ') {display: none;}';
        } else {
            css = '#listgrid th:nth-child(' + column +
                '), #listgrid td:nth-child(' + column +
                ') {width: ' + columnWidth + '; display: table-cell;}';
        }
    }

	return css;
}
