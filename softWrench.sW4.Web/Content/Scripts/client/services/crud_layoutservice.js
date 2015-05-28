var app = angular.module('sw_layout');




app.factory('layoutservice', function (fieldService) {

    function getMaxNumberOfColumns(datamap, schema, displayables, verticalOrientation) {
        if (verticalOrientation) {
            return 1;
        }

        var countColumns = fieldService.countVisibleDisplayables(datamap, schema, displayables);

        //ensure that no more than 4 inputs are placed on one row
        if (countColumns > 4) {
            countColumns = 4;
        }

        return countColumns
    };

    function getDefaultColumnClassesForFieldSet(datamap, schema, displayables, verticalOrientation, inputSize) {
        var maxColumns = 0;

        //use the provided inputSize, else calculate the number of columns
        if (inputSize) {
            maxColumns = inputSize;
        } else {
            maxColumns = getMaxNumberOfColumns(datamap, schema, displayables, verticalOrientation);
        }

        //use full-width fields on xsmall screen
        var classes = ' col-xs-12';

        if (maxColumns == 1) {
            //don't add any other column class for full-width fields

            //make sure full-width field appear on a newline (FF bug fix)
            classes += ' newrow';
        } else {
            //add the small screen columns
            classes += ' col-sm-6';

            //calculate the medium screen columns
            classes += ' col-md-' + 12 / maxColumns;
        }

        return classes;
    };

    function getDefaultColumnClassesForLabel(datamap, schema, displayables, verticalOrientation, inputSize) {
        var maxColumns = 0;

        //use the provided inputSize, else calculate the number of columns
        if (inputSize) {
            maxColumns = inputSize;
        } else {
            maxColumns = getMaxNumberOfColumns(datamap, schema, displayables, verticalOrientation);
        }

        //use full-width fields on xsmall screen, label above the input
        var classes = ' col-xs-12';

        //calculate the small screen columns
        if (maxColumns == 1) {
            classes += ' col-sm-3';
        } else {
            classes += ' col-sm-6';
        }

        //calculate the medium screen columns
        classes += ' col-md-' + 2 * maxColumns;

        return classes;
    };

    function getDefaultColumnClassesForInput(datamap, schema, displayables, verticalOrientation, inputSize) {
        var maxColumns = 0;

        //use the provided inputSize, else calculate the number of columns
        if (inputSize) {
            maxColumns = inputSize;
        } else {
            maxColumns = getMaxNumberOfColumns(datamap, schema, displayables, verticalOrientation);
        }

        //use full-width fields on xsmall screen, input below the label
        var classes = ' col-xs-12';

        //calculate the small screen columns
        if (maxColumns == 1) {
            classes += ' col-sm-9';
        } else {
            classes += ' col-sm-6';
        }

        //calculate the medium screen columns
        classes += ' col-md-' + (12 - (2 * maxColumns));

        return classes;
    };

    function convertInputSizeToColumnCount(fieldMetadata) {
        var columnCount = null;

        if (fieldMetadata.rendererParameters != null && fieldMetadata.rendererParameters['inputsize'] != null) {
            switch (fieldMetadata.rendererParameters['inputsize']) {
                case 'xsmall':
                    columnCount = 4;
                    break;
                case 'small':
                    columnCount = 3;
                    break;
                case 'medium':
                    columnCount = 2;
                    break;
                case 'large':
                    columnCount = 1;
                    break;
            }
        }

        return columnCount;
    };

    return {

        getFieldClass: function (fieldMetadata, datamap, schema, displayables, isVerticalOrientation) {
            var cssclass = "";

            if (fieldMetadata.rendererParameters != null && fieldMetadata.rendererParameters['fieldclass'] != null) {
                cssclass += fieldMetadata.rendererParameters['fieldclass'];
            } else {
                if (fieldMetadata.schema != null &&
                fieldMetadata.schema.rendererParameters != null &&
                fieldMetadata.schema.rendererParameters['fieldclass'] != null) {
                    cssclass += fieldMetadata.schema.rendererParameters['fieldclass'];
                }
            }

            if (fieldMetadata.rendererParameters != null && fieldMetadata.header != null) {
                cssclass += ' hasheader';
            }

            if (fieldMetadata.displayables != null) {
                cssclass += ' haschildren';
            }

            var columnCount = convertInputSizeToColumnCount(fieldMetadata);
            cssclass += getDefaultColumnClassesForFieldSet(datamap, schema, displayables, isVerticalOrientation, columnCount);

            cssclass += ' row';

            return cssclass;
        },


        getInputClass: function (fieldMetadata, datamap, schema, displayables, isVerticalOrientation) {
            var cssclass = "";

            if (fieldMetadata.type == 'ApplicationSection') {
                return ' col-xs-12';
            }

            if (fieldMetadata.rendererParameters != null && fieldMetadata.rendererParameters['inputclass'] != null) {
                cssclass += fieldMetadata.rendererParameters['inputclass'];
            } else {
                if (fieldMetadata.schema != null &&
                fieldMetadata.schema.rendererParameters != null &&
                fieldMetadata.schema.rendererParameters['inputclass'] != null) {
                    cssclass += fieldMetadata.schema.rendererParameters['inputclass'];
                }
            }

            if (fieldMetadata.resourcepath != undefined && fieldMetadata.header == null) {
                cssclass += ' col-md-12';
                return cssclass;
            }

            var columnCount = convertInputSizeToColumnCount(fieldMetadata);

            if (this.hasSameLineLabel(fieldMetadata)) {
                cssclass += getDefaultColumnClassesForInput(datamap, schema, displayables, isVerticalOrientation, columnCount);
            } else {
                cssclass += ' col-xs-12';
            }

            return cssclass;
        },

        getLabelClass: function (fieldMetadata, datamap, schema, displayables, isVerticalOrientation) {
            var cssclass = "";

            if (fieldMetadata.rendererParameters != null && fieldMetadata.rendererParameters['labelclass'] != null) {
                cssclass += fieldMetadata.rendererParameters['labelclass'];
            } else {
                if (fieldMetadata.schema != null &&
                fieldMetadata.schema.rendererParameters != null &&
                fieldMetadata.schema.rendererParameters['labelclass'] != null) {
                    cssclass += fieldMetadata.schema.rendererParameters['labelclass'];
                }
            }

            if (fieldMetadata.resourcepath != undefined && fieldMetadata.header == null) {
                return null;
            }

            var returnClass = '';
            var columnCount = convertInputSizeToColumnCount(fieldMetadata);

            if (this.hasSameLineLabel(fieldMetadata)) {
                returnClass += getDefaultColumnClassesForLabel(datamap, schema, displayables, isVerticalOrientation, columnCount);
            } else {
                returnClass += 'col-xs-12';
            }

            //fix SWWEB-732, blank lable adding extra space
            if (returnClass == 'col-xs-12' && fieldMetadata.label === null) {
                returnClass = cssclass + ' ' + returnClass + ' ng-hide';
            }

            return cssclass + ' ' + returnClass;
        },

        hasSameLineLabel: function (fieldMetadata) {


            return (fieldMetadata.header != null && fieldMetadata.header.displacement != 'ontop') ||
            (fieldMetadata.header == null);

        }
    }
});