(function (angular, _) {
    "use strict";

    angular.module("sw_mobile_services").factory("offlineSchemaService", ["$log", "fieldService", "schemaService", "securityService", "dispatcherService", offlineSchemaService]);

    function offlineSchemaService($log, fieldService, schemaService, securityService, dispatcherService) {

        const dateFormat = "MMM DD";
        const dateFormatHours = "MMM DD hh a";
        const dateFormatMinutes = "MMM DD hh:mm a";

        function loadDetailSchema(currentListSchema, currentApplication, selectedItem) {
            var detailSchemaId = "detail";
            var schemaDetailService = schemaService.getProperty(currentListSchema, "list.click.service");
            var overridenSchema =  !!schemaDetailService && !!selectedItem
                                    ? dispatcherService.invokeServiceByString(schemaDetailService, [currentApplication, currentListSchema, selectedItem]) 
                                    : schemaService.getProperty(currentListSchema, "list.click.schema");
            if (overridenSchema) {
                detailSchemaId = overridenSchema;
            }
            return this.locateSchema(currentApplication, detailSchemaId);
        };

        function locateSchema(application, schemaId) {
            var schemasList = application.data.schemasList;

            for (var i = 0; i < schemasList.length; i++) {
                var schema = schemasList[i];
                if ("list".equalsIc(schema.stereotype)) {
                    //build dict cache
                    buildQualifierCache(schema);
                }
                if (schema.schemaId.equalsIc(schemaId)) {
                    return schema;
                }
            }
            return null;
        };


        function buildQualifierCache(schema) {
            schema.jscache = schema.jscache || {};
            if (schema.jscache.griddisplayables) {
                //already cached
                return;
            }
            schema.jscache.qualifiercache = {};
            var displayables = schema.displayables;
            for (var i = 0; i < displayables.length; i++) {
                var displayable = displayables[i];
                if (displayable.qualifier) {
                    schema.jscache.qualifiercache[displayable.qualifier] = displayable;
                }
            }
        };

        function locateSchemaByStereotype(application, stereotype) {
            var schemasList = application.data.schemasList;

            for (var i = 0; i < schemasList.length; i++) {
                var schema = schemasList[i];
                if (schema.stereotype == "list") {
                    //build dict cache
                    buildQualifierCache(schema);
                }
                if (stereotype.equalsIc(schema.stereotype)) {
                    return schema;
                }
                else if ("detail".equalsIc(stereotype) && schema.schemaId.equalsIc("detail")) {
                    return schema;
                }
            }
            return null;
        };

        function setValueIfNotPresent(item, field, value) {
            if (!item[field]) {
                item[field] = value;
            }
        }

        function fillDefaultOfflineValues(schema, item) {
            var user = securityService.currentFullUser();
            // TODO: handle case where no default value is supposed to be used (client forcefully regitered empty default value)
            setValueIfNotPresent(item, "siteid", user["SiteId"]);
            setValueIfNotPresent(item, "orgid", user["OrgId"]);
        };

        function fillDefaultValues(schema, item, parent) {
            var scope = !!parent ? { previousdata: { fields: parent }, parentdata: { fields: parent } } : {};
            fieldService.fillDefaultValues(schema.displayables, item, scope);
            fillDefaultOfflineValues(schema, item);
        };

        function locateDisplayableByQualifier(schema, qualifier) {
            if (schema == null) {
                return null;
            }

            schema.jscache = schema.jscache || {};
            if (schema.jscache.qualifiercache) {
                //already cached
                return schema.jscache.qualifiercache[qualifier];
            }
            buildQualifierCache(schema);
            const item = schema.jscache.qualifiercache[qualifier];
            return item ? item.attribute : null;
        }

        function formatDate(dateString, showHours, showMinutes) {
            if (!dateString) {
                return null;
            }
            const dateMoment = moment(dateString);
            if (showMinutes) {
                return dateMoment.format(dateFormatMinutes);
            }
            if (showHours) {
                return dateMoment.format(dateFormatHours);
            }
            return dateMoment.format(dateFormat);
        }

        function buildDisplayValue(schema, qualifier, item) {
            if (schema == null) {
                return null;
            }

            const displayable = locateDisplayableByQualifier(schema, qualifier);
            if (!displayable || !displayable.attribute || !item) {
                return null;
            }

            if ("featured" === qualifier && ("datetime" === displayable.dataType || "timestamp" === displayable.dataType)) {
                return formatDate(item[displayable.attribute], true, true);
            }
            if ("featured" === qualifier && "date" === displayable.dataType) {
                return formatDate(item[displayable.attribute], false, false);
            }

            if (_.contains([true, "true"], displayable.rendererParameters["showLabel"])) {
                return `${displayable.label}: ${item[displayable.attribute]}`;
            }

            return item[displayable.attribute];
        }

        function getFieldByAttribute(schema, attribute) {
            Validate.notEmpty(schema);
            const fields = schema.displayables;
            return !fields || fields.length <= 0 ? null : fields.find(f => f.attribute === attribute);
        }

        const service = {
            loadDetailSchema: loadDetailSchema,
            locateSchema: locateSchema,
            locateSchemaByStereotype: locateSchemaByStereotype,
            fillDefaultValues: fillDefaultValues,
            locateDisplayableByQualifier: locateDisplayableByQualifier,
            buildDisplayValue: buildDisplayValue,
            getFieldByAttribute: getFieldByAttribute
        };
        return service;

    }
})(angular, _);