var app = angular.module('sw_layout');




app.factory('layoutservice', function (fieldService) {

    function getMaxNumberOfColumns(datamap, schema, displayables, verticalOrientation) {
        if (verticalOrientation) {
            return 1;
        }
        return fieldService.countVisibleDisplayables(datamap, schema, displayables);
    };

    function getDefaultColumnClassesForFieldSet(datamap, schema, displayables, verticalOrientation) {
        var maxColumns = getMaxNumberOfColumns(datamap, schema, displayables, verticalOrientation);
        var classes = " col-xs-12";
        if (maxColumns == 1) {
            return classes;
        }
        classes += " col-md-" + 12 / maxColumns;
        classes += " col-sm-6" ;
        return classes;
    };

    function getDefaultColumnClassesForLabel(datamap, schema, displayables, verticalOrientation) {
        var maxColumns = getMaxNumberOfColumns(datamap, schema, displayables, verticalOrientation);
        var classes = " col-xs-12";
        if (maxColumns == 1) {
            classes += " singlelinelabel";
            classes += " col-md-2";
            return classes;
        }
        classes += " col-md-" + 2 * maxColumns; //50%
        classes += " col-sm-6";
        return classes;
    };

    function getDefaultColumnClassesForInput(datamap, schema, displayables, verticalOrientation) {
        var maxColumns = getMaxNumberOfColumns(datamap, schema, displayables, verticalOrientation);
        var classes = " col-xs-12";
        if (maxColumns == 1) {
            classes += " col-md-10";
            return classes;
        }
        classes += " col-md-" + (12 - (2 * maxColumns));
        classes += " col-sm-6";
        return classes;
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

            if (fieldMetadata.rendererParameters != null) {
                //make sure the fieldset is the correct width
                switch (fieldMetadata.rendererParameters['inputsize']) {
                    case 'xsmall':
                        cssclass += ' col-xs-12 col-sm-6 col-md-3';
                        break;
                    case 'small':
                        cssclass += ' col-xs-12 col-sm-6 col-md-4';
                        break;
                    case 'medium':
                        cssclass += ' col-xs-12 col-sm-6';
                        break;
                    default:
                        cssclass += getDefaultColumnClassesForFieldSet(datamap, schema, displayables, isVerticalOrientation);
                }
            }
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



            if (this.hasSameLineLabel(fieldMetadata)) {
                switch (fieldMetadata.rendererParameters['inputsize']) {
                    case 'xsmall':
                        cssclass += ' col-sm-4';
                        break;
                    case 'small':
                        cssclass += ' col-sm-6';
                        break;
                    case 'medium':
                        cssclass += ' col-sm-6 col-md-8';
                        break;
                    default:
                        cssclass += getDefaultColumnClassesForInput(datamap, schema, displayables, isVerticalOrientation);
                }
            } else {
                //                cssclass += ' col-sm-9 col-md-10';
                cssclass += this.getDefaultColumnClassesForInput(datamap, schema, displayables, isVerticalOrientation);
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

            if (this.hasSameLineLabel(fieldMetadata)) {
                switch (fieldMetadata.rendererParameters['inputsize']) {
                    case 'xsmall':
                        returnClass += 'col-sm-8';
                        break;
                    case 'small':
                        returnClass += 'col-sm-6';
                        break;
                    case 'medium':
                        returnClass += 'col-sm-6 col-md-4';
                        break;
                    default:
                        returnClass += getDefaultColumnClassesForLabel(datamap, schema, displayables, isVerticalOrientation);
                }
            } else {
                returnClass += getDefaultColumnClassesForLabel(datamap, schema, displayables, isVerticalOrientation);
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