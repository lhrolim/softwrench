var app = angular.module('sw_layout');

app.factory('tabsService', function (fieldService,i18NService) {

    "ngInject";

    var nonInlineCompositionsDict = function (schema) {
        if (schema.nonInlineCompositionsDict != undefined) {
            //caching
            return schema.nonInlineCompositionsDict;
        }
        var resultDict = {};
        for (var i = 0; i < schema.nonInlineCompositionIdxs.length; i++) {
            var idx = schema.nonInlineCompositionIdxs[i];
            var composition = schema.displayables[idx];
            resultDict[composition.relationship] = composition;
        }
        schema.nonInlineCompositionsDict = resultDict;
        return resultDict;
    };

    var getCompositionSchema = function (baseSchema, compositionKey, schemaId) {
        var schemas = nonInlineCompositionsDict(baseSchema);
        var thisSchema = schemas[compositionKey];
        schemas = thisSchema.schema.schemas;
        return schemaId == "print" ? schemas.print : schemas.list;
    };

    var getCompositionIdName = function (baseSchema, compositionKey, schemaId) {
        return getCompositionSchema(baseSchema, compositionKey, schemaId).idFieldName;
    };


    var buildTabObjectForPrint = function (datamap, tabSchema, schema) {
        var result = {};        

        result.items = [];
        result.items.push(datamap);
        result.schema = tabSchema;
        result.title = i18NService.getTabLabel(tabSchema, schema);

        return result;
    };

    return {

        hasTabs: function (schema) {
            if (schema.hasTabs != undefined) {
                //cache
                return schema.hasTabs;
            }
            var length = this.tabsDisplayables(schema).length;
            schema.hasTabs = length > 0;
            return length;
        },

        tabsDisplayablesForPrinting: function (schema, datamap) {
            if (schema.tabsDisplayablesForPrinting != undefined) {
                //cache
                return schema.tabsDisplayablesForPrinting;
            }
            var resultList = [];
            var displayables = this.tabsDisplayables(schema);
            $.each(displayables, function (key, displayable) {
                var value = datamap.fields[displayable.relationship];
                if (value != undefined && value.length > 0) {
                    resultList.push(displayable);
                }
            });
            schema.tabsDisplayablesForPrinting = resultList;
            return resultList;
        },

        ///Returns a list of all the tabs of the passed schema
        tabsDisplayables: function (schema) {
            if (schema.tabsDisplayables != undefined) {
                //cache
                return schema.tabsDisplayables;
            }
            var resultList = [];
            var idxArray = [];
            idxArray = idxArray.concat(schema.nonInlineCompositionIdxs);
            idxArray = idxArray.concat(schema.tabsIdxs);
            idxArray.sort(function (a, b) {
                return a - b;
            });

            for (var i = 0; i < idxArray.length; i++) {
                var idx = idxArray[i];
                var displayable = schema.displayables[idx];
                if (!displayable.isHidden) {
                    resultList.push(displayable);
                }
            }
            //cache
            schema.tabsDisplayables = resultList;
            return resultList;
        },

        nonInlineCompositionsDict: function (schema) {
            return nonInlineCompositionsDict(schema);
        },

        /*
        * param notExpansible = array of compositions that we do not need to hit the server, since they are not expandable
        *
        *
        * Returns a string in the same format the server expects for expanding the compositions on the ExtendedDataController.ExpandCompositions method
        *
        */
        buildCompositionsToExpand: function (compositionsToExpandObj, schema, datamap, schemaId, notExpansible, printMode) {
            var resultString = "";
            if (compositionsToExpandObj == null) {
                return "";
            }
            $.each(compositionsToExpandObj, function (key, obj) {
                if (obj.value == false) {
                    return;
                }

                var displayable = fieldService.getDisplayableByKey(schema, key);

                if (fieldService.isTab(displayable)) {
                    notExpansible.push({ key: key, tabObject: buildTabObjectForPrint(datamap, displayable, schema) });
                    return;
                }

                var compositionData = datamap.fields[key];

                if (compositionData == undefined) {
                    //this happens when the composition data has not been fetch yet,due to a lazy strategy
                    resultString += key + "=lazy,,,";
                    return;
                }

                //now, we are retrieving data for printing
                var currentSchema = getCompositionSchema(schema, key, obj.schema);
                if (currentSchema.properties.expansible != undefined && currentSchema.properties.expansible == "false") {
                    if (notExpansible != undefined && compositionData.length > 0) {
                        //only adding if there´s actual at least one element of this nonExpansible composition
                        notExpansible.push({ key: key, schema: currentSchema });
                    }
                    return;
                }
                var printSchema = getCompositionSchema(schema, key, schemaId);
                if (printMode && printSchema.schemaId == currentSchema.schemaId) {
                    notExpansible.push({ key: key, schema: currentSchema });
                    return;
                }


                var compositionIdField = getCompositionIdName(schema, key, schemaId);
                var compositionIdArray = [];

                for (var i = 0; i < compositionData.length; i++) {
                    var composition = compositionData[i];
                    compositionIdArray.push(composition[compositionIdField]);
                }
                if (compositionIdArray.length > 0) {
                    resultString += key + "=" + compositionIdArray.join(",") + ",,,";
                }
            });
            if (resultString != "") {
                resultString = resultString.substring(0, resultString.length - 3);
            }
            return resultString;
        },

        locatePrintSchema: function (baseSchema, compositionKey) {
            var schemas = nonInlineCompositionsDict(baseSchema);
            var thisSchema = schemas[compositionKey];
            return thisSchema.schema.schemas.print;
        },

        getTitle: function (baseSchema, compositionKey) {
            var schemas = nonInlineCompositionsDict(baseSchema);
            var thisSchema = schemas[compositionKey];
            return thisSchema.label;
        }

    };
});


