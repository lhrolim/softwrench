var app = angular.module('sw_layout');

//var PRINTMODAL_$_KEY = '[data-class="printModal"]';


app.factory('excelService', function ($rootScope, $http, $timeout, $log, tabsService, fixHeaderService,
    i18NService,userService,
    redirectService, searchService, contextService, fileService, alertService,restService) {

    "ngInject";

    function needsRegionSelection(mode) {
        if (mode != "assetlistreport") {
            return false;
        }
        var isInAllAccessModules = contextService.InModule(["tom", "itom", "purchase", "assetcontrol"]);
        if (isInAllAccessModules) {
            return true;
        }
        

        var isXitc = contextService.InModule(["xitc"]);
        return isXitc && userService.InGroup("C-HLC-WW-AR-WW");
    }

    function regionSelectionRequired(searchData,region) {
        var hasSearchInUrl = sessionStorage.swGlobalRedirectURL && sessionStorage.swGlobalRedirectURL.indexOf("searchValues")!=-1;
        return isObjectEmpty(searchData) && !region && !hasSearchInUrl;
    }


    return {
        showModalExportToExcel: function (fn, schema, searchData, searchSort, searchOperator, paginationData) {
            //TODO: fix this crap
            var modalTitle = i18NService.get18nValue('_exportotoexcel.export', 'Export') + " " + schema.applicationName +
                " " + i18NService.get18nValue('_exportotoexcel.toexcel', 'to excel');
            var selectRegionText = i18NService.get18nValue('_exportotoexcel.selectregion', 'Select Region');

            var selectText = i18NService.get18nValue('_exportotoexcel.selectexportmode', 'Select export mode');
            var exportModeGridOnlyText = i18NService.get18nValue('_exportotoexcel.gridonly', 'Grid only');
            var exportModeAllTheColumnsText = i18NService.get18nValue('_exportotoexcel.allthecolumns', 'Asset List');
            var exportModeCategoriesText = i18NService.get18nValue('_exportotoexcel.categories', 'Categories');

            bootbox.dialog({
                message: "<form id='infos' action=''>" +
                    "<label class='control-label'>" + selectText + ":</label><br>" +
                    "<label class='control-label' for='gridonlyid'>" +
                    "<input type='radio' name='exportMode' id='gridonlyid' value='list' checked='checked' /> "
                    + exportModeGridOnlyText +
                    "</label><br>" +
                    "<label class='control-label' for='allthecolumnsid'>" +
                    "<input type='radio' name='exportMode' id='allthecolumnsid' value='assetlistreport' /> "
                    + exportModeAllTheColumnsText +
                    "</label>" +
                    "</form>",
                title: modalTitle,
                buttons: {
                    main: {
                        label: i18NService.get18nValue('_exportotoexcel.export', 'Export'),
                        className: "btn-primary",
                        callback: function (result) {
                            if (result) {
                                var exportModeSelected = $('input[name=exportMode]:checked', '#infos').val();
                                if (needsRegionSelection(exportModeSelected)) {
                                    //HAP-944
                                    handleAssetListReportRegionFilter(fn);
                                    return;
                                }

                                fn(schema, searchData, searchSort, searchOperator, paginationData, exportModeSelected);
                            }
                        }
                    },
                    cancel: {
                        label: i18NService.get18nValue('_exportotoexcel.cancel', 'Cancel'),
                        className: "btn btn-default",
                        callback: function () {
                            return null;
                        }
                    }

                },
                className: "hapag-modal-exporttoexcel"
            });

            function handleAssetListReportRegionFilter(finalCbk) {



                bootbox.dialog({
                    message: "<form id='region' action=''>" +
                        "<label class='control-label'>" + selectRegionText + ":</label><br>" +
                        "<label class='control-label' for='gridonlyid'>" +
                        "<input type='radio' name='region' id='gridonlyid' value='C-HLC-WW-RG-NAMERICA'  /> "
                            + "Region North America" +
                        "</label><br>" +
                        "<label class='control-label' for='gridonlyid'>" +
                        "<input type='radio' name='region' id='gridonlyid' value='C-HLC-WW-RG-SAMERICA' /> "
                            + "Region South America" +
                        "</label><br>" +
                        "<label class='control-label' for='gridonlyid'>" +
                        "<input type='radio' name='region' id='gridonlyid' value='C-HLC-WW-RG-EUROPE'  /> "
                            + "Region Europe" +
                        "</label><br>" +
                       "<input type='radio' name='region' id='gridonlyid' value='C-HLC-WW-RG-ASIA'  /> "
                            + "Region Asia" +
                        "</label><br>" +
                        "<input type='radio' name='region' id='gridonlyid' value='C-HLC-WW-RG-GSC'  /> "
                            + "Region GSC" +
                        "</label><br>" +
                          "<input type='radio' name='region' id='gridonlyid' value='C-HLC-WW-RG-HQ'  /> "
                            + "Region HeadQuarters" +
                        "</label><br>" +
                        "</form>",
                    title: selectRegionText,
                    buttons: {
                        main: {
                            label: i18NService.get18nValue('_exportotoexcel.export', 'Export'),
                            className: "btn-primary",
                            callback: function (result) {

                                if (result) {
                                    var region = $('input[name=region]:checked', '#region').val();
                                    if (regionSelectionRequired(searchData, region)) {
                                        alertService.alert("Please select either a Region or a filter on the grid");
                                        return;
                                    }
                                    var metadataParameter = region ? "region={0}".format(region) : null;
                                    finalCbk(schema, searchData, searchSort, searchOperator, paginationData, 'assetlistreport', metadataParameter);
                                }
                            }
                        },
                        cancel: {
                            label: i18NService.get18nValue('_exportotoexcel.cancel', 'Cancel'),
                            className: "btn btn-default",
                            callback: function () {
                                return null;
                            }
                        }

                    },
                    className: "hapag-modal-exporttoexcel"
                });
            }
        },


        exporttoexcel: function (schema, searchData, searchSort, searchOperator, paginationData, schemaSelected, parameterQuery) {
            var application = schema.applicationName;
            if (contextService.isClient("hapag") && application == "asset" && !schemaSelected) {
                var fn = this.exporttoexcel;
                this.showModalExportToExcel(fn, schema, searchData, searchSort, searchOperator, paginationData);
                return null;
            }
            var schemaToUse = schemaSelected ? schemaSelected : schema.schemaId;

            var parameters = {};
            parameters.key = {};
            parameters.key.schemaId = schemaToUse;
            parameters.key.mode = schema.mode;
            parameters.key.platform = "web";
            parameters.application = application;
            parameters.currentmodule = contextService.retrieveFromContext('currentmodule');
            parameters.currentmetadata = contextService.retrieveFromContext('currentmetadata');
            parameters.currentmetadataparameter = parameterQuery;
            var searchDTO;
            var reportDto = contextService.retrieveReportSearchDTO(schema.schemaId);
            if (reportDto != null) {
                reportDto = $.parseJSON(reportDto);
                searchDTO = searchService.buildReportSearchDTO(reportDto, searchData, searchSort, searchOperator, paginationData.filterFixedWhereClause, paginationData.unionFilterFixedWhereClause);
            } else {
                searchDTO = searchService.buildSearchDTO(searchData, searchSort, searchOperator, paginationData.filterFixedWhereClause, paginationData.unionFilterFixedWhereClause);
            }

            var currentModule = contextService.currentModule();
            if (currentModule != null) {
                parameters.module = currentModule;
            }
            searchDTO.pageNumber = 1;
            searchDTO.totalCount = paginationData.totalCount;
            searchDTO.pageSize = paginationData.pageSize;

            parameters.searchDTO = searchDTO;
            //this quick wrapper ajax call will validate if the user is still logged in or not
            restService.getPromise("ExtendedData", "PingServer").then(function() {
                fileService.download(url("/Excel/Export" + "?" + $.param(parameters)), function(html, url) {}, function(html, url) {
                    alertService.alert("Error generating the {0}.{1} report. Please contact your administrator".format(application, schemaToUse));
                });
            });

        }
    }

});

