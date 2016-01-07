(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('reportservice', ["$http", "alertService", "fieldService", "searchService", "redirectService", "printService", "contextService", "validationService", function ($http, alertService, fieldService, searchService, redirectService, printService, contextService, validationService) {

    var getDefaultFilterValue = function (displayable) {
        var defaultValue = null;
        if (displayable.filter.default == '@MIN_DATE') {
            try {
                defaultValue = new Date(1970, 0, 1).toString(displayable.rendererParameters.format);
            } catch (e) {
                defaultValue = new Date(1970, 0, 1).toString(getDateFormat());
            }
        } else if (displayable.filter.default == '@CURR_DATE') {
            try {
                defaultValue = new Date().toString(displayable.rendererParameters.format);
            } catch (e) {
                defaultValue = new Date().toString(getDateFormat());
            }
        } else if (!nullOrEmpty(displayable.filter.default)) {
            defaultValue = displayable.filter.default;
        }
        return defaultValue;
    }

    var getCommodityDescription = function (datamap) {
        var description = "";
        if (datamap.system != null && datamap.system != '') {
            description += datamap.system + '-';
            if (datamap.component != null && datamap.component != '') {
                description += datamap.component + '-';
                if (datamap.item != null && datamap.item != '') {
                    description += datamap.item + '-';
                    if (datamap.module != null && datamap.item != '') {
                        description += datamap.module + '-';
                    }
                }
            }
        }
        return description.length > 0 ? description.substring(0, description.length - 1) : "";
    }

    var generateSearchDTO = function (datamap, schema) {

        var filters = fieldService.getFilterDisplayables(schema.displayables);
        var searchData = {};
        var searchOperators = {};

        for (var i = 0; i < filters.length; i++) {
            var displayable = filters[i];
            var attribute = displayable.attribute;
            var operationId = displayable.filter.operation;
                
            var operation;
            if (operationId != null) {
                operation = searchService.getSearchOperationById(operationId);
                if (operation == null) {
                    operation = searchService.defaultSearchOperation();
                }
                
                if (attribute.endsWith("___start")) {
                    // set between operator on 'start' attribute
                    var betweenOperatorAttr = attribute.substring(0, attribute.length - 8);
                    searchOperators[betweenOperatorAttr] = operation;
                } else if (attribute.endsWith("___end")) {
                    // do nothing, operator was set on 'start' attribute
                } else {
                    // default case
                    searchOperators[attribute] = operation;
                }

                if (datamap[attribute] == null || datamap[attribute] == '') {
                    datamap[attribute] = getDefaultFilterValue(displayable);
                }
                searchData[attribute] = datamap[attribute];
            }
        }

        return searchService.buildSearchDTO(searchData, {}, searchOperators, {});
    }

    var generateReport = function(currentschema, searchDTO) {
        var parameters = {};

        var reportSchema = currentschema.properties["nextschema.schemaid"];
        var reportName = currentschema.properties["report.reportName"];

        parameters.searchDTO = searchDTO;        
        parameters.title = currentschema.title;
        parameters.reportName = reportName;
        parameters.application = currentschema.applicationName;
        parameters.customParameters = {};
        parameters.customParameters.title = currentschema.title;

        parameters.currentmodule = contextService.retrieveFromContext('currentmodule');
        parameters.currentmetadata = contextService.retrieveFromContext('currentmetadata');

        var reportUrl = removeEncoding(url("Report/Index?" + $.param(parameters) + "&Key.schemaId=" + reportSchema));        
        redirectService.redirectNewWindow(reportUrl, isIe9());
    }

    return {

        viewHardwareRepairReport: function (datamap, schema) {

            var description = "";
            if (schema.title.indexOf('/') != -1) {
                description = getCommodityDescription(datamap);
            }
            datamap.description = description.length > 0 ? description : null;

            //Setting Default Start Date and End Date
            if (datamap.reportdate___start == null || datamap.reportdate___start == '') {
                datamap.reportdate___start = '01/01/1970';
            }
            if (datamap.reportdate___end == null || datamap.reportdate___end == '') {
                datamap.reportdate___end = getCurrentDate();
            }

            var defaultOperator = {};
            defaultOperator["reportdate"] = searchService.getSearchOperationById('BTW');
            defaultOperator["internalpriority"] = searchService.getSearchOperationById('EQ');
            defaultOperator["status"] = searchService.getSearchOperationById('EQ');
            defaultOperator["pluspcustomer"] = searchService.getSearchOperationById('CONTAINS');
            defaultOperator["commodities_description"] = searchService.getSearchOperationById('STARTWITH');
            
            var searchDTO = searchService.buildSearchDTO({
                "reportdate___start": datamap.reportdate___start,
                "reportdate___end": datamap.reportdate___end,
                "internalpriority": datamap.internalpriority,
                "status": datamap.status,
                "pluspcustomer": datamap.pluspcustomer,
                "commodities_.description": datamap.description
            }, {}, {
                "reportdate": defaultOperator["reportdate"],
                "internalpriority": defaultOperator["internalpriority"],
                "status": defaultOperator["status"],
                "pluspcustomer": defaultOperator["pluspcustomer"],
                "commodities_description": defaultOperator["commodities_description"]
            }, {});

            generateReport(schema, searchDTO);
        },

        viewIncidentReport: function (datamap, schema) {
            //TODO: FIX THIS
            datamap["commodities_.description"] = getCommodityDescription(datamap);
            
            var searchDTO = generateSearchDTO(datamap, schema);

            if (schema.schemaId == "incidentperlocationreportfilters") {
                generateReport(schema, searchDTO);
            } else if (schema.schemaId == "incidentdetailreportfilters") {
                var reportSchema = schema.properties["nextschema.schemaid"];
                var appName = schema.name;
                var parameters = {};
                parameters.searchDTO = searchDTO;

                //TODO Improve this solution
                contextService.insertReportSearchDTO(reportSchema, searchDTO);

                redirectService.goToApplicationView(appName, reportSchema, null, schema.title, parameters);
            }            
        },

        viewITCReport: function (datamap, schema) {

            var validationErrors = validationService.validate(schema.displayables, datamap);
            if (validationErrors.length > 0) {
                //interrupting here, can´t be done inside service
                return;
            }

            var searchDTO = generateSearchDTO(datamap, schema);
            searchDTO.searchSort = "resppartygroup";

            generateReport(schema, searchDTO);
        },

        selectAll: function (schema, datamap) {

            for (var i = 0; i < datamap.length; i++) {
                datamap[i].fields['checked'] = true;

            }
        },

        deselectAll: function (schema, datamap) {
            for (var i = 0; i < datamap.length; i++) {
                datamap[i].fields['checked'] = false;
            }
        },

        printSelected: function (schema, datamap) {
            printService.printDetailedList(schema, datamap);
        }
    };
}]);

})(angular);