﻿
(function (angular) {
    'use strict';

    function layoutservice($log,fieldService) {
      

        function getDefaultColumnClassesForFieldSet(datamap, schema, displayables, params) {
            const log = $log.getInstance("layoutservice#getDefaultColumnClassesForFieldSet", ["layout"]);
            var maxColumns = CalculateMaxNumberOfColumns(datamap, schema, displayables, params);

            //if the field has a defined inputsize, override the section setting
            if (params.columnCount) {
                maxColumns = params.columnCount;
            }

            //use full-width fields on xsmall screen
            var classes = ' col-xs-' + 12;
            classes += ' col-xsp-' + 12 / maxColumns;


            if (maxColumns === 1) {
                //don't add any other column class for full-width fields

                //make sure full-width field appear on a newline (FF bug fix)
                classes += ' newrow';
            } else {
                //add the small screen columns
                classes += ' col-sm-' + 12 / maxColumns;

                //calculate the medium screen columns
                classes += ' col-md-' + 12 / maxColumns;
            }

            return { classes, maxColumns };
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
                let inputSize = '';

                if (fieldMetadata.rendererParameters['inputsize'] != null) {
                    inputSize = fieldMetadata.rendererParameters['inputsize'];
                }

                columnCount = convertInputSizeToColumnCount(inputSize);
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
                    columnCount = 12;
                    break;
                case 'xsmall':
                    columnCount = 6;
                    break;
                case 'small':
                    columnCount = 4;
                    break;
                case 'medium':
                    columnCount = 3;
                    break;
                case 'large':
                    columnCount = 2;
                    break;
                case 'xlarge':
                    columnCount = 1;
                    break;
                default:
            }

            return columnCount;
        };

        function hasLabelOnTop(fieldMetadata) {
            return fieldMetadata.rendererType !== "label" && !(fieldMetadata.rendererType === "checkbox" && !!fieldMetadata.rendererParameters["layout"]);
        }

        function getFieldClass(fieldMetadata, datamap, schema, displayables, params = {}) {

            var cssclass = "";
            if (fieldMetadata.rendererParameters && fieldMetadata.rendererParameters["class"]) {
                cssclass =  fieldMetadata.rendererParameters["class"] + " ";
            }

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
            if (fieldMetadata.type === "ApplicationSection") {
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

            if (fieldMetadata.resourcepath != null) {
                cssclass += ' resourcesection';
            }

            //add class if section is a tab section
            if (fieldMetadata.displayables != null && fieldMetadata.rendererParameters["tabsection"] === "true") {
                cssclass += " tabsection";
            }

            if (hasLabelOnTop(fieldMetadata)) {
                //this class will set a min-height of 60px, which shouldn´t be put if the labels are not on top
                cssclass += " inputfield";
            }

            //add classes if childinputsize is set
            if (fieldMetadata.rendererParameters != null && fieldMetadata.rendererParameters.childinputsize != null) {
                cssclass += ' childinputsize ' + fieldMetadata.rendererParameters.childinputsize;
            }

            params.columnCount = getFieldColumnCount(fieldMetadata);

            const result = getDefaultColumnClassesForFieldSet(datamap, schema, displayables, params);

            cssclass += result.classes;
            cssclass += ' row';

            //if the field is a checkbox with a layout
            if (fieldMetadata.rendererType === 'checkbox') {
                if (fieldMetadata.rendererParameters.layout === 'left' || fieldMetadata.rendererParameters.layout === 'right') {
                    if (params.sectionparameters && params.sectionparameters.headerSection) {
                        cssclass += ' inline-checkbox-header';
                    } else {
                        cssclass += ' inline-checkbox';
                        if (result.maxColumns > 1) {
                            cssclass += ' inline-checkbox-side-by-side';
                        }
                    }
                }

            }

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

            if (fieldMetadata.rendererType === 'checkbox') {
                //console.log(fieldMetadata.rendererParameters.layout);
                //console.log(fieldMetadata, datamap, schema, displayables, params);
                if (fieldMetadata.rendererParameters.layout === 'left' || fieldMetadata.rendererParameters.layout === 'right') {
                    return '';
                }
            }

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

        function getCheckboxLabelLeftClass(fieldMetadata) {
            if (fieldMetadata.rendererParameters.layout !== 'right') {
                return 'ng-hide';
            }
        }

        function getCheckboxLabelRightClass(fieldMetadata) {
            if (fieldMetadata.rendererParameters.layout !== 'left') {
                return 'ng-hide';
            }
        }


        function hasSameLineLabel (fieldMetadata) {
            return (fieldMetadata.header != null && fieldMetadata.header.displacement != 'ontop') ||
            (fieldMetadata.header == null);

        };


        const service = {
            getFieldClass,
            getInputClass,
            getLabelClass,
            hasSameLineLabel,
            getCheckboxLabelLeftClass,
            getCheckboxLabelRightClass
        };

        return service;
    }

    angular
      .module('sw_layout')
      .service('layoutservice', ['$log', "fieldService", layoutservice]);

})(angular);