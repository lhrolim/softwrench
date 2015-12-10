
(function (angular) {
    'use strict';

    function layoutservice($log,fieldService) {
      

        function getDefaultColumnClassesForFieldSet(datamap, schema, displayables, params) {

            var maxColumns = CalculateMaxNumberOfColumns(datamap, schema, displayables, params);

            var log = $log.getInstance("layoutservice#getDefaultColumnClassesForFieldSet", ["layout"]);

            //log.debug(params);

            //console.log(params);

            //use full-width fields on xsmall screen
            var classes = ' col-xs-12';

            if (maxColumns === 1) {
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

        function getDefaultMaxNumberOfColumns(datamap, schema, displayables, verticalOrientation) {
            if (verticalOrientation) {
                return 1;
            }

            var countColumns = fieldService.countVisibleDisplayables(datamap, schema, displayables);

            //ensure that no more than 4 inputs are placed on one row
            if (countColumns > 6) {
                countColumns = 6;
            }

            return countColumns;
        };

        function getFieldColumnCount(fieldMetadata) {
            var columnCount = null;

            if (fieldMetadata.rendererParameters != null) {
                var inputSize = '';

                if (fieldMetadata.rendererParameters['inputsize'] != null) {
                    inputSize = fieldMetadata.rendererParameters['inputsize'];
                } 

                columnCount = convertInputSizeToColumnCount(inputSize)
            }

            return columnCount;
        };

        function CalculateMaxNumberOfColumns(datamap, schema, displayables, params) {
            var maxColumns = null;

            if (params.sectionparameters != null && params.sectionparameters.childinputsize != null) {
                if (params.sectionparameters.inputsize != null) {
                    maxColumns = 1;
                } else {
                    maxColumns = convertInputSizeToColumnCount(params.sectionparameters.childinputsize);
                }
            } else if (params.columnCount != null) {
                maxColumns = params.columnCount;
            } else {
                maxColumns = getDefaultMaxNumberOfColumns(datamap, schema, displayables, params.isVerticalOrientation);
            }

            return maxColumns;
        };

        function convertInputSizeToColumnCount(inputSize) {
            var columnCount = null;

            switch (inputSize) {
                case 'xxsmall':
                    columnCount = 6;
                    break;
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
                default:
            }

            return columnCount;
        };


        function getFieldClass (fieldMetadata, datamap, schema, displayables, params) {
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

            //if section has inputsize render parameter
            if (fieldMetadata.type == "ApplicationSection") {
                if (fieldMetadata.rendererParameters != null && fieldMetadata.rendererParameters.inputsize != null) {
                    cssclass += ' sidebyside';
                }                
            }

            //add class if section has header
            if (fieldMetadata.rendererParameters != null && fieldMetadata.header != null) {
                cssclass += ' hasheader';
            }

            //add class if section has children
            if (fieldMetadata.displayables != null) {
                cssclass += ' haschildren';
            }

            //add classes if childinputsize is set
            if (fieldMetadata.rendererParameters != null && fieldMetadata.rendererParameters.childinputsize != null) {
                cssclass += ' childinputsize ' + fieldMetadata.rendererParameters.childinputsize;
            }

            params.columnCount = getFieldColumnCount(fieldMetadata);

            cssclass += getDefaultColumnClassesForFieldSet(datamap, schema, displayables, params);
            cssclass += ' row';

            return cssclass;
        };

        function getInputClass (fieldMetadata, datamap, schema, displayables, params) {
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

            cssclass += ' col-xs-12';

            return cssclass;
        };

        function getLabelClass (fieldMetadata, datamap, schema, displayables, params) {
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
            params.columnCount = getFieldColumnCount(fieldMetadata);

            returnClass += 'col-xs-12';

            //fix SWWEB-732, blank lable adding extra space
            if (returnClass == 'col-xs-12' && fieldMetadata.label === null) {
                returnClass = cssclass + ' ' + returnClass + ' ng-hide';
            }

            return cssclass + ' ' + returnClass;
        };

        function hasSameLineLabel (fieldMetadata) {
            return (fieldMetadata.header != null && fieldMetadata.header.displacement != 'ontop') ||
            (fieldMetadata.header == null);

        };


        var service = {
            getFieldClass: getFieldClass,
            getInputClass:getInputClass,
            getLabelClass:getLabelClass,
            hasSameLineLabel:hasSameLineLabel
        };

        return service;
    }

    angular
      .module('sw_layout')
      .factory('layoutservice', ['$log', "fieldService", layoutservice]);

})(angular);


